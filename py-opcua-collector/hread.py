import asyncio
import os
import argparse
from datetime import datetime, timedelta

import common.logger as logging
from collector.collector import UaCollector, UaCollectorOptions
from collector.connection import UaConnection
from asyncua import Client
from common.config import parse_config
from common.helper import str_to_datetime
from processor.influxdb import InfluxDBProcessor, InfluxDBProcessorOptions
from processor.console import ConsoleProcessor

logger = logging.getLogger('py_ua_hread')

# Commandline Argument Parsing
parser = argparse.ArgumentParser()
parser.add_argument('--location', default="*", help='node location')
parser.add_argument('--type', default="*", help='node type')
parser.add_argument(
    "--limit", type=int, default=1000, help="maximum item per read"
)
parser.add_argument(
    "--starttime",
    default=None,
    help="start time, formatted as YYYY-MM-DD [HH:MM[:SS]]. Default: utc() - one day",
)
parser.add_argument(
    "--endtime",
    default=None,
    help="end time, formatted as YYYY-MM-DD [HH:MM[:SS]]. Default: utc()",
)


async def main(args):
    # init config
    diconfig = parse_config(os.getcwd())
    colter_conf = diconfig.collector_config

    async with Client(url=colter_conf["UA_SERVER_URL"]) as client:
        ua_connection = UaConnection(client)
        colter = UaCollector(
            ua_connection, diconfig.data_info_config,
            UaCollectorOptions(location=args.location, type_=args.type, item_per_sub=colter_conf["ITEM_PER_SUBSCRIBER"],
                               sample_rate=colter_conf["SAMPLE_RATE"], flush_rate=colter_conf["FLUSH_RATE"])
        )
        # colter.register(
        #     InfluxDBProcessor(InfluxDBProcessorOptions(
        #         url=colter_conf["INFLUXDB_URL"],
        #         org=colter_conf["INFLUXDB_ORG"],
        #         token=colter_conf["INFLUXDB_TOKEN"],
        #         bucket=colter_conf["INFLUXDB_BUCKET"],
        #         monitor_bucket=colter_conf["INFLUXDB_MONITOR_BUCKET"]))
        # )
        colter.register(
            ConsoleProcessor()
        )

        args.starttime = str_to_datetime(
            args.starttime, datetime.utcnow() - timedelta(days=1))
        args.endtime = str_to_datetime(args.endtime, datetime.utcnow())

        await colter.history_collect(args)


if __name__ == "__main__":
    args = parser.parse_args()

    asyncio.run(main(args))
