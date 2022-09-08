using OpcCollector.Collector.SubscriberEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Technosoftware.DaAeHdaClient.Da;

namespace OpcCollector.Collector
{
    public delegate void OnDataHandler(OnDataArgs args);

    public interface ISubscriber
    {
        void Subscribe(TsCDaSubscriptionState state);

        void ReSubscribe(TsCDaSubscriptionState state);

        void Unsubscribe();

        void OnData(OnDataHandler handler);

    }
}
