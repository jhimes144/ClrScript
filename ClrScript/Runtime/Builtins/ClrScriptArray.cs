using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClrScript.Runtime.Builtins
{
    public abstract class ClrScriptArray { }

    public class ClrScriptArray<T> : ClrScriptArray, IList<T>, IList, IReadOnlyList<T>
    {
        readonly List<T> _contents = new List<T>();

        [ClrScriptMember]
        public T this[double index]
        {
            get => _contents[(int)index];
        }

        public T this[int index]
        {
            get => _contents[index];
        }

        T IList<T>.this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        object IList.this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public bool IsSynchronized => false;

        public object SyncRoot => null;

        int IReadOnlyCollection<T>.Count => _contents.Count;

        int ICollection<T>.Count => _contents.Count;

        int ICollection.Count => _contents.Count;

        // NOTICE: A few optimizations count on this method being named 'add' and 'AddClr'.
        [ClrScriptMember(NameOverride = "add")]
        public void AddClr(T item)
        {
            _contents.Add(item);
        }

        public void Add(T item)
        {
            _contents.Add(item);
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public double Count()
        {
            return _contents.Count;
        }

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public void Clear()
        {
            _contents.Clear();
        }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public bool Contains(T item)
        {
            return _contents.Contains(item);
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
