using OpcCollector.Collector.SubscriberEvent;
using OpcCollector.Processor;
using OpcCollector.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Technosoftware.DaAeHdaClient.Da;
using System.Diagnostics;
using System.Windows.Media;
using Technosoftware.DaAeHdaClient;
using Technosoftware.DaAeHdaClient.Hda;

namespace OpcCollector.Collector
{

    public class HdaCollector : BaseCollector, IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public HdaConnection Conn { get; }
        private HdaCollectorOptions _opts;

        internal List<IProcessor> processors = new List<IProcessor>();

        public bool IsRunning => _isRunning;

        public HdaCollector(HdaConnection connection, DataInfoConfig dataInfoConf, HdaCollectorOptions opts) : base(dataInfoConf)
        {
            Conn = connection;
            _opts = opts;
        }

        public HdaCollector(HdaConnection conn, DataInfoConfig dataInfoConf) : this(conn, dataInfoConf, new HdaCollectorOptions()) { }


        public void Register(IProcessor processor)
        {
            processors.Add(processor);
        }

        public void UnRegister(IProcessor processor)
        {
            processors.Remove(processor);
        }

        private void init()
        {

        }

        public void Run()
        {
            if (_isRunning)
            {
                return;
            }

            Logger.Info("Init HdaCollector");
            init();

            _isRunning = true;

            Logger.Info("Start collecting data");

            Task.WaitAll(collect());
        }

        private async Task collect()
        {
            foreach (var devConf in _dataInfoConf.Devices)
            {
                collectDevice(devConf);
            }
        }

        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;

            Logger.Info("Stopped");
        }

        private void clean()
        {
        }

        public void Dispose()
        {
            clean();
        }

        private void collectDevice(DeviceConfig devConf)
        {
            var tags = devConf.Tags;
            var items = new List<OpcItem>();

            foreach (var tag in tags)
            {
                items.Add(new OpcItem(tag.TagNumber));
            }

            var part = new List<OpcItem>(_opts.TagPerFetch);

            foreach (var (item, i) in items.Select((v, i) => (v, i)))
            {
                part.Add(item);
                if (part.Count == part.Capacity || i == (items.Count - 1))
                {
                    var datum = fetch(devConf, part.ToArray());
                    part.Clear();

                    foreach (var processor in processors)
                    {
                        try
                        {
                            processor.Apply(datum, null);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e, "Cannot call Apply of {0}", processor.GetType());
                        }
                    }

                }

            }
        }

        private CollectorData[] fetch(DeviceConfig devConf, OpcItem[] items)
        {
            var trend = Conn.CreateTrend(_opts.Start, _opts.End);
            var nValidItem = items.Length;

            foreach (var item in items)
            {
                try
                {
                    trend.AddItem(item);

                }
                catch (OpcResultException e)
                {
                    nValidItem--;

                    Logger.Warn("Item `{0}` could not be add to trend: {1}. code={2}", item.ItemName, e.Result.Description(), e.Result.Code);
                }
            }

            var result = trend.ReadRaw();
            var totalValues = result.Sum(item => item.Count);

            Logger.Debug("fetch dev: [{0}], tags: {1}/{2}, from-to: {3}-{4}, received: {5}", devConf.Name, nValidItem, items.Length, _opts.Start, _opts.End, totalValues);

            var datum = new List<CollectorData>(totalValues);

            foreach (var item in result)
            {
                foreach (TsCHdaItemValue val in item)
                {
                    DateTime ts = DateTime.SpecifyKind(val.Timestamp, DateTimeKind.Utc);

                    datum.Add(new CollectorData
                    {
                        metadata = new[] { devConf },
                        TagNumber = item.ItemName,
                        TagValue = val.Value,
                        Timestamp = ts
                    });
                }
            }

            return datum.ToArray();
        }

    }

    public class HdaCollectorOptions
    {
        public DateTime Start;
        public DateTime End;
        public int TagPerFetch = 5;
    }
}
