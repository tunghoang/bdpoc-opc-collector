import common.logger as logging
import asyncio
from typing import List
from asyncua.common.subscription import DataChangeNotif

from .connection import UaConnection
from .subscriber import UaSubscriber
from common.config import DataInfoConfig, DeviceConfig, TagConfig
from common.accumulator import Buffer
from time import monotonic_ns as time


class CollectorData:
    def __init__(self, metadata, node, value, timestamp):
        self.metadata = metadata
        self.node = node
        self.value = value
        self.timestamp = timestamp


class UaCollectorMetric:
    def __init__(self):
        self.collect_rate = 0.0
        self.total_tag = 0
        self.total_subscriber = 0
        self.flush_ticks = 0
        self.last_buffered = 0


class UaCollectorOptions:
    """docstring for UaCollectorOptions."""

    def __init__(self, item_per_sub=50, sample_rate=1000, flush_rate=5000):
        self.item_per_sub = item_per_sub
        self.sample_rate = sample_rate
        self.flush_rate = flush_rate


class UaCollector:
    """docstring for UaCollector."""

    def __init__(self, connection: UaConnection, diconfig: DataInfoConfig, opts=UaCollectorOptions()):
        self.logger = logging.getLogger(__name__)
        self.conn = connection
        self.diconfig = diconfig

        self._metric = UaCollectorMetric()
        self._opts = opts
        self._processors = []
        self._subs: List[UaSubscriber] = []
        self.running = False
        self._loop = None

    def get_node(self, tag: TagConfig):
        return self.conn.c.get_node(tag.nodeid())

    async def run(self):
        if self._loop is not None:
            return self._loop

        self.logger.info("Init UaCollector")
        await self._init()

        self.logger.info("Consume data change notification")
        await self._start_subs()

        self.running = True

        self.logger.info("Run flush_loop")
        self._loop = self._flush_loop()

        await self._loop

    async def stop(self):
        if not self.running:
            return

        await self._stop_subs()
        self.running = False

    def register(self, processor):
        self._processors.append(processor)

    def un_register(self, processor):
        self._processors.remove(processor)

    async def _init(self):
        if len(self._subs) > 0:
            return

        self.logger.info("Init: Devices=%s", len(self.diconfig.devices))

        for devc in self.diconfig.devices:
            await self._init_device(devc)

        self._acc = Buffer(self._metric.total_tag *
                           (int(self._opts.flush_rate/1e3) * 3))

    def _on_data_handler(self, subr, node, val, data: DataChangeNotif):
        self._acc.add(CollectorData(metadata=subr.metadata,
                      node=node, value=val, timestamp=data.monitored_item.Value.SourceTimestamp))

    async def _new_subscriber(self):
        subr = await self.conn.subscribe(self._opts.sample_rate)

        subr.on_data(self._on_data_handler)
        self._subs.append(subr)

        return subr

    async def _init_device(self, dev_conf: DeviceConfig):
        tags = dev_conf.tags
        i = 0
        item_per_sub = self._opts.item_per_sub

        while i < len(tags):
            # TODO: add check Node exists
            items = [self.get_node(t) for t in tags[i:i+item_per_sub]]
            subr = await self._new_subscriber()
            subr.metadata = dev_conf
            i += item_per_sub
            nerr = await subr.add_items(items) or 0
            self._metric.total_tag += len(items) - nerr
            self._metric.total_subscriber += 1

    async def _start_subs(self):
        for subr in self._subs:
            await subr.start()

    async def _stop_subs(self):
        for subr in self._subs:
            await subr.stop()

    async def _flush_loop(self):
        start_time = time()

        # flush loop until stopped or completely flush all buffered data
        while self.running or not self._acc.empty():
            self.logger.debug("Enter flush_loop at %s", start_time)

            # prepare for new loop
            last_time = start_time
            items = self._acc.flush()
            start_time = time()

            # process data / flush
            for processor in self._processors:
                try:
                    processor.apply(items, self._metric)
                except Exception as ex:
                    self.logger.error("call apply of %r err: %r",
                                      type(processor).__name__, ex)

            # calculate flush tick
            flush_ticks = time() - start_time
            if flush_ticks/1e9 > (self._opts.flush_rate / 2):
                self.logger.warning(
                    "last flush loop spent too much times: %s ms for %s buffered", flush_ticks, len(items))

            # calculate wait time for next loop
            # TODO: improve this wait time at least as the same as C# version
            # we expect every loop will run at the begining of a second
            elapsed = start_time - last_time
            wait = self._opts.flush_rate - \
                int(flush_ticks / 1e6) % self._opts.flush_rate

            # calculate collect metric
            self._metric.flush_ticks = flush_ticks
            self._metric.collect_rate = len(items) / elapsed * 1e9
            self._metric.last_buffered = len(items)
            metric = self._metric

            self.logger.debug(
                f"rate={metric.collect_rate} ticks={flush_ticks/1e9} buffered={metric.last_buffered} elapsed={elapsed} wait={wait} subs={metric.total_subscriber} tags={metric.total_tag}")

            # if flush_ticks / 1e9 < self._opts.flush_rate:
            await asyncio.sleep(wait / 1e3)

        self.logger.info("flush loop stopped")
