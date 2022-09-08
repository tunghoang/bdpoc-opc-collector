using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Technosoftware.DaAeHdaClient;
using Technosoftware.DaAeHdaClient.Hda;

namespace OpcCollector.Collector
{
    public class HdaConnection : IDisposable, IConnetion
    {
        internal TsCHdaServer _server = new TsCHdaServer();

        public bool IsConnected => _server.IsConnected;

        public HdaConnection()
        {
        }

        public void Connect(string url)
        {
            _server.Connect(url);
        }

        public void Disconnect()
        {
            if (_server.IsConnected)
            {
                _server.Disconnect();
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

        public TsCHdaItemValueCollection[] ReadRaw(OpcItem[] items, DateTime start, DateTime end)
        {
            TsCHdaTrend trend = new TsCHdaTrend(_server)
            {
                StartTime = new TsCHdaTime(start),
                EndTime = new TsCHdaTime(end),
                IncludeBounds = true,
                MaxValues = 1000
            };

            foreach (var item in items)
            {
                trend.AddItem(item);
            }

            return trend.ReadRaw();
        }

        public TsCHdaItemValueCollection[] Read(OpcItem[] items, DateTime start, DateTime end)
        {
            TsCHdaTrend trend = new TsCHdaTrend(_server)
            {
                StartTime = new TsCHdaTime(start),
                EndTime = new TsCHdaTime(end),
                IncludeBounds = true,
                MaxValues = 1000
            };

            foreach (var item in items)
            {
                trend.AddItem(item);
            }

            return trend.Read();
        }
    }
}
