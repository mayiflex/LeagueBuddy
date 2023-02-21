using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBuddy.Main.DataCsses
{
    internal class ConcurrentQueueEnqueueEvent<T>
    {
        private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        public event EventHandler Enqueued;
        public void Enqueue(T item)
        {
            queue.Enqueue(item);
            if (Enqueued != null) Enqueued(this, EventArgs.Empty);
        }
        public int Count { get { return queue.Count; } }

        public T? Dequeue()
        {
            try
            {
                T item;
                queue.TryDequeue(out item);
                return item;
            }
            catch
            {
                return default;
            }
        }
    }
}
