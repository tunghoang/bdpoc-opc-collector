from typing import List
import common.logger as logging
from collector.collector import CollectorData, UaCollectorMetric
from common.config import DeviceConfig
from processor.base import BaseProcessor


class ConsoleProcessor(BaseProcessor):
    """docstring for ConsoleProcessor."""

    def __init__(self):
        self.logger = logging.getLogger(__name__)

    def apply(self, datum: List[CollectorData], metric):
        try:
            self.logger.info(f'total: {len(datum)}')
            for item in datum:
                dev_conf: DeviceConfig = item.metadata
                self.logger.info(
                    f'{dev_conf.name} {item.node} {item.value} {item.timestamp}')

        except Exception as ex:
            self.logger.error("cannot push data to influxdb %r", ex)
