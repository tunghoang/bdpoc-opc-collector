using OpcCollector.Collector;
using OpcCollector.Common;
using OpcCollector.Processor.InfluxDB;
using System;
using System.Threading.Tasks;
using Technosoftware.DaAeHdaClient;


namespace DACollectord
{

    class Collectord
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private bool _terminated = false;

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

        public async Task Run()
        {
            setupGracefulShutdown();

            await _run();

            //while (!_terminated)
            //{
            //    _run();

            //}
        }

        private async Task _run()
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
                collector = new DaCollector(connection, ConfigMgr.DataInfo(), DaCollectorOptions.ParseCollectorCfg(ConfigMgr.Collector()));

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
                var completedTask = await Task.WhenAny(new[] {
                    Task.Run(async () => {
                        await collector.RunAsync();
                        _terminated = true;
                    }),
                    Task.Run(async () => {
                        // bootstrap wait
                        await Task.Delay(1000 * 30);
                        // check for closed
                        while (!connection.IsClosed() && !_terminated)
                        {
                            await Task.Delay(2000);
                        }
                    })
                });

                await completedTask;
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
