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
        public void Apply(OnDataArgs[] args, CollectorMetric metric)
        {
            foreach (var arg in args)
            {
                ApplyOne(arg);
            }
        }

        public void ApplyOne(OnDataArgs args)
        {
            var devConf = (DeviceConfig)args.metadata;
            var requestHandle = args.requestHandle;
            var subscriptionHandle = args.subscriptionHandle;
            var values = args.values;

            Console.WriteLine($"dev: {devConf.Name} subscriptionHandle: {subscriptionHandle}, values length: {values.Length}");

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
                    Console.WriteLine($"ItemName: {values[i].ItemName}");
                    Console.WriteLine($"Key: {values[i].Key}");
                    Console.WriteLine($"DiagnosticInfo: {values[i].DiagnosticInfo}");
                    Console.WriteLine($"Quality: {values[i].Quality}");
                    Console.WriteLine($"QualitySpecified: {values[i].QualitySpecified}");
                    Console.WriteLine($"QualityDetail: {values[i].Quality} Code: {values[i].Quality.GetCode()} LimitBits: {values[i].Quality.LimitBits} QualityBits: {values[i].Quality.QualityBits} VendorBits: {values[i].Quality.VendorBits}");
                    Console.WriteLine($"Timestamp: {values[i].Timestamp}");
                    Console.WriteLine($"TimestampSpecified: {values[i].TimestampSpecified}");

                    //if (values[i].Value.GetType().IsArray)
                    //{
                    //    UInt16[] arrValue = (UInt16[])values[i].Value;
                    //    for (int j = 0; j < arrValue.GetLength(0); j++)
                    //    {
                    //        Console.Write($"Value[{j}]      : "); Console.WriteLine(arrValue[j]);
                    //    }
                    //}
                    //else
                    //{
                    //    TsCDaItemValueResult valueResult = values[i];
                    //    //TsCDaQuality quality = new TsCDaQuality(193);
                    //    //valueResult.Quality = quality;
                    //    string message =
                    //        $"\r\n\tQuality: is not good : {valueResult.Quality} Code:{valueResult.Quality.GetCode()} LimitBits: {valueResult.Quality.LimitBits} QualityBits: {valueResult.Quality.QualityBits} VendorBits: {valueResult.Quality.VendorBits}";
                    //    if (valueResult.Quality.QualityBits != TsDaQualityBits.Good && valueResult.Quality.QualityBits != TsDaQualityBits.GoodLocalOverride)
                    //    {
                    //        Console.WriteLine(message);
                    //    }

                    //    Console.Write("Value         : "); Console.WriteLine(values[i].Value);
                    //}
                    //Console.Write("Time Stamp    : "); Console.WriteLine(values[i].Timestamp.ToString(CultureInfo.InvariantCulture));
                    //Console.Write("Quality       : "); Console.WriteLine(values[i].Quality);
                }
                //Console.Write("Result        : "); Console.WriteLine(values[i].Result.Description());
            }
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
