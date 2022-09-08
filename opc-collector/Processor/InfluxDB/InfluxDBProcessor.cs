using InfluxDB.Client;
using InfluxDB.Client.Writes;
using InfluxDB.Client.Api.Domain;
using OpcCollector.Collector.SubscriberEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Technosoftware.DaAeHdaClient.Da;
using OpcCollector.Common;

namespace OpcCollector.Processor.InfluxDB
{
    public class InfluxDBProcessor : IProcessor, IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private InfluxDBProcessorOptions _options;
        private InfluxDBClient _client;
        private WriteApi _writeApi;

        public InfluxDBProcessor(InfluxDBProcessorOptions options)
        {
            _options = options;
            _client = InfluxDBClientFactory.Create(options.Url, options.Token);
            _writeApi = _client.GetWriteApi();
        }

        public void Dispose()
        {
            _writeApi?.Dispose();
            _writeApi = null;
            _client?.Dispose();
            _client = null;
        }

        public void Apply(OnDataArgs args)
        {
            // TODO: Disable log if needed
            //Logger.Info("Writing to influxdb...");

            try
            {
                var items = args.values;
                var devConf = (DeviceConfig)args.metadata;

                foreach (var item in items)
                {
                    if (item.Result.IsSuccess())
                    {
                        var point = PointData.Measurement(devConf.Name)
                            //.Tag("ItemName", item.ItemName)
                            //.Tag("Key", item.Key)
                            //.Tag("Type", item.Value.GetType().ToString())
                            //.Field("Quality", item.Quality)
                            //.Field("DiagnosticInfo", item.DiagnosticInfo)
                            .Field(item.ItemName, item.Value)
                            .Timestamp(item.Timestamp.ToUniversalTime(), WritePrecision.Ns);

                        _writeApi.WritePoint(point, _options.Bucket, _options.Org);
                        Logger.Debug("Wrote: {0}", point.ToLineProtocol());
                    }
                    // TODO: handle the failed case
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Cannot push data to influxdb");
            }

        }
    }
}
