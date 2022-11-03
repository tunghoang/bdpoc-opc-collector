import asyncio
import os

import common.logger as logging
from collector.collector import UaCollector, UaCollectorOptions
from collector.connection import UaConnection
from asyncua import Client
from common.config import parse_config
from processor.influxdb import InfluxDBProcessor, InfluxDBProcessorOptions

logger = logging.getLogger('py_ua_collectord')


async def main():
    # init config
    diconfig = parse_config(os.getcwd())
    colter_conf = diconfig.collector_config

    async with Client(url=colter_conf["UA_SERVER_URL"]) as client:
        ua_connection = UaConnection(client)
        colter = UaCollector(
            ua_connection, diconfig.data_info_config,
            UaCollectorOptions(item_per_sub=colter_conf["ITEM_PER_SUBSCRIBER"],
                               sample_rate=colter_conf["SAMPLE_RATE"], flush_rate=colter_conf["FLUSH_RATE"])
        )
        colter.register(
            InfluxDBProcessor(InfluxDBProcessorOptions(
                url=colter_conf["INFLUXDB_URL"],
                org=colter_conf["INFLUXDB_ORG"],
                token=colter_conf["INFLUXDB_TOKEN"],
                bucket=colter_conf["INFLUXDB_BUCKET"],
                monitor_bucket=colter_conf["INFLUXDB_MONITOR_BUCKET"]))
        )

        await colter.run()


if __name__ == "__main__":
    asyncio.run(main())
