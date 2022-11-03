using OpcCollector.Common;
using System;
using System.Threading.Tasks;

namespace DACollectord
{
    internal class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            try
            {
                Logger.Info("App starting...");

                ConfigMgr.Init();
                Logger.Info("ConfigMgr loaded.");

                var collectord = new Collectord();
                Task.WaitAll(collectord.Run());
                //collectord.Run();

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
