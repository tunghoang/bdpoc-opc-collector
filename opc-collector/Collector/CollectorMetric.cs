using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcCollector.Collector
{
    public class CollectorMetric
    {
        public double CollectRate = 0;
        public long FlushTicks = 0;
        public int TotalTag = 0;
        public int LastBuffered = 0;
        public int TotalSubscriber = 0;
    }
}
