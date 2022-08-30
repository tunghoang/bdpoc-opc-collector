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

        #region Event Handlers

        /// <summary>
        /// A delegate to receive data change updates from the server.
        /// </summary>
        /// <param name="subscriptionHandle">
        /// A unique identifier for the subscription assigned by the client. If the parameter
        ///	<see cref="TsCDaSubscriptionState.ClientHandle">ClientHandle</see> is not defined this
        ///	parameter is empty.</param>
        /// <param name="requestHandle">
        ///	An identifier for the request assigned by the caller. This parameter is empty if
        ///	the	corresponding parameter	in the calls Read(), Write() or Refresh() is not	defined.
        ///	Can	be used	to Cancel an outstanding operation.
        ///	</param>
        /// <param name="values">
        ///	<para class="MsoBodyText" style="MARGIN: 1pt 0in">The set of changed values.</para>
        ///	<para class="MsoBodyText" style="MARGIN: 1pt 0in">Each value will always have
        ///	item’s ClientHandle field specified.</para>
        /// </param>
        public void OnDataChangeEvent(object subscriptionHandle, object requestHandle, TsCDaItemValueResult[] values)
        {
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
                    Console.Write("Time Stamp    : "); Console.WriteLine(values[i].Timestamp.ToString(CultureInfo.InvariantCulture));
                    Console.Write("Quality       : "); Console.WriteLine(values[i].Quality);
                }
                Console.Write("Result        : "); Console.WriteLine(values[i].Result.Description());
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        #endregion

        public void Run2()
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

                // Add a group with default values Active = true and UpdateRate = 500ms
                TsCDaSubscriptionState groupState = new TsCDaSubscriptionState { Name = "MyGroup", UpdateRate = 5000 };
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

                subscriber.OnData((e) => {
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

                Console.WriteLine("   Press any key to disconnect.");
                Console.ReadLine();
                myDaServer.Disconnect();
                myDaServer.Dispose();
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


        #region OPC Sample Functionality

        public void Run()
        {
            try
            {

                const string serverUrl = "opcda://localhost/Matrikon.OPC.Simulation.1";

                Console.WriteLine();
                Console.WriteLine("Simple OPC DA Client based on the OPC DA/AE/HDA Solution .NET");
                Console.WriteLine("-------------------------------------------------------------");
                Console.Write("   Press <Enter> to connect to "); Console.WriteLine(serverUrl);
                Console.ReadLine();
                Console.WriteLine("   Please wait...");

                TsCDaServer myDaServer = new TsCDaServer();

                // Connect to the server
                myDaServer.Connect(serverUrl);

                // Get the status from the server
                OpcServerStatus status = myDaServer.GetServerStatus();
                Console.WriteLine($"   Status of Server is {status.ServerState}");

                Console.WriteLine("   Connected, press <Enter> to create an active group object and add several items.");
                Console.ReadLine();

                // Add a group with default values Active = true and UpdateRate = 500ms
                TsCDaSubscriptionState groupState = new TsCDaSubscriptionState { Name = "MyGroup" /* Group Name*/ };
                var group = (TsCDaSubscription)myDaServer.CreateSubscription(groupState);

                // Add Items
                TsCDaItem[] items = new TsCDaItem[4];
                items[0] = new TsCDaItem
                {
                    ItemName = "SimulatedData.Ramp",
                    ClientHandle = 100,
                    MaxAgeSpecified = true,
                    MaxAge = 0,
                    Active = true,
                    ActiveSpecified = true
                };
                // Item Name
                // Client Handle
                // Read from Cache

                items[1] = new TsCDaItem
                {
                    ItemName = "CTT.SimpleTypes.InOut.Integer",
                    ClientHandle = 150,
                    Active = true,
                    ActiveSpecified = true
                };
                // Item Name
                // Client Handle

                items[2] = new TsCDaItem
                {
                    ItemName = "CTT.SimpleTypes.InOut.Short",
                    ClientHandle = 200,
                    Active = false,
                    ActiveSpecified = true
                };
                // Item Name
                // Client Handle

                items[3] = new TsCDaItem
                {
                    ItemName = "CTT.Arrays.InOut.Word[]",
                    ClientHandle = 250,
                    Active = false,
                    ActiveSpecified = true
                };
                // Item Name
                // Client Handle

                // Synchronous Read with server read function (DA 3.0) without a group
                TsCDaItemValueResult[] itemValues = myDaServer.Read(items);

                for (int i = 0; i < itemValues.GetLength(0); i++)
                {
                    if (itemValues[i].Result.IsError())
                    {
                        Console.WriteLine($"   Item {itemValues[i].ItemName} could not be read");
                    }
                }

                var itemResults = group.AddItems(items);

                for (int i = 0; i < itemResults.GetLength(0); i++)
                {
                    if (itemResults[i].Result.IsError())
                    {
                        Console.WriteLine($"   Item {itemResults[i].ItemName} could not be added to the group");
                    }
                }

                Console.WriteLine("");
                Console.WriteLine("   Group and items added, press <Enter> to create a data change subscription");
                Console.WriteLine("   and press <Enter> again to deactivate the data change subscription.");
                Console.WriteLine("   This stops the reception of data change notifications.");
                Console.ReadLine();

                group.DataChangedEvent += OnDataChangeEvent;

                Console.ReadLine();

                // Set group inactive
                groupState.Active = false;
                group.ModifyState((int)TsCDaStateMask.Active, groupState);

                Console.WriteLine("   Data change subscription deactivated, press <Enter> to remove all");
                Console.WriteLine("   and disconnect from the server.");
                group.Dispose();
                myDaServer.Disconnect();
                myDaServer.Dispose();
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

        #endregion
    }
}
