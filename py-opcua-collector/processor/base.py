from typing import List
from collector.collector import CollectorData, UaCollectorMetric


class BaseProcessor():
    """docstring for BaseProcessor."""

    def __init__(self):
        pass

    def apply(self, datum: List[CollectorData], metric):
        raise NotImplemented()
