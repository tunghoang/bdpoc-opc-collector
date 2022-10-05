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

namespace OpcCollector.Collector
{

    public class DaCollector : IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static int elapsedOffset = 500;

        public DaConnection Conn { get; }
        private DataInfoConfig _dataInfoConf;
        private DaCollectorOptions _opts;

        internal List<IProcessor> processors = new List<IProcessor>();
        private bool _isRunning = false;
        private CollectorMetric metric = new CollectorMetric();
        private Dictionary<string, DaSubscriber> subs = new Dictionary<string, DaSubscriber>();
        private Accumulator<OnDataArgs> acc;

        public bool IsRunning => _isRunning;

        public DaCollector(DaConnection connection, DataInfoConfig dataInfoConf, DaCollectorOptions opts)
        {
            Conn = connection;
            _dataInfoConf = dataInfoConf;
            _opts = opts;
        }

        public DaCollector(DaConnection conn, DataInfoConfig dataInfoConf) : this(conn, dataInfoConf, new DaCollectorOptions()) { }


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
            if (subs.Count > 0)
            {
                return;
            }

            if (Conn.IsClosed())
            {
                throw new DaConnectionClosedException();
            }

            clean();

            Logger.Info("Init: Devices={0}", _dataInfoConf.Devices.Length);

            foreach (var devConf in _dataInfoConf.Devices)
            {
                initDevice(devConf);
            }

            // init accumulator
            acc = new Accumulator<OnDataArgs>(metric.TotalTag * (_opts.FlushRate * 2));
        }

        public async Task RunAsync()
        {
            if (_isRunning)
            {
                return;
            }

            Logger.Info("Init DaCollector");
            init();

            Logger.Info("Run accumulator");
            acc.Run();

            Logger.Info("Accept OPC Event");
            foreach (var subKV in subs)
            {
                subKV.Value.Resume();
            }

            _isRunning = true;

            Logger.Info("Run FlushLoop");
            var fLoop = flushLoop();

            await fLoop;
        }

        private async Task flushLoop()
        {
            Stopwatch timer = Stopwatch.StartNew();
            DateTime startTime = DateTime.Now;

            while (_isRunning)
            {
                // start timer
                Logger.Debug("Enter Loop at {0}", startTime.ToString());
                timer.Reset();
                timer.Start();

                // flushing
                DateTime lastTime = startTime;
                OnDataArgs[] items = acc.Flush();
                startTime = DateTime.Now;

                int totalItem = items.Sum(i => i.values.Length);
                foreach (var processor in processors)
                {
                    try
                    {
                        processor.Apply(items, metric);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "Cannot call Apply of {0}", processor.GetType());
                    }
                }

                // wait for next tick or skip if over
                timer.Stop();
                if (timer.ElapsedMilliseconds > (_opts.FlushRate / 2))
                {
                    Logger.Warn("last flush loop spent too much times: {0} ms for {1} buffered", timer.ElapsedMilliseconds, totalItem);
                }

                double elapsed = Math.Ceiling(startTime.Subtract(lastTime).TotalMilliseconds);
                int wait = _opts.FlushRate - Convert.ToInt32(timer.ElapsedMilliseconds) % _opts.FlushRate - startTime.Millisecond + 500;

                metric.FlushTicks = timer.ElapsedTicks;
                // TODO: calculate with SampleRate
                metric.CollectRate = (double)totalItem / (elapsed) * 1000.0; // elasped time (ms) - 500 ms in order to hope that will improve precision
                metric.LastBuffered = totalItem;

                Logger.Debug("metric {0} {1} {2} {3} {4} {5} {6}", metric.CollectRate, timer.ElapsedMilliseconds, metric.LastBuffered, elapsed, wait, metric.TotalSubscriber, metric.TotalTag);

                // if current loop spent more time than one flush tick, skip waiting
                if (timer.ElapsedMilliseconds < _opts.FlushRate)
                {
                    await Task.Delay(wait);
                }
            }

            Logger.Info("Flush Loop Stopped");
        }

        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            foreach (var subKV in subs)
            {
                subKV.Value.Pause();
            }

            acc.Close();

            _isRunning = false;

            Logger.Info("Stopped");
        }

        private void clean()
        {
            foreach (var subKV in subs)
            {
                //subKV.Value.Unsubscribe();
                subKV.Value.Dispose();
            }
        }

        public void Dispose()
        {
            clean();
        }

        private void onDataHandler(OnDataArgs args)
        {
            acc.Add(args);
        }

        private void initDevice(DeviceConfig devConf)
        {
            var tags = devConf.Tags;
            var items = new List<TsCDaItem>();

            foreach (var tag in tags)
            {
                items.Add(new TsCDaItem { ItemName = tag.TagNumber });
            }

            var part = new List<TsCDaItem>(_opts.ItemPerSub);

            foreach (var (item, i) in items.Select((v, i) => (v, i)))
            {
                part.Add(item);
                if (part.Count == part.Capacity || i == (items.Count - 1))
                {
                    var sub = newSubscriber($"{devConf.Name}::{i}::{Guid.NewGuid()}");
                    sub.Metadata = devConf;
                    int nerror = sub.AddItems(part.ToArray());
                    metric.TotalTag += part.Count - nerror;
                    metric.TotalSubscriber++;
                    part.Clear();
                }

            }
        }

        private DaSubscriber newSubscriber(string subName)
        {
            var sub = Conn.Subscribe(new TsCDaSubscriptionState { Name = subName, Active = false, UpdateRate = _opts.SampleRate });

            sub.OnData(onDataHandler);

            subs.Add(sub.sid, sub);

            return sub;
        }

    }

    public class DaCollectorOptions
    {
        public int ItemPerSub = 50;
        public int SampleRate = 1000;
        public int FlushRate = 5000;

        public static DaCollectorOptions ParseCollectorCfg(Dictionary<string, string> collectorConfig)
        {
            var opts = new DaCollectorOptions();

            if (collectorConfig.ContainsKey("ITEM_PER_SUBSCRIBER"))
            {
                opts.ItemPerSub = Int32.Parse(collectorConfig["ITEM_PER_SUBSCRIBER"]);
            }

            if (collectorConfig.ContainsKey("SAMPLE_RATE"))
            {
                opts.SampleRate = Int32.Parse(collectorConfig["SAMPLE_RATE"]);
            }

            if (collectorConfig.ContainsKey("FLUSH_RATE"))
            {
                opts.FlushRate = Int32.Parse(collectorConfig["FLUSH_RATE"]);
            }

            return opts;

        }
    }

}
