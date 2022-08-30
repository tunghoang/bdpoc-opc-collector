using OpcCollector.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcCollector.Collector
{
    public class CollectorException : BaseException
    {
        public CollectorException(string err) : base(err) { }
        public CollectorException(string err, Exception innerEx) : base(err, innerEx) { }
    }

    public class DaConnectionClosedException : CollectorException
    {
        public DaConnectionClosedException() : base("Connection is closed.") { }
    }
    public class BadDaSubscriberException : CollectorException
    {
        public BadDaSubscriberException() : base("Subscriber is not valid.") { }
        public BadDaSubscriberException(string s) : base(s) { }
    }
}
