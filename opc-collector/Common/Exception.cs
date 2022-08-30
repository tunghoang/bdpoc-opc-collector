using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcCollector.Common
{
    public class BaseException : Exception
    {
        public BaseException() : base() { }
        public BaseException(string err) : base(err) { }
        public BaseException(string err, Exception innerEx) : base(err, innerEx) { }
    }
}
