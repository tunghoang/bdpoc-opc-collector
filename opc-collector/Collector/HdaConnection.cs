using NLog;
using OpcCollector.Common;
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
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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

        public TsCHdaServer Server()
        {
            return _server;
        }

        public OpcServerStatus Status()
        {
            if (IsClosed())
            {
                throw new DaConnectionClosedException();
            }

            return _server.GetServerStatus();
        }

        public TsCHdaTrend CreateTrend(DateTime start, DateTime end)
        {
            return CreateTrend(new TsCHdaTime(start), new TsCHdaTime(end));
        }

        public TsCHdaTrend CreateTrend(TsCHdaTime start, TsCHdaTime end)
        {
            return new TsCHdaTrend(_server)
            {
                StartTime = start,
                EndTime = end,
                IncludeBounds = true,
                MaxValues = 0,
            };
        }
        //opchda://PDM-INTEG/OPC.PHDServerHDA.1
        public TsCHdaItemValueCollection[] ReadRaw(OpcItem[] items, DateTime start, DateTime end)
        {
            return ReadRaw(CreateTrend(start, end), items);
        }

        public TsCHdaItemValueCollection[] ReadRaw(OpcItem[] items, String start, String end)
        {
            return ReadRaw(CreateTrend(Convert.ToDateTime(start).ToUniversalTime(), Convert.ToDateTime(end).ToUniversalTime()), items);
        }

        public TsCHdaItemValueCollection[] ReadRaw(TsCHdaTrend trend, OpcItem[] items)
        {
            foreach (var item in items)
            {
                try
                {
                    trend.AddItem(item);

                }
                catch (OpcResultException e)
                {
                    if (e.Message == "Could not add item to trend.")
                    {
                        Logger.Warn("Item `{0}` could not be add to group: {1}. code={2}", item.ItemName, e.Result.Description(), e.Result.Code);
                    }
                }
            }

            return trend.ReadRaw();
        }
    }
}
