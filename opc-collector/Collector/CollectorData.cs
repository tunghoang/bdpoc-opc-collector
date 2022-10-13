using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcCollector.Collector
{
    public class CollectorData
    {
        public object[] metadata;

        public string TagNumber;
        public object TagValue;
        public DateTime Timestamp;
    }

    public class CollectorDataBatch
    {
        public object metadata;
        public object subscriptionHandle;
        public object requestHandle;
        public CollectorData[] items;
    }
}
