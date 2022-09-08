using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcCollector.Collector
{
    internal interface IConnetion
    {
        bool IsConnected { get; }

        void Connect(string url);
        void Disconnect();
        bool IsClosed();

    }
}
