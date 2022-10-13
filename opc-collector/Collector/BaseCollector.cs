using OpcCollector.Common;
using OpcCollector.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcCollector.Collector
{
    public class BaseCollector
    {
        protected DataInfoConfig _dataInfoConf;

        protected bool _isRunning = false;

        public BaseCollector(DataInfoConfig dataInfoConf)
        {
            _dataInfoConf = dataInfoConf;
        }
    }
}
