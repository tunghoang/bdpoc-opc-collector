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
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            try
            {
                ConfigMgr.Init();

                //var myOpcSample = new OpcSample();
                //myOpcSample.Run();

                //var myOpcHdaSample = new OpcHdaSample();
                //myOpcHdaSample.Run();

                var collectord = new Collectord();
                collectord.Run();

            }
            catch (Exception e)
            {
                Logger.Error(e, "Main app error");
            }
            finally
            {
                NLog.LogManager.Shutdown(); // Flush and close down internal threads and timers
            }

        }
    }
}
