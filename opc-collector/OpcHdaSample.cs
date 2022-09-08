#region Using Directives

using OpcCollector.Collector;
using OpcCollector.Common;
using OpcCollector.Processor;
using System;
using System.Globalization;
using System.Linq;
using Technosoftware.DaAeHdaClient;
using Technosoftware.DaAeHdaClient.Da;
using Technosoftware.DaAeHdaClient.Hda;
using YamlDotNet.Core.Tokens;

#endregion

namespace OpcCollector
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

                Console.WriteLine();
                Console.WriteLine("---------------");
                Console.WriteLine();

                var results = connection.ReadRaw(
                    new[] { new OpcItem("Simulation Items/Random/Int4") },
                    new DateTime(2022, 09, 06, 10, 00, 00),
                    new DateTime(2022, 09, 06, 11, 00, 00)
                 );

                foreach (var item in results)
                {
                    Console.WriteLine($"{item.ItemName}");

                    foreach (TsCHdaItemValue val in item)
                    {
                        if ((val.Quality.GetCode() & (int)TsDaQualityMasks.QualityMask) != (int)TsDaQualityBits.Good)
                            Console.WriteLine($"      {val.Timestamp}, {val.Quality}");
                        else
                            Console.WriteLine($"      {val.Timestamp}, {val.Value}");
                    }
                }

                connection.Disconnect();
                //myDaServer.Disconnect();
                connection.Dispose();
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
