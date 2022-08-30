using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OpcCollector.Common
{
    public class TagConfig
    {
        public string Name;
    }
    public class DeviceConfig
    {
        public string Name;
        public TagConfig[] Tags;
    }

    public class YamlConfig
    {
        public DeviceConfig[] Devices;
    }
    public class ConfigMgr
    {
        private static ConfigMgr _instance = new ConfigMgr();

        public YamlConfig YmlConfig;

        private ConfigMgr()
        {

        }

        public static ConfigMgr Instance
        {
            get { return _instance; }
        }

        public static void Init(string path)
        {
            string yamlContent = File.ReadAllText(path);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var config = deserializer.Deserialize<YamlConfig>(yamlContent);
            _instance.YmlConfig = config;
        }

    }
}
