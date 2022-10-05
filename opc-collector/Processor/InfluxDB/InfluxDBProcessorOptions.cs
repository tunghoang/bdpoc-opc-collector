using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcCollector.Processor.InfluxDB
{
    public class InfluxDBProcessorOptions
    {
        public string Token;
        public string Bucket;
        public string MonitorBucket;
        public string Org;
        public string Url;
    }
}
