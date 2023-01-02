using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZetanStudio.Collections
{
    public class Heap<T> where T : class, IHeapItem<T>
    {
        private readonly T[] items;
        private readonly int maxSize;
        private readonly HeapType heapType;

        public int Count { get; private set; }

        public Heap(int size, HeapType heapType = HeapType.MinHeap)
        {
            items = new T[size];
            maxSize = size;
            this.heapType = heapType;
        }

        public void Add(T item)
        {
            if (Count >= maxSize) return;
            item.HeapIndex = Count;
            items[Count] = item;
            Count++;
            SortUpForm(item);
        }

        public T RemoveRoot()
        {
            if (Count < 1) return default;
            T root = items[0];
            root.HeapIndex = -1;
            Count--;
            if (Count > 0)
            {
                items[0] = items[Count];
                items[0].HeapIndex = 0;
                SortDownFrom(items[0]);
            }
            return root;
        }

        public bool Contains(T item)
        {
            if (item == default || item.HeapIndex < 0 || item.HeapIndex > Count - 1) return false;
            return Equals(items[item.HeapIndex], item);//用items.Contains()就等着哭吧
        }

        public void Clear()
        {
            Count = 0;
        }

        public bool Exists(Predicate<T> predicate)
        {
            return Array.Exists(items, predicate);
        }

        public T[] ToArray()
        {
            return items;
        }

        public List<T> ToList()
        {
            return items.ToList();
        }

        private void SortUpForm(T item)
        {
            int parentIndex = (int)((item.HeapIndex - 1) * 0.5f);
            while (true)
            {
                T parent = items[parentIndex];
                if (Equals(parent, item)) return;
                if (heapType == HeapType.MinHeap ? item.CompareTo(parent) < 0 : item.CompareTo(parent) > 0)
                {
                    if (!Swap(item, parent))
                        return;//交换不成功则退出，防止死循环
                }
                else return;
                parentIndex = (int)((item.HeapIndex - 1) * 0.5f);
            }
        }

        private void SortDownFrom(T item)
        {
            while (true)
            {
                int leftChildIndex = item.HeapIndex * 2 + 1;
                int rightChildIndex = item.HeapIndex * 2 + 2;
                if (leftChildIndex < Count)
                {
                    int swapIndex = leftChildIndex;
                    if (rightChildIndex < Count && (heapType == HeapType.MinHeap ?
                        items[rightChildIndex].CompareTo(items[leftChildIndex]) < 0 : items[rightChildIndex].CompareTo(items[leftChildIndex]) > 0))
                        swapIndex = rightChildIndex;
                    if (heapType == HeapType.MinHeap ? items[swapIndex].CompareTo(item) < 0 : items[swapIndex].CompareTo(item) > 0)
                    {
                        if (!Swap(item, items[swapIndex]))
                            return;//交换不成功则退出，防止死循环
                    }
                    else return;
                }
                else return;
            }
        }

        public void Update()
        {
            if (Count < 1) return;
            SortDownFrom(items[0]);
            SortUpForm(items[Count - 1]);
        }

        private bool Swap(T item1, T item2)
        {
            if (!Contains(item1) || !Contains(item2)) return false;
            items[item1.HeapIndex] = item2;
            items[item2.HeapIndex] = item1;
            int item1Index = item1.HeapIndex;
            item1.HeapIndex = item2.HeapIndex;
            item2.HeapIndex = item1Index;
            return true;
        }

        public static implicit operator bool(Heap<T> obj)
        {
            return obj != null;
        }

        public enum HeapType
        {
            MinHeap,
            MaxHeap
        }
    }

    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex { get; set; }
    }

    public class ReadOnlySet<T> : ISet<T>, IReadOnlyCollection<T>
    {
        private readonly ISet<T> set;

        public int Count => set.Count;

        public bool IsReadOnly => true;

        public ReadOnlySet(ISet<T> set)
        {
            this.set = set;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)set).GetEnumerator();
        }

        bool ISet<T>.Add(T item)
        {
            throw new InvalidOperationException("只读");
        }

        void ISet<T>.ExceptWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("只读");
        }

        void ISet<T>.IntersectWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("只读");
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return set.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return set.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return set.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return set.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return set.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return set.Equals(other);
        }

        void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("只读");
        }

        void ISet<T>.UnionWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("只读");
        }

        void ICollection<T>.Add(T item)
        {
            throw new InvalidOperationException("只读");
        }

        void ICollection<T>.Clear()
        {
            throw new InvalidOperationException("只读");
        }

        public bool Contains(T item)
        {
            return set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            set.CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new InvalidOperationException("只读");
        }
    }
}