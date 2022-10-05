using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace OpcCollector.Common
{
    public class Accumulator<T>
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Channel<T> ch = Channel.CreateUnbounded<T>();
        private Buffer<T> buf;
        private int maxLen;

        public Accumulator(int maxLen)
        {
            this.maxLen = maxLen;
            buf = new Buffer<T>(maxLen);
        }

        public void Add(T item)
        {
            AddAsync(item);
        }

        public T[] Flush()
        {
            return buf.FlushAll();
        }

        public async Task AddAsync(T item)
        {
            await ch.Writer.WriteAsync(item);
            //Logger.Debug("written!");
        }

        public void Run()
        {
            RunAsync();
        }

        public async Task RunAsync()
        {
            while (await ch.Reader.WaitToReadAsync())
            {
                var item = await ch.Reader.ReadAsync();
                int droppred = buf.Add(item);
                if(droppred > 0)
                {
                    Logger.Warn("bounded buffer overflow, dropped {0} old items", droppred);
                }
            }

            Logger.Info("accumulator stopped!");
        }

        public void Close()
        {
            ch.Writer.Complete();
        }
    }
}
