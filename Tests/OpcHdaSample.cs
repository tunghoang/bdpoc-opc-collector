using OpcCollector.Collector;
using OpcCollector.Common;
using OpcCollector.Processor.Console0;
using OpcCollector.Processor.InfluxDB;
using System;
using Technosoftware.DaAeHdaClient;
using Technosoftware.DaAeHdaClient.Hda;


namespace Tests
{

    /// <summary>
    /// Simple OPC DA Client Application
    /// </summary>
    class OpcHdaSample
    {
        public void Run()
        {
            try
            {
                // const string serverUrl = "opcda://192.168.1.101/OPC.PHDServerDA.1";
                string serverUrl = "";
                ConfigMgr.Collector().TryGetValue("HDA_SERVER_URL", out serverUrl);

                HdaConnection connection = new HdaConnection();

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

                //OpcResult[] r1 = null;

                //connection.Server().CreateBrowser(null, out r1);

                Console.WriteLine();
                Console.WriteLine("---------------");
                Console.WriteLine();

                //var tagNumber = Console.ReadLine();
                var start = Console.ReadLine();
                var end = Console.ReadLine();

                //var results = connection.ReadRaw(
                //    new[] { new OpcItem(tagNumber) },
                //    start,
                //    end
                // );

                //foreach (var item in results)
                //{
                //    Console.WriteLine($"{item.ItemName}");

                //    foreach (TsCHdaItemValue val in item)
                //    {
                //        if ((val.Quality.GetCode() & (int)TsDaQualityMasks.QualityMask) != (int)TsDaQualityBits.Good)
                //            Console.WriteLine($"      {val.Timestamp}, {val.Quality}");
                //        else
                //            Console.WriteLine($"      {val.Timestamp}, {val.Value}");
                //    }
                //}

                var collector = new HdaCollector(
                    connection,
                    ConfigMgr.DataInfo(),
                    new HdaCollectorOptions { Start = Convert.ToDateTime(start).ToUniversalTime(), End = Convert.ToDateTime(end).ToUniversalTime() }
                );

                collector.Register(new ConsoleProcessor());

                collector.Run();

                collector.Stop();
                collector.Dispose();
                connection.Disconnect();
                //myDaServer.Disconnect();
                connection.Dispose();
                Console.WriteLine("   Disconnected from the server.");
                Console.WriteLine();

            }
            catch (OpcResultException e)
            {
                Console.WriteLine("   " + e.Message);

            }
            catch (Exception e)
            {
                Console.WriteLine("   " + e.Message);

            }
            finally
            {
                Console.ReadLine();
            }

        }
    }
}
