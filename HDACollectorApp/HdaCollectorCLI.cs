using OpcCollector.Collector;
using OpcCollector.Common;
using OpcCollector.Processor.InfluxDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Technosoftware.DaAeHdaClient;

namespace HDACollectorApp
{
    internal class HdaCollectorCLI
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private bool _terminated = false;
        private HdaCollectorCliOptions cliOpts;
        private HdaCollectorOptions colOpts;

        public HdaCollectorCLI(HdaCollectorCliOptions options)
        {
            cliOpts = options;

            var start = Convert.ToDateTime(options.Start).ToUniversalTime();
            var end = Convert.ToDateTime(options.End).ToUniversalTime();
            var elapsed = Convert.ToInt32(Math.Ceiling(end.Subtract(start).TotalSeconds));

            if(elapsed <= 0)
            {
                throw new ArgumentException("'end' time must be latter than 'start' time");
            }

            colOpts = new HdaCollectorOptions
            {
                Start = start,
                End = end,
            };

            if (elapsed < 100000)
            {
                colOpts.TagPerFetch = 100000 / elapsed;
                // limit total value per fetch to 10000 values
            }
            else
            {
                // TODO: correctly handle when total duration is greater than 100000 (mean 100000 values per tag)
                // solution: may split to smaller period, for now set tag per fetch to 1
                colOpts.TagPerFetch = 1;
            }

        }

        private void setupGracefulShutdown()
        {
            Console.CancelKeyPress += (_, ea) =>
            {
                // Tell .NET to not terminate the process
                ea.Cancel = true;

                Console.WriteLine("Received SIGINT (Ctrl+C)");
                _terminated = true;
            };

            AppDomain.CurrentDomain.ProcessExit += (_, ea) =>
            {
                Console.WriteLine("Received SIGTERM");
                _terminated = true;
            };
        }

        public void Run()
        {
            setupGracefulShutdown();

            Task.WaitAll(_run());

        }

        private async Task _run()
        {

            HdaConnection connection = null;
            HdaCollector collector = null;

            try
            {
                string serverUrl = ConfigMgr.Collector()["HDA_SERVER_URL"];
                connection = new HdaConnection();

                // Connect to the server
                connection.Connect(serverUrl);

                // Get the status from the server
                OpcServerStatus status = connection.Status();
                Logger.Info("Server connected. Status of Server is {0}", status.ServerState);


                // HdaCollector
                collector = new HdaCollector(connection, ConfigMgr.DataInfo(), colOpts);

                // register all processor
                collector.Register(new InfluxDBProcessor(new InfluxDBProcessorOptions
                {
                    Url = ConfigMgr.Collector()["INFLUXDB_URL"],
                    Token = ConfigMgr.Collector()["INFLUXDB_TOKEN"],
                    Bucket = ConfigMgr.Collector()["INFLUXDB_BUCKET"],
                    MonitorBucket = ConfigMgr.Collector()["INFLUXDB_MONITOR_BUCKET"],
                    Org = ConfigMgr.Collector()["INFLUXDB_ORG"],
                }));

                // run collector
                await Task.WhenAny(new[] {
                    Task.Run(async () => {
                        collector.Run();
                        _terminated = true;
                    }),
                    Task.Run(async () => { 
                        // check for closed
                        while (!connection.IsClosed() && !_terminated)
                        {
                            await Task.Delay(2000);
                        }
                    })
                });

                Logger.Info("Wait 5s for asynchronous processing to complete...");
                await Task.Delay(5000);
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
