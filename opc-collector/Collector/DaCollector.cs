using OpcCollector.Collector.SubscriberEvent;
using OpcCollector.Processor;
using OpcCollector.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Technosoftware.DaAeHdaClient.Da;

namespace OpcCollector.Collector
{

    public class DaCollector : IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public DaConnection Conn { get; }
        private DataInfoConfig _dataInfoConf;
        private DaCollectorOptions _opts;

        internal List<IProcessor> processors = new List<IProcessor>();
        private bool _isRunning = false;
        private Dictionary<string, DaSubscriber> subs = new Dictionary<string, DaSubscriber>();

        public bool IsRunning => _isRunning;

        public DaCollector(DaConnection connection, DataInfoConfig dataInfoConf, DaCollectorOptions opts)
        {
            Conn = connection;
            _dataInfoConf = dataInfoConf;
            _opts = opts;
        }

        public DaCollector(DaConnection conn, DataInfoConfig dataInfoConf) : this(conn, dataInfoConf, new DaCollectorOptions { ItemPerSub = 50, SampleRate = 1000 }) { }


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

        }

        public void RunAsync()
        {
            if (_isRunning)
            {
                return;
            }

            init();

            Logger.Info("Starting");

            foreach (var subKV in subs)
            {
                subKV.Value.Resume();
            }

            _isRunning = true;
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
            foreach (var processor in processors)
            {
                try
                {
                    processor.Apply(args);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Cannot call Apply of {0}", processor.GetType());
                }

            }
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
                    sub.AddItems(part.ToArray());
                    part.Clear();
                }

            }
        }

        private DaSubscriber newSubscriber(string subName)
        {
            var sub = Conn.Subscribe(new TsCDaSubscriptionState { Name = subName, Active = false, UpdateRate = _opts.SampleRate });

            sub.OnData(this.onDataHandler);

            subs.Add(sub.sid, sub);

            return sub;
        }

    }

    public class DaCollectorOptions
    {
        public int ItemPerSub;
        public int SampleRate;
    }

}
