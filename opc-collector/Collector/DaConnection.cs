using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Technosoftware.DaAeHdaClient;
using Technosoftware.DaAeHdaClient.Da;

namespace OpcCollector.Collector
{
    public class DaConnection
    {
        internal TsCDaServer _server = new TsCDaServer();

        private Dictionary<string, DaSubscriber> subs = new Dictionary<string, DaSubscriber>();

        public DaConnection()
        {
        }

        public void Connect(string url)
        {
            _server.Connect(url);
        }

        private bool isClosed()
        {
            return !_server.IsConnected;
        }

        public OpcServerStatus Status()
        {
            if (isClosed())
            {
                throw new DaConnectionClosedException();
            }

            return _server.GetServerStatus();
        }

        internal void AddSubscriber(DaSubscriber s)
        {
            subs[s.sid] = s;
        }

        internal void RemoveSubscriber(DaSubscriber s)
        {
            subs.Remove(s.sid);
        }

        public DaSubscriber Subscribe(TsCDaSubscriptionState state)
        {
            if (isClosed())
            {
                throw new DaConnectionClosedException();
            }

            var s = new DaSubscriber(this);
            s.Subscribe(state);

            //AddSubscriber(s);

            return s;
        }

        public void Subscribe()
        {
            Subscribe(new TsCDaSubscriptionState { });
        }

        public TsCDaItemValueResult[] Read(TsCDaItem[] items)
        {
            return _server.Read(items);
        }

        public OpcItemResult[] Write(TsCDaItemValue[] items)
        {
            return _server.Write(items);
        }
    }
}
