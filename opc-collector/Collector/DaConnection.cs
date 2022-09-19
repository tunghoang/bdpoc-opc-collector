using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Xml.Linq;
using Technosoftware.DaAeHdaClient;
using Technosoftware.DaAeHdaClient.Da;

namespace OpcCollector.Collector
{
    public class DaConnection : IDisposable, IConnetion
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal TsCDaServer _server = new TsCDaServer();

        private Dictionary<string, DaSubscriber> subs = new Dictionary<string, DaSubscriber>();

        public bool IsConnected => _server.IsConnected;

        public DaConnection()
        {
        }

        public void Connect(string url)
        {
            Logger.Info("Connecting to {0}", url);
            _server.Connect(url);
            Logger.Info("Connected to {0}", url);
        }

        public void Disconnect()
        {
            if (_server.IsConnected)
            {
                foreach(var s in subs)
                {
                    s.Value.Unsubscribe();
                }
                subs.Clear();
                Logger.Info("Disconnecting from {0}", _server.Url);
                _server.Disconnect();
                Logger.Info("Disconnected");
            }
            _server.Dispose();
        }

        public void Dispose()
        {
            Disconnect();
        }

        public bool IsClosed()
        {
            return !_server.IsConnected;
        }

        public OpcServerStatus Status()
        {
            if (IsClosed())
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
            if (IsClosed())
            {
                throw new DaConnectionClosedException();
            }

            var s = new DaSubscriber(this);
            s.Subscribe(state);

            return s;
        }

        public DaSubscriber Subscribe()
        {
            return Subscribe(new TsCDaSubscriptionState { });
        }

        public TsCDaItemValueResult[] Read(TsCDaItem[] items)
        {
            return _server.Read(items);
        }

        public OpcItemResult[] Write(TsCDaItemValue[] items)
        {
            return _server.Write(items);
        }

        public List<TsCDaBrowseElement> BrowseDeep(OpcItem itemId, TsCDaBrowseFilters filters, int level = 0)
        {
            var results = new List<TsCDaBrowseElement>();

            TsCDaBrowsePosition position = null;
            TsCDaBrowseElement[] elements = _server.Browse(itemId, filters, out position);

            if (elements != null)
            {
                foreach (var el in elements)
                {
                    if (el.HasChildren)
                    {
                        // nested fetch all child
                        var childItemId = new OpcItem(el.ItemPath, el.ItemName);
                        var childs = BrowseDeep(childItemId, filters, level - 1);
                        results.AddRange(childs);
                    }
                    else
                    {
                        results.Add(el);
                    }
                }
            }

            // loop until all elements have been fetched.
            while (position != null)
            {
                // fetch next batch of elements,.
                elements = _server.BrowseNext(ref position);

                // add children.
                if (elements != null)
                {
                    foreach (var el in elements)
                    {
                        if (el.HasChildren)
                        {
                            // nested fetch all child
                            var childItemId = new OpcItem(el.ItemPath, el.ItemName);
                            var childs = BrowseDeep(childItemId, filters, level - 1);
                            results.AddRange(childs);
                        }
                        else
                        {
                            results.Add(el);
                        }
                    }
                }
            }

            return results;
        }

        public List<TsCDaBrowseElement> ListTag(TsCDaBrowseFilters filters)
        {
            var elements = BrowseDeep(null, filters);

            return elements;
        }
    }
}
