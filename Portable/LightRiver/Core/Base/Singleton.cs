using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LightRiver
{
#if !NET45
    public sealed class Singleton<T>
        where T : class
    {
        private readonly object _lockObj = new object();
        private readonly Func<T> _delegate;
        private bool _isDelegateInvoked;

        private T _value;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Singleton&lt;T&gt;"/> class.
        /// </summary>
        public Singleton()
            : this(() => default(T))
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Singleton&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="delegate">The @delegate.</param>
        public Singleton(Func<T> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public T Value
        {
            get
            {
                if (_isDelegateInvoked)
                    return _value;

                T temp = _delegate();
                Interlocked.CompareExchange<T>(ref _value, temp, null);

                bool lockTaken = false;

                try {
                    // WP7 does not support the overload with the
                    // Boolean indicating if the lock was taken.
                    Monitor.Enter(_lockObj);
                    lockTaken = true;

                    _isDelegateInvoked = true;
                }
                finally {
                    if (lockTaken) {
                        Monitor.Exit(_lockObj);
                    }
                }

                return _value;
            }
        }
    }
#endif
}
