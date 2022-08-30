using OpcCollector.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcCollector
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ConfigMgr.Init(@"config.yaml");

            var myOpcSample = new OpcSample();
            myOpcSample.Run();
        }
    }
}
