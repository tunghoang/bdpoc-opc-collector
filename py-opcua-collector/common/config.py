from asyncua.ua import NodeIdType, NodeId
from typing import Any, List
from yaml import Loader, load
from os import path

class TagConfig:
    def __init__(self, tag_number, type: str = 'string', namespace=0):
        self.tag_number = tag_number
        self.namespace: Any = namespace
        # TwoByte = 0
        # FourByte = 1
        # Numeric = 2
        # String = 3
        # Guid = 4
        # ByteString = 5
        type = type.lower()
        if type == "string":
            self.type = NodeIdType.String
        elif type == "numeric":
            self.type = NodeIdType.Numeric
        else:
            raise Exception("unsupported type")

    def nodeid(self):
        return NodeId(self.tag_number, self.namespace, self.type)


class DeviceConfig:
    def __init__(self, name: str = '', tags: List[TagConfig] = []):
        self.name = name
        self.tags = tags

class DataInfoConfig:
    """docstring for DataInfoConfig."""

    def __init__(self, devices: List[DeviceConfig]=[]):
        self.devices = devices


class Config:
    def __init__(self, collector_config,  data_info_config: DataInfoConfig):
        self.data_info_config = data_info_config
        self.collector_config = collector_config


def parse_config(dir):
    stream = open(path.join(dir, "tags.yaml"), 'r')
    raw = load(stream, Loader)
    
    # improve this duplicated unmarshal by implement custom yaml Loader on above load call 
    raw_devs = raw["devices"]
    devs = []

    for e in raw_devs:
        tags = []
        for t in e["tags"]:
            # TODO: default type=string, namespace=2 for testing
            tags.append(TagConfig(t["tag_number"], t.get("type") or "string", t.get("namespace") or 2))

        devs.append(DeviceConfig(e["name"], tags))

    dinfo_conf = DataInfoConfig(devs)

    stream = open(path.join(dir, "config.yaml"), 'r')
    raw = load(stream, Loader)

    return Config(raw, dinfo_conf)
