using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace SkyApm.Common
{
    public class PriorityQueue<T> : IEnumerable<T>
    {
        readonly IComparer<T> _comparer;
        int _count;
        int _capacity;
        T[] _items;

        public PriorityQueue(IComparer<T> comparer)
        {
            _comparer = comparer;
            _capacity = 11;
            _items = new T[_capacity];
        }

        public PriorityQueue() : this(Comparer<T>.Default)
        {
        }

        public virtual int Count => _count;

        public virtual bool TryDequeue(out T value)
        {
            if (!TryPeek(out var result))
            {
                value = default;
                return false;
            }

            int newCount = --_count;
            T lastItem = _items[newCount];
            _items[newCount] = default;
            if (newCount > 0)
            {
                TrickleDown(0, lastItem);
            }

            value = result;
            return true;
        }

        public virtual bool TryPeek(out T value)
        {
            if (_count == 0)
            {
                value = default;
                return false;
            }
            else
            {
                value = _items[0];
                return true;
            }
        }

        public virtual void Enqueue(T item)
        {
            Contract.Requires(item != null);

            int oldCount = _count;
            if (oldCount == _capacity)
            {
                GrowHeap();
            }
            _count = oldCount + 1;
            BubbleUp(oldCount, item);
        }

        public virtual void Remove(T item)
        {
            int index = Array.IndexOf(_items, item);
            if (index == -1)
            {
                return;
            }

            _count--;
            if (index == _count)
            {
                _items[index] = default(T);
            }
            else
            {
                T last = _items[_count];
                _items[_count] = default(T);
                TrickleDown(index, last);
                if (EqualityComparer<T>.Default.Equals(_items[index], last))
                {
                    BubbleUp(index, last);
                }
            }
        }

        void BubbleUp(int index, T item)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) >> 1;
                T parentItem = _items[parentIndex];
                if (_comparer.Compare(item, parentItem) >= 0)
                {
                    break;
                }
                _items[index] = parentItem;
                index = parentIndex;
            }
            _items[index] = item;
        }

        void GrowHeap()
        {
            int oldCapacity = _capacity;
            _capacity = oldCapacity + (oldCapacity <= 64 ? oldCapacity + 2 : (oldCapacity >> 1));
            var newHeap = new T[_capacity];
            Array.Copy(_items, 0, newHeap, 0, _count);
            _items = newHeap;
        }

        void TrickleDown(int index, T item)
        {
            int middleIndex = _count >> 1;
            while (index < middleIndex)
            {
                int childIndex = (index << 1) + 1;
                T childItem = _items[childIndex];
                int rightChildIndex = childIndex + 1;
                if (rightChildIndex < _count
                    && _comparer.Compare(childItem, _items[rightChildIndex]) > 0)
                {
                    childIndex = rightChildIndex;
                    childItem = _items[rightChildIndex];
                }
                if (_comparer.Compare(item, childItem) <= 0)
                {
                    break;
                }
                _items[index] = childItem;
                index = childIndex;
            }
            _items[index] = item;
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual void Clear()
        {
            _count = 0;
            Array.Clear(_items, 0, 0);
        }
    }
}
