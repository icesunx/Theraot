﻿using System;
using System.Collections.Generic;
using System.Threading;
using Theraot.Core;

namespace Theraot.Collections.Specialized
{
    [Serializable()]
    public sealed class FlagArray : ICollection<bool>, IExtendedCollection<bool>, IEnumerable<bool>, ICloneable<FlagArray>, IList<bool>
    {
        private IReadOnlyCollection<bool> _asReadOnly;
        private int[] _data;
        private int _length;

        public FlagArray(FlagArray prototype)
        {
            if (ReferenceEquals(prototype, null))
            {
                throw new ArgumentNullException("prototype", "prototype is null.");
            }
            else
            {
                _length = prototype._length;
                _data = new int[GetLength(_length)];
                prototype._data.CopyTo(_data, 0);
                _asReadOnly = new ExtendedReadOnlyCollection<bool>(this);
            }
        }

        public FlagArray(int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", "length < 0");
            }
            else
            {
                _length = length;
                _data = new int[GetLength(_length)];
                _asReadOnly = new ExtendedReadOnlyCollection<bool>(this);
            }
        }

        public FlagArray(int length, bool defaultValue)
            : this(length)
        {
            if (defaultValue)
            {
                Fill(defaultValue);
            }
        }

        public int Count
        {
            get
            {
                return _length;
            }
        }

        public IEnumerable<int> Flags
        {
            get
            {
                int count = 0;
                foreach (var item in _data)
                {
                    foreach (var bit in item.BitsBinary())
                    {
                        if (bit == 1)
                        {
                            yield return count;
                        }
                        count++;
                        if (count == _length)
                        {
                            yield break;
                        }
                    }
                }
            }
        }

        bool ICollection<bool>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        IReadOnlyCollection<bool> IExtendedCollection<bool>.AsReadOnly
        {
            get
            {
                return _asReadOnly;
            }
        }

        public bool this[int index]
        {
            get
            {
                var _index = index >> 5;
                var bit = index & 31;
                var mask = 1 << bit;
                return GetBit(_index, mask);
            }
            set
            {
                var _index = index >> 5;
                var bit = index & 31;
                var mask = 1 << bit;
                if (value)
                {
                    SetBit(_index, mask);
                }
                else
                {
                    UnsetBit(_index, mask);
                }
            }
        }

        public FlagArray Clone()
        {
            return new FlagArray(this);
        }

        public bool Contains(bool item)
        {
            return System.Linq.Enumerable.Contains(this, item);
        }

        public bool Contains(bool item, IEqualityComparer<bool> comparer)
        {
            return System.Linq.Enumerable.Contains(this, item, comparer);
        }

        public void CopyTo(bool[] array, int arrayIndex)
        {
            Extensions.CanCopyTo(_length, array, arrayIndex);
            Extensions.CopyTo(this, array, arrayIndex);
        }

        public void CopyTo(bool[] array)
        {
            Extensions.CanCopyTo(_length, array);
            Extensions.CopyTo(this, array);
        }

        public void CopyTo(bool[] array, int arrayIndex, int countLimit)
        {
            Extensions.CanCopyTo(array, arrayIndex, countLimit);
            Extensions.CopyTo(this, array, arrayIndex, countLimit);
        }

        public IEnumerator<bool> GetEnumerator()
        {
            int count = 0;
            foreach (var item in _data)
            {
                foreach (var bit in item.BitsBinary())
                {
                    yield return bit == 1;
                    count++;
                    if (count == _length)
                    {
                        yield break;
                    }
                }
            }
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        void ICollection<bool>.Add(bool item)
        {
            throw new NotSupportedException();
        }

        void ICollection<bool>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<bool>.Remove(bool item)
        {
            throw new NotSupportedException();
        }

        bool IExtendedCollection<bool>.Remove(bool item, IEqualityComparer<bool> comparer)
        {
            throw new NotSupportedException();
        }

        void IList<bool>.Insert(int index, bool item)
        {
            throw new NotSupportedException();
        }

        void IList<bool>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public int IndexOf(bool item)
        {
            return Extensions.IndexOf(this, item);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void Fill(bool value)
        {
            int _value = value ? unchecked((int)0xffffffff) : 0;
            for (int index = 0; index < _length; index++)
            {
                _data[index] = _value;
            }
        }

        private bool GetBit(int index, int mask)
        {
            return (Thread.VolatileRead(ref _data[index]) & mask) != 0;
        }

        private int GetLength(int length)
        {
            return (length >> 5) + (length & 31) == 0 ? 0 : 1;
        }

        private void SetBit(int index, int mask)
        {
        again:
            int readed = Thread.VolatileRead(ref _data[index]);
            if ((readed & mask) == 0)
            {
                if (Interlocked.CompareExchange(ref _data[index], readed | mask, readed) != readed)
                {
                    goto again;
                }
            }
        }

        private void UnsetBit(int index, int mask)
        {
            mask = ~mask;
        again:
            int readed = Thread.VolatileRead(ref _data[index]);
            if ((readed & mask) != 0)
            {
                if (Interlocked.CompareExchange(ref _data[index], readed & mask, readed) != readed)
                {
                    goto again;
                }
            }
        }
    }
}