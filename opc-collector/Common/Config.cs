using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Technosoftware.DaAeHdaClient.Da;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OpcCollector.Common
{
    public class TagConfig
    {
        public string TagNumber;
    }
    public class DeviceConfig
    {
        public string Name;
        public TagConfig[] Tags;

        public TagConfig FindTag(TsCDaItemValueResult item)
        {
            foreach (var tag in Tags)
            {
                if (tag.TagNumber == item.ItemName)
                {
                    return tag;
                }
            }

            return null;
        }
    }

    public class DataInfoConfig
    {
        public DeviceConfig[] Devices;
    }

    public class ConfigMgr
    {
        private static ConfigMgr _instance = new ConfigMgr();

        public DataInfoConfig dataInfoConfig;

        public Dictionary<string, string> collectorConfig;

        private ConfigMgr()
        {

        }

        public static ConfigMgr Instance
        {
            get { return _instance; }
        }

        public static void Init()
        {
            string dataInfoYml = File.ReadAllText(@"tags.yaml");

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var config = deserializer.Deserialize<DataInfoConfig>(dataInfoYml);
            _instance.dataInfoConfig = config;

            string collectorConfigYml = File.ReadAllText(@"config.yaml");

            _instance.collectorConfig = deserializer.Deserialize<Dictionary<string, string>>(collectorConfigYml);
        }

        public static Dictionary<string, string> Collector()
        {
            return _instance.collectorConfig;
        }

        public static DataInfoConfig DataInfo()
        {
            return _instance.dataInfoConfig;
        }

    }
}
