from typing import List
import common.logger as logging
# from urllib.parse import urlparse
from collector.collector import CollectorData, UaCollectorMetric
from common.config import DeviceConfig
from processor.base import BaseProcessor
from influxdb_client import InfluxDBClient, Point, WritePrecision
from influxdb_client.client.write_api import SYNCHRONOUS


class InfluxDBProcessorOptions:
    def __init__(self, url: str, org: str, token: str, bucket: str, monitor_bucket: str = ''):
        self.url = url
        self.org = org
        self.token = token
        self.bucket = bucket
        self.monitor_bucket = monitor_bucket

        # self._o = urlparse(url)

    # @property
    # def host(self):
    #     """The host property."""
    #     return self._o.hostname
    #
    # @property
    # def port(self):
    #     """The port property."""
    #     return self._o.port


class InfluxDBProcessor(BaseProcessor):
    """docstring for InfluxDBProcessor."""

    def __init__(self, opts: InfluxDBProcessorOptions):
        self.logger = logging.getLogger(__name__)
        self.options = opts
        self._client = InfluxDBClient(
            url=opts.url, token=opts.token, org=opts.org)
        self._write_api = self._client.write_api(write_options=SYNCHRONOUS)

    def apply(self, datum: List[CollectorData], metric):
        points: List[Point] = []

        try:
            for item in datum:
                dev_conf: DeviceConfig = item.metadata
                points.append(Point(dev_conf.name).field(
                    item.node, item.value).time(item.timestamp, WritePrecision.S))

            self._batch_write(points)

            if self.options.monitor_bucket != "" and isinstance(metric, UaCollectorMetric):
                metric_point = Point("py_collector_metric") \
                                .field("collect_rate", metric.collect_rate) \
                                .tag("location", metric.opts.location) \
                                .tag("type", metric.opts.type)

                self._write_api.write(
                    bucket=self.options.monitor_bucket, org=self.options.org, record=metric_point)

        except Exception as ex:
            self.logger.error("cannot push data to influxdb %r", ex)

    def _batch_write(self, points: List[Point]):
        i = 0
        self.logger.debug(f"batch write {len(points)}")
        while i < len(points):
            records = points[i:i+500]
            self._write_api.write(bucket=self.options.bucket,
                                  org=self.options.org, record=records)
            i += 500
