using OpcCollector.Collector.SubscriberEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcCollector.Processor
{
    public interface IProcessor
    {
        void Apply(OnData args);
    }
}
