using System.Collections.Generic;

namespace SkyApm.Common
{
    public class ConcurrentPriorityQueue<T> : PriorityQueue<T> where T : class
    {
        private readonly object _locker = new object();

        public ConcurrentPriorityQueue(IComparer<T> comparer) : base(comparer)
        {
        }

        public ConcurrentPriorityQueue() : this(Comparer<T>.Default)
        {
        }

        public override int Count
        {
            get
            {
                lock (_locker)
                {
                    return base.Count;
                }
            }
        }

        public override bool TryDequeue(out T value)
        {
            lock (_locker)
            {
                return base.TryDequeue(out value);
            }
        }

        public override bool TryPeek(out T value)
        {
            lock (_locker)
            {
                return base.TryPeek(out value);
            }
        }

        public override void Enqueue(T item)
        {
            lock (_locker)
            {
                base.Enqueue(item);
            }
        }

        public override void Remove(T item)
        {
            lock (_locker)
            {
                base.Remove(item);
            }
        }

        public override IEnumerator<T> GetEnumerator()
        {
            lock (_locker)
            {
                return base.GetEnumerator();
            }
        }

        public override void Clear()
        {
            lock (_locker)
            {
                base.Clear();
            }
        }
    }
}
