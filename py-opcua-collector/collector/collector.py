import common.logger as logging
import asyncio
import re
from typing import List
from asyncua.common.subscription import DataChangeNotif

from .connection import UaConnection
from .subscriber import UaSubscriber, UaNode
from common.config import DataInfoConfig, DeviceConfig, TagConfig
from common.accumulator import Buffer
from time import monotonic_ns as time
from datetime import timedelta


class CollectorData:
    def __init__(self, metadata, node, value, timestamp):
        self.metadata = metadata
        self.node = node
        self.value = value
        self.timestamp = timestamp


class UaCollectorMetric:
    def __init__(self, opts):
        self.collect_rate = 0.0
        self.total_tag = 0
        self.total_subscriber = 0
        self.flush_ticks = 0
        self.last_buffered = 0
        self.opts = opts


class UaCollectorOptions:
    """docstring for UaCollectorOptions."""

    def __init__(self, location="*", type_="*", item_per_sub=50, sample_rate=1000, flush_rate=5000):
        self.location = location
        self.type = type_
        self.item_per_sub = item_per_sub
        self.sample_rate = sample_rate
        self.flush_rate = flush_rate


class UaCollector:
    """docstring for UaCollector."""

    def __init__(self, connection: UaConnection, diconfig: DataInfoConfig, opts=UaCollectorOptions()):
        self.logger = logging.getLogger(__name__)
        self.conn = connection
        self.diconfig = diconfig

        self._metric = UaCollectorMetric(opts)
        self._opts = opts
        self._processors = []
        self._subs: List[UaSubscriber] = []
        self.running = False
        self._loop = None
        self.node_dict = {}

    def get_node(self, tag: TagConfig):
        return self.conn.c.get_node(tag.nodeid())

    async def get_data_root(self):
        objnode = self.conn.c.nodes.objects
        childs = await objnode.get_children()
        # path = ["0:Objects", "1:PHDServer"]
        # => hard-coded index of PHDServer Object
        data_root = childs[1]
        return data_root

    async def run(self, args):
        if self._loop is not None:
            return self._loop

        self.logger.info("Init UaCollector")
        await self._init(args)

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

    async def history_collect(self, args):
        if self._loop is not None:
            return self._loop

        self.logger.info("Init UaCollector {read_mode=true}")
        (devc, ua_nodes) = await self._init(args)

        self.logger.info("Start history collection")
        await self._history_read(args, ua_nodes, devc)

        self.logger.info("Completed history collection")

    def register(self, processor):
        self._processors.append(processor)

    def un_register(self, processor):
        self._processors.remove(processor)

    async def _init(self, args, read_mode=False):
        if len(self._subs) > 0:
            return (None, [])

        # self.logger.info("Init: Devices=%s", len(self.diconfig.devices))

        # for devc in self.diconfig.devices:
        #     await self._init_device(devc)

        devc = DeviceConfig('phdpeer', [])
        ua_nodes = await self._get_all_node(args, devc)

        if not read_mode:
            await self._init_sub(ua_nodes, devc)
            self._acc = Buffer(self._metric.total_tag *
                               (int(self._opts.flush_rate/1e3) * 3))

        return (devc, ua_nodes)

    def _on_data_handler(self, subr, node, val, data):
        ua_node = self.node_dict[node]
        tag_name = ua_node.metadata
        self._acc.add(CollectorData(metadata=subr.metadata,
                      node=tag_name, value=val, timestamp=data.SourceTimestamp))

    async def _new_subscriber(self):
        subr = await self.conn.subscribe(self._opts.sample_rate)

        subr.on_data(self._on_data_handler)
        self._subs.append(subr)
        self._metric.total_subscriber += 1

        return subr

    async def _create_ua_node(self, node):
        try:
            await node.read_value()
            tag_name = (await node.read_browse_name()).Name
            ua_node = UaNode(node, tag_name)
            self.node_dict[node] = ua_node

            return ua_node
        except Exception as ex:
            print(ex)
            self.logger.warn("ignore node due to un-qualified: %r", await node.read_browse_name())

            return None

    async def _get_all_node(self, args, dev_conf):
        root = await self.get_data_root()

        childs = await root.get_children()
        nodes = []
        location = args.location
        type_ = args.type

        labels = []
        tagListFile = open(f'/tmp/taglist-{location}-{type_}.txt', 'w')

        for n in childs:
            ua_node = await self._create_ua_node(n)
            if ua_node is not None:
                # filter node by location and type
                # node tag name format: {location}_{type}_*.*
                tokens = ua_node.metadata.split("_")
                if len(tokens) < 2:
                    continue

                if location != "*" and tokens[0] != location:
                    continue

                if type_ != "*" and re.search(type_, tokens[1]) is None:
                    continue

                self.logger.info("--> %s", ua_node.metadata)
                tagListFile.write(ua_node.metadata)
                nodes.append(ua_node)
                labels.append(ua_node.metadata)

        self.logger.info("total node after filter(location=%s, type=%s): %s/%s",
                         location, type_, len(nodes), len(childs))
        self.logger.info("%s", ",".join(labels))
        tagListFile.close()

        return nodes

    async def _init_sub(self, ua_nodes, metadata):
        i = 0
        item_per_sub = self._opts.item_per_sub

        while i < len(ua_nodes):
            sub_nodes = [ua_node.n for ua_node in ua_nodes[i:i+item_per_sub]]
            # TODO: add check Node exists
            subr = await self._new_subscriber()
            subr.metadata = metadata
            nerr = await subr.add_items(sub_nodes) or 0
            self.logger.info("init subscribe, nodes: %s/%s",
                             len(sub_nodes) - nerr, len(sub_nodes))
            self._metric.total_tag += len(sub_nodes) - nerr

            i += item_per_sub

        return i

    async def _init_device(self, dev_conf: DeviceConfig):
        tags = dev_conf.tags
        items = [self.get_node(t) for t in tags]
        await self._init_sub(items, dev_conf)

    async def _start_subs(self):
        for subr in self._subs:
            await subr.start()

    async def _stop_subs(self):
        for subr in self._subs:
            await subr.stop()

    async def _process_items(self, items, metric=None):
        for processor in self._processors:
            try:
                processor.apply(items, metric)
            except Exception as ex:
                self.logger.error("call apply of %r err: %r",
                                  type(processor).__name__, ex)

    async def _history_read(self, args, ua_nodes, devc):
        limit = args.limit
        starttime = args.starttime
        endtime = args.endtime

        # truncate time period to limit the number of item per read
        # default rate=1s => limit 1000 item/read = 1000s
        start = starttime
        while start < endtime:
            # calculate end_time
            end = start + timedelta(seconds=limit)
            if end > endtime:
                end = endtime

            # fetch and process
            for ua_node in ua_nodes:
                tag_name = ua_node.metadata
                values = await ua_node.n.read_raw_history(start, end)
                items = [CollectorData(metadata=devc, node=tag_name, value=data.Value.Value,
                                       timestamp=data.SourceTimestamp) for data in values]

                self.logger.info(
                    f"{tag_name} start={start} end={end} received={len(items)}")

                await self._process_items(items)

            # next iteration
            start = end

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

            self.logger.info(
                f"{self._opts.location}-{self._opts.type} rate={metric.collect_rate} ticks={flush_ticks/1e9} buffered={metric.last_buffered} elapsed={elapsed} wait={wait} subs={metric.total_subscriber} tags={metric.total_tag}")

            # if flush_ticks / 1e9 < self._opts.flush_rate:
            await asyncio.sleep(wait / 1e3)

        self.logger.info("flush loop stopped")
