using OpcCollector.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ConfigMgr.Init();

            (new OpcHdaSample()).Run();
        }
    }
}
