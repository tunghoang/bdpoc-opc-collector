using OpcCollector.Collector;
using OpcCollector.Common;
using OpcCollector.Processor.InfluxDB;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using Technosoftware.DaAeHdaClient;


namespace OpcCollector
{

    class Collectord
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public void Run()
        {
            _run();

            //while (true)
            //{
            //    _run();

            //}
        }

        private void _run()
        {

            DaConnection connection = null;
            DaCollector collector = null;

            try
            {
                string serverUrl = ConfigMgr.Collector()["DA_SERVER_URL"];
                connection = new DaConnection();

                // Connect to the server
                connection.Connect(serverUrl);

                // Get the status from the server
                OpcServerStatus status = connection.Status();
                Logger.Info("Server connected. Status of Server is {0}", status.ServerState);

                // DaCollector
                collector = new DaCollector(connection, ConfigMgr.DataInfo());

                // register all processor
                collector.Register(new InfluxDBProcessor(new InfluxDBProcessorOptions
                {
                    Url = ConfigMgr.Collector()["INFLUXDB_URL"],
                    Token = ConfigMgr.Collector()["INFLUXDB_TOKEN"],
                    Bucket = ConfigMgr.Collector()["INFLUXDB_BUCKET"],
                    Org = ConfigMgr.Collector()["INFLUXDB_ORG"],
                }));

                // run collector
                collector.RunAsync();

                // check for closed
                while (true)
                {
                    Thread.Sleep(2000);
                    if (connection.IsClosed())
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Collectord run error");
            }
            finally
            {
                collector?.Stop();
                collector?.Dispose();
                connection?.Disconnect();
                connection?.Dispose();
            }

        }
    }
}