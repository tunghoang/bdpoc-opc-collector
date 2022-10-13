using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpcCollector.Common;

namespace HDACollectorApp
{
    public class HdaCollectorCliOptions
    {
        [Option('v', "verbose", Default = false, Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option("start", Required = true, HelpText = "Start date time.")]
        public string Start { get; set; }

        [Option("end", Required = true, HelpText = "End date time.")]
        public string End { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<HdaCollectorCliOptions>(args)
                   .WithParsed<HdaCollectorCliOptions>(o =>
                   {
                       ConfigMgr.Init();

                       var cli = new HdaCollectorCLI(o);
                       cli.Run();
                   });
        }
    }
}
