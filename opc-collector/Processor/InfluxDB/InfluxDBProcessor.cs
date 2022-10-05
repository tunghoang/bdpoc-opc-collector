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
using OpcCollector.Collector;

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

        public void Apply(OnDataArgs[] args, CollectorMetric metric)
        {
            int total = args.Sum(i => i.values.Length);
            List<PointData> points = new List<PointData>(total + 1);

            try
            {
                
                foreach (var arg in args)
                {
                    var items = arg.values;
                    var devConf = (DeviceConfig)arg.metadata;

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
                                .Timestamp(item.Timestamp.ToUniversalTime(), WritePrecision.S);

                            points.Add(point);
                        }

                        // TODO: handle the failed case
                        if(points.Count > 500 || points.Count > points.Capacity)
                        {
                            batchWrite(points.ToArray());
                            points.Clear();
                        }
                    }
                }

                if(points.Count > 0)
                {
                    batchWrite(points.ToArray());
                }

                // write last metric
                var metricPoint = PointData.Measurement("collector_metric").Field("collect_rate", metric.CollectRate).Timestamp(DateTime.UtcNow, WritePrecision.S);
                _writeApi.WritePoint(metricPoint, _options.MonitorBucket, _options.Org);

            }
            catch (Exception e)
            {
                Logger.Error(e, "Cannot push data to influxdb");
            }
            
        }

        private void batchWrite(PointData[] points)
        {
            Logger.Debug("batch write");
            _writeApi.WritePoints(points.ToArray(), _options.Bucket, _options.Org);
        }

        public void ApplyOne(OnDataArgs args)
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
                            .Timestamp(item.Timestamp.ToUniversalTime(), WritePrecision.S);
                        _writeApi.WritePoint(point, _options.Bucket, _options.Org);
                        //Logger.Debug("Wrote: {0}", point.ToLineProtocol());
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
