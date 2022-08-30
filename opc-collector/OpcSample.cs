#region Copyright (c) 2011-2022 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2022 Technosoftware GmbH. All rights reserved
// Web: https://technosoftware.com 
// 
// License: 
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// SPDX-License-Identifier: MIT
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2022 Technosoftware GmbH. All rights reserved

#region Using Directives

using OpcCollector.Collector;
using OpcCollector.Common;
using System;
using System.Globalization;
using System.Linq;
using Technosoftware.DaAeHdaClient;
using Technosoftware.DaAeHdaClient.Da;
using YamlDotNet.Core.Tokens;

#endregion

namespace OpcCollector
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
                const string serverUrl = "opcda://localhost/Matrikon.OPC.Simulation.1";

                DaConnection connection = new DaConnection();

                // Connect to the server
                connection.Connect(serverUrl);

                var myDaServer = connection._server;

                // Get the status from the server
                OpcServerStatus status = connection.Status();
                Console.WriteLine($"Server Connected. Status of Server is {status.ServerState}");
                Console.WriteLine();

                var listTags = connection.ListTag(new TsCDaBrowseFilters());
                foreach (var tag in listTags)
                {
                    Console.WriteLine($"Name: {tag.Name}, ItemName: {tag.ItemName}, ItemPath: {tag.ItemPath}");
                }

                Console.WriteLine();
                Console.WriteLine("---------------");
                Console.WriteLine();

                // Add a group with default values Active = true and UpdateRate = 500ms
                TsCDaSubscriptionState groupState = new TsCDaSubscriptionState { Name = "MyGroup", UpdateRate = 1000 };
                var subscriber = (DaSubscriber)connection.Subscribe(groupState);

                // Add Items
                TsCDaItem[] items = new TsCDaItem[ConfigMgr.Instance.YmlConfig.Devices[0].Tags.Length];

                foreach (var (tag, i) in ConfigMgr.Instance.YmlConfig.Devices[0].Tags.Select((tag, index) => (tag, index)))
                {
                    items[i] = new TsCDaItem
                    {
                        ItemName = tag.Name,
                    };
                }

                subscriber.AddItems(items);

                subscriber.OnData((e) =>
                {
                    var requestHandle = e.requestHandle;
                    var subscriptionHandle = e.subscriptionHandle;
                    var values = e.values;

                    Console.WriteLine($"subscriptionHandle: {subscriptionHandle}, values length: {values.Length}");

                    if (requestHandle != null)
                    {
                        Console.Write("DataChange() for requestHandle :"); Console.WriteLine(requestHandle.GetHashCode().ToString());
                    }
                    else
                    {
                        Console.WriteLine("DataChange():");
                    }
                    for (int i = 0; i < values.GetLength(0); i++)
                    {
                        Console.Write("Client Handle : "); Console.WriteLine(values[i].ClientHandle);
                        if (values[i].Result.IsSuccess())
                        {
                            Console.WriteLine($"Type: {values[i].Value.GetType()}");

                            if (values[i].Value.GetType().IsArray)
                            {
                                UInt16[] arrValue = (UInt16[])values[i].Value;
                                for (int j = 0; j < arrValue.GetLength(0); j++)
                                {
                                    Console.Write($"Value[{j}]      : "); Console.WriteLine(arrValue[j]);
                                }
                            }
                            else
                            {
                                TsCDaItemValueResult valueResult = values[i];
                                TsCDaQuality quality = new TsCDaQuality(193);
                                valueResult.Quality = quality;
                                string message =
                                    $"\r\n\tQuality: is not good : {valueResult.Quality} Code:{valueResult.Quality.GetCode()} LimitBits: {valueResult.Quality.LimitBits} QualityBits: {valueResult.Quality.QualityBits} VendorBits: {valueResult.Quality.VendorBits}";
                                if (valueResult.Quality.QualityBits != TsDaQualityBits.Good && valueResult.Quality.QualityBits != TsDaQualityBits.GoodLocalOverride)
                                {
                                    Console.WriteLine(message);
                                }

                                Console.Write("Value         : "); Console.WriteLine(values[i].Value);
                            }
                            //Console.Write("Time Stamp    : "); Console.WriteLine(values[i].Timestamp.ToString(CultureInfo.InvariantCulture));
                            //Console.Write("Quality       : "); Console.WriteLine(values[i].Quality);
                        }
                        //Console.Write("Result        : "); Console.WriteLine(values[i].Result.Description());
                    }
                    Console.WriteLine();
                    Console.WriteLine();
                });

                subscriber.Pause();

                Console.WriteLine("   Press any key to resume.");
                Console.ReadLine();
                subscriber.Resume();

                Console.WriteLine("   Press any key to disconnect.");
                Console.ReadLine();

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
