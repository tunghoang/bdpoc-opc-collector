using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpcCollector.Collector.SubscriberEvent;
using Technosoftware.DaAeHdaClient;
using Technosoftware.DaAeHdaClient.Da;

namespace OpcCollector.Collector
{

    public class DaSubscriber : ISubscriber, IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal readonly object _mu = new object(); // lock

        internal DaConnection _conn = null;
        internal TsCDaSubscription _subscription = null;

        internal OnDataHandler _onDataHandler;

        public string sid = Guid.NewGuid().ToString();
        public object Metadata { get; set; }

        public TsCDaSubscription Group
        {
            get { return _subscription; }
        }

        public DaSubscriber(DaConnection conn)
        {
            _conn = conn;
        }

        public void Dispose()
        {
            try
            {
                if (_subscription != null)
                {
                    Unsubscribe();
                }

            }
            catch (Exception)
            {
                // ignore exception
            }
        }

        public void OnData(OnDataHandler handler)
        {
            _onDataHandler += handler;
        }
        private void onDataChangeEvent(object subscriptionHandle, object requestHandle, TsCDaItemValueResult[] values)
        {
            Logger.Debug("On data change. subscription={0} requestHandle={1}", subscriptionHandle, requestHandle);

            var args = new OnDataArgs { subscriber = this, metadata = Metadata, subscriptionHandle = subscriptionHandle, requestHandle = requestHandle, values = values };

            _onDataHandler?.Invoke(args);

        }

        public void Subscribe(TsCDaSubscriptionState state)
        {
            if (_subscription != null)
            {
                return;
            }

            state.ClientHandle = sid;

            lock (_mu)
            {
                // create the subscription.
                _subscription = (TsCDaSubscription)_conn._server.CreateSubscription(state);
                _subscription.DataChangedEvent += onDataChangeEvent;
                _conn.AddSubscriber(this);
            }
        }

        public void ReSubscribe(TsCDaSubscriptionState state)
        {
            if (_subscription != null)
            {
                Unsubscribe();
            }

            Subscribe(state);
        }

        public void Unsubscribe()
        {
            if (_subscription == null)
            {
                return;
            }

            _conn._server.CancelSubscription(_subscription);
            _conn.RemoveSubscriber(this);

            _subscription.Dispose();
            _subscription = null;
        }

        public void Pause()
        {
            // Set subscription inactive
            if (!_subscription.State.Active)
            {
                return;
            }

            var _state = (TsCDaSubscriptionState)_subscription.State.Clone();
            _state.Active = false;
            _subscription.ModifyState((int)TsCDaStateMask.Active, _state);
        }

        public void Resume()
        {
            // Set subscription active
            if (_subscription.State.Active)
            {
                return;
            }

            var _state = (TsCDaSubscriptionState)_subscription.State.Clone();
            _state.Active = true;
            _subscription.ModifyState((int)TsCDaStateMask.Active, _state);
        }

        public int AddItems(TsCDaItem[] items)
        {
            int nerror = 0;

            var itemResults = _subscription.AddItems(items);
            foreach (var item in itemResults)
            {
                if (item.Result.IsError())
                {
                    nerror++;
                    Logger.Warn("Item `{0}` could not be add to group: {1}. code={2}", item.ItemName, item.Result.Description(), item.Result.Code);
                }
            }

            return nerror;
        }

        public OpcItemResult[] RemoveItems(TsCDaItem[] items)
        {
            var itemResults = _subscription.RemoveItems(items);
            foreach (var item in itemResults)
            {
                if (item.Result.IsError())
                {
                    Logger.Warn("Item `{0}` could not be remove from group {1}. code={2}", item.ItemName, item.Result.Description(), item.Result.Failed());
                }
            }

            return itemResults;
        }
    }
}
