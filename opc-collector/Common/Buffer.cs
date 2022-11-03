using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcCollector.Common
{
    public class Buffer<T>
    {
        private List<T> buf;
        private int maxLen = 100;
        private int last = -1;

        public Buffer() : this(100) { }

        public Buffer(int maxLen)
        {
            this.maxLen = maxLen;
            buf = new List<T>(maxLen);
        }

        public int Len()
        {
            lock(buf)
            {
                return length();
            }
        }

        private int length()
        {
            return buf.Count;
        }

        public int Add(T[] items)
        {
            int dropped = 0;

            lock(buf)
            {
                foreach(var item in items)
                {
                    dropped += add(item);
                }    
            }

            return dropped;
        }

        public int Add(T item)
        {
            lock(buf)
            {
                return add(item);
            }
        }

        private int add(T item)
        {
            int dropped = 0;

            if(buf.Count == maxLen)
            {
                last = next(last);
                buf[last] = item;
                dropped++;
            }
            else
            {
                buf.Add(item);
                last = buf.Count - 1;
            }

            return dropped;
        }

        private int next(int index)
        {
            index++;
            if(index == buf.Count)
            {
                return 0;
            }

            return index;
        }

        private int reset()
        {
            int dropped = buf.Count;
            buf.Clear();
            last = -1;

            return dropped;
        }

        public T[] FlushAll()
        {
            T[] items = { };

            lock(buf) {
                items = buf.ToArray();
                reset();
            }

            return items;
        }
    }
}
