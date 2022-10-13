using OpcCollector.Collector;
using OpcCollector.Collector.SubscriberEvent;
using OpcCollector.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Technosoftware.DaAeHdaClient.Da;

namespace OpcCollector.Processor.Console0
{
    public class ConsoleProcessor : IProcessor
    {
        public void Apply(CollectorData[] args, DaCollectorMetric metric)
        {
            foreach (var arg in args)
            {
                ApplyOne(arg);
            }
        }

        public void ApplyOne(CollectorData args)
        {
            var devConf = (DeviceConfig)args.metadata[0];
            if (args.metadata.Length >= 3)
            {
                var requestHandle = args.metadata[1];
                var subscriptionHandle = args.metadata[2];

                Console.WriteLine($"dev: {devConf.Name} subscriptionHandle: {subscriptionHandle}");

                if (requestHandle != null)
                {
                    Console.Write("DataChange() for requestHandle :"); Console.WriteLine(requestHandle.GetHashCode().ToString());
                }
                else
                {
                    Console.WriteLine("DataChange():");
                }
            }

            //Console.Write("Client Handle : "); Console.WriteLine(args.ClientHandle);

            Console.Write($"Device: {devConf.Name}, ");
            //Console.Write($"Type: {args.TagValue.GetType()}, ");
            Console.Write($"ItemName: {args.TagNumber}, ");
            //Console.WriteLine($"Key: {args.Key}");
            //Console.WriteLine($"DiagnosticInfo: {args.DiagnosticInfo}");
            //Console.WriteLine($"Quality: {args.Quality}");
            //Console.WriteLine($"QualitySpecified: {args.QualitySpecified}");
            //Console.WriteLine($"QualityDetail: {args.Quality} Code: {args.Quality.GetCode()} LimitBits: {args.Quality.LimitBits} QualityBits: {args.Quality.QualityBits} VendorBits: {args.Quality.VendorBits}");
            Console.WriteLine($"Timestamp: {args.Timestamp}");
            //Console.WriteLine($"TimestampSpecified: {args.TimestampSpecified}");

            //if (args.Value.GetType().IsArray)
            //{
            //    UInt16[] arrValue = (UInt16[])args.Value;
            //    for (int j = 0; j < arrValue.GetLength(0); j++)
            //    {
            //        Console.Write($"Value[{j}]      : "); Console.WriteLine(arrValue[j]);
            //    }
            //}
            //else
            //{
            //    TsCDaItemValueResult valueResult = args;
            //    //TsCDaQuality quality = new TsCDaQuality(193);
            //    //valueResult.Quality = quality;
            //    string message =
            //        $"\r\n\tQuality: is not good : {valueResult.Quality} Code:{valueResult.Quality.GetCode()} LimitBits: {valueResult.Quality.LimitBits} QualityBits: {valueResult.Quality.QualityBits} VendorBits: {valueResult.Quality.VendorBits}";
            //    if (valueResult.Quality.QualityBits != TsDaQualityBits.Good && valueResult.Quality.QualityBits != TsDaQualityBits.GoodLocalOverride)
            //    {
            //        Console.WriteLine(message);
            //    }

            //    Console.Write("Value         : "); Console.WriteLine(args.Value);
            //}
            //Console.Write("Time Stamp    : "); Console.WriteLine(args.Timestamp.ToString(CultureInfo.InvariantCulture));
            //Console.Write("Quality       : "); Console.WriteLine(args.Quality);

            //Console.Write("Result        : "); Console.WriteLine(args.Result.Description());

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
