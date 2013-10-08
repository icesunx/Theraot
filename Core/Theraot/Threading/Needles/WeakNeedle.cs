using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace Theraot.Threading.Needles
{
    [global::System.Diagnostics.DebuggerNonUserCode]
    public partial class WeakNeedle<T> : INeedle<T>, IEquatable<WeakNeedle<T>>
        where T : class
    {
        private GCHandle _handle;
        private int _hashCode;
        private int _managedDisposal;
        private bool _trackResurrection;

        public WeakNeedle()
            : this(false)
        {
            //Empty
        }

        public WeakNeedle(bool trackResurrection)
        {
            _trackResurrection = trackResurrection;
            _hashCode = base.GetHashCode();
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public WeakNeedle(T target)
            : this(target, false)
        {
            //Empty
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public WeakNeedle(T target, bool trackResurrection)
        {
            _trackResurrection = trackResurrection;
            Allocate(target, _trackResurrection);
            if (IsAliveExtracted())
            {
                _hashCode = target.GetHashCode();
            }
            else
            {
                _hashCode = base.GetHashCode();
            }
        }

        public virtual bool IsAlive
        {
            get
            {
                return IsAliveExtracted();
            }
        }

        public virtual bool TrackResurrection
        {
            get
            {
                return _trackResurrection;
            }
        }

        public virtual T Value
        {
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            get
            {
                T value;
                TryGetTarget(out value);
                return value;
            }
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            set
            {
                Allocate(value, _trackResurrection);
            }
        }

        public static explicit operator T(WeakNeedle<T> needle)
        {
            if (ReferenceEquals(needle, null))
            {
                throw new ArgumentNullException("needle");
            }
            else
            {
                return needle.Value;
            }
        }

        public static implicit operator WeakNeedle<T>(T field)
        {
            return new WeakNeedle<T>(field);
        }

        public static bool operator !=(WeakNeedle<T> left, WeakNeedle<T> right)
        {
            return NotEqualsExtracted(left, right);
        }

        public static bool operator ==(WeakNeedle<T> left, WeakNeedle<T> right)
        {
            return EqualsExtracted(left, right);
        }

        public override bool Equals(object obj)
        {
            var _obj = obj as WeakNeedle<T>;
            if (!ReferenceEquals(null, _obj))
            {
                return EqualsExtractedExtracted(this, _obj);
            }
            else
            {
                if (obj is T)
                {
                    var target = Value;
                    if (IsAlive)
                    {
                        return EqualityComparer<T>.Default.Equals(target, (T)obj);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public bool Equals(WeakNeedle<T> other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            else
            {
                return EqualsExtractedExtracted(this, other);
            }
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public void Release()
        {
            Dispose();
        }

        public override string ToString()
        {
            var target = Value;
            if (IsAlive)
            {
                return target.ToString();
            }
            else
            {
                return "<Dead Needle>";
            }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public bool TryGetTarget(out T target)
        {
            target = default(T);
            if (!_handle.IsAllocated)
            {
                return false;
            }
            else
            {
                try
                {
                    object obj = _handle.Target;
                    if (obj == null)
                    {
                        return false;
                    }
                    else
                    {
                        target = (T)obj;
                        return true;
                    }
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        protected void Allocate(T value, bool trackResurrection)
        {
            var suspention = SuspendDisposal();
            if (ReferenceEquals(suspention, null))
            {
                ReleaseExtracted();
                _handle = GetNewHandle(value, trackResurrection);
                if (Interlocked.CompareExchange(ref _managedDisposal, 0, 1) == 1)
                {
                    GC.ReRegisterForFinalize(this);
                }
                UnDispose();
            }
            else
            {
                using (suspention)
                {
                    var oldHandle = _handle;
                    _handle = GetNewHandle(value, trackResurrection);
                    if (oldHandle.IsAllocated)
                    {
                        oldHandle.Free();
                        try
                        {
                            oldHandle.Free();
                        }
                        catch (InvalidOperationException)
                        {
                            //Empty
                        }
                    }
                }
            }
        }

        private static bool EqualsExtracted(WeakNeedle<T> left, WeakNeedle<T> right)
        {
            if (ReferenceEquals(left, null))
            {
                if (ReferenceEquals(right, null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (ReferenceEquals(right, null))
                {
                    return false;
                }
                else
                {
                    return EqualsExtractedExtracted(left, right);
                }
            }
        }

        private static bool EqualsExtractedExtracted(WeakNeedle<T> left, WeakNeedle<T> right)
        {
            var _left = left.Value;
            if (left.IsAlive)
            {
                var _right = right.Value;
                if (right.IsAlive)
                {
                    return EqualityComparer<T>.Default.Equals(_left, _right);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return !right.IsAlive;
            }
        }

        private static bool NotEqualsExtracted(WeakNeedle<T> left, WeakNeedle<T> right)
        {
            if (ReferenceEquals(left, null))
            {
                if (ReferenceEquals(right, null))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (ReferenceEquals(right, null))
                {
                    return true;
                }
                else
                {
                    return NotEqualsExtractedExtracted(left, right);
                }
            }
        }

        private static bool NotEqualsExtractedExtracted(WeakNeedle<T> left, WeakNeedle<T> right)
        {
            var _left = left.Value;
            if (left.IsAlive)
            {
                var _right = right.Value;
                if (right.IsAlive)
                {
                    return !EqualityComparer<T>.Default.Equals(_left, _right);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return right.IsAlive;
            }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        private GCHandle GetNewHandle(T value, bool trackResurrection)
        {
            return GCHandle.Alloc(value, trackResurrection ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        private bool IsAliveExtracted()
        {
            if (_handle.IsAllocated)
            {
                try
                {
                    if (ReferenceEquals(_handle.Target, null))
                    {
                        _handle.Free();
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        private void ReleaseExtracted()
        {
            if (_handle.IsAllocated)
            {
                try
                {
                    _handle.Free();
                }
                catch (InvalidOperationException)
                {
                    //Empty
                }
            }
        }

        private void ReportManagedDisposal()
        {
            Thread.VolatileWrite(ref _managedDisposal, 1);
        }
    }
}