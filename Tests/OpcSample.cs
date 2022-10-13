using OpcCollector.Collector;
using OpcCollector.Common;
using OpcCollector.Processor.Console0;
using OpcCollector.Processor.InfluxDB;
using System;
using Technosoftware.DaAeHdaClient;


namespace Tests
{

    /// <summary>
    /// Simple OPC DA Client Application
    /// </summary>
    class OpcSample
    {

        public void Run()
        {
            try
            {
                // const string serverUrl = "opcda://192.168.1.101/OPC.PHDServerDA.1";
                string serverUrl = "";
                ConfigMgr.Collector().TryGetValue("DA_SERVER_URL", out serverUrl);

                DaConnection connection = new DaConnection();

                // Connect to the server
                connection.Connect(serverUrl);

                // Get the status from the server
                OpcServerStatus status = connection.Status();
                Console.WriteLine($"Server Connected. Status of Server is {status.ServerState}");
                Console.WriteLine();

                //var listTags = connection.ListTag(new TsCDaBrowseFilters());
                //foreach (var tag in listTags)
                //{
                //    Console.WriteLine($"Name: {tag.Name}, ItemName: {tag.ItemName}, ItemPath: {tag.ItemPath}");
                //}

                Console.WriteLine();
                Console.WriteLine("---------------");
                Console.WriteLine();

                var collector = new DaCollector(connection, ConfigMgr.DataInfo());

                // register all processor
                collector.Register(new InfluxDBProcessor(new InfluxDBProcessorOptions
                {
                    Url = ConfigMgr.Collector()["INFLUXDB_URL"],
                    Token = ConfigMgr.Collector()["INFLUXDB_TOKEN"],
                    Bucket = ConfigMgr.Collector()["INFLUXDB_BUCKET"],
                    Org = ConfigMgr.Collector()["INFLUXDB_ORG"],
                }));
                //collector.Register(new ConsoleProcessor());

                // run collector
                collector.RunAsync();

                Console.WriteLine("   Press any key to stop collector and disconnect.");
                Console.ReadLine();

                collector.Stop();
                collector.Dispose();
                connection.Disconnect();
                //myDaServer.Disconnect();
                connection.Dispose();
                Console.ReadLine();
                Console.WriteLine("   Disconnected from the server.");
                Console.WriteLine();

            }
            catch (OpcResultException e)
            {
                Console.WriteLine("   " + e.Message);

                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("   " + e.Message);
                Console.ReadLine();
            }

        }
    }
}
