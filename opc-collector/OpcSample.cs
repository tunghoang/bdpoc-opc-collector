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
using OpcCollector.Processor;
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

                var collector = new DaCollector(connection);

                // register all processor
                collector.Register(new ConsoleProcessor());

                // init
                collector.Init();

                // run collector
                collector.Run();

                Console.WriteLine("   Press any key to stop collector and disconnect.");
                Console.ReadLine();

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
