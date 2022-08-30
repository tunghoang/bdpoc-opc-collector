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
        internal readonly object _mu = new object(); // lock

        internal DaConnection _conn = null;
        internal TsCDaSubscription _subscription = null;

        internal OnDataHandler _onDataHandler;

        public string sid = Guid.NewGuid().ToString();

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
            var args = new OnData { subscriptionHandle = subscriptionHandle, requestHandle = requestHandle, values = values };

            _onDataHandler?.Invoke(args);

            System.Console.WriteLine($"On data change event ---->");
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
            if(!_subscription.State.Active)
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

        public TsCDaItemResult[] AddItems(TsCDaItem[] items)
        {
            var results = new bool[items.Length];

            var itemResults = _subscription.AddItems(items);
            for (int i = 0; i < itemResults.GetLength(0); i++)
            {
                if (itemResults[i].Result.IsError())
                {
                    Console.WriteLine($"   Item {itemResults[i].ItemName} could not be added to the group.");
                    results[i] = false;
                }
                else
                {
                    results[i] = true;
                }
            }

            return itemResults;
        }

        public OpcItemResult[] RemoveItems(TsCDaItem[] items)
        {
            var results = new bool[items.Length];

            var itemResults = _subscription.RemoveItems(items);
            for (int i = 0; i < itemResults.GetLength(0); i++)
            {
                if (itemResults[i].Result.IsError())
                {
                    Console.WriteLine($"   Item {itemResults[i].ItemName} could not be removed from group.");
                    results[i] = false;
                }
                else
                {
                    results[i] = true;
                }
            }

            return itemResults;
        }
    }
}
