using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Technosoftware.DaAeHdaClient.Da;

namespace OpcCollector.Collector.SubscriberEvent
{
    public class OnData : EventArgs
    {
        public object subscriptionHandle;
        public object requestHandle;
        public TsCDaItemValueResult[] values;
    }
}
