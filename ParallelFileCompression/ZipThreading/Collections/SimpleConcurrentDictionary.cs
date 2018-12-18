using System.Collections.Generic;
using System.Threading;

namespace ZipThreading.Collections
{
    public sealed class SimpleConcurrentDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _internalDictionary;

        private readonly object _lock = new object();

        private volatile bool _isExecutionAborted = false;

        public SimpleConcurrentDictionary()
        {
            _internalDictionary = new Dictionary<TKey, TValue>();
        }

        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                _internalDictionary.Add(key, value);
            }
        }

        public bool TryTakeAndRemove(TKey key, out TValue value, bool waitForValue = false)
        {
            lock (_lock)
            {
                while (waitForValue && !_internalDictionary.ContainsKey(key) && !_isExecutionAborted)
                {
                    Monitor.Wait(_lock);
                }

                if (!_internalDictionary.ContainsKey(key))
                {
                    value = default(TValue);
                    return false;
                }

                value = _internalDictionary[key];
                _internalDictionary.Remove(key);
                return true;
            }
        }

        public void AbortExecutions()
        {
            lock (_lock)
            {
                _isExecutionAborted = true;
                Monitor.PulseAll(_lock);
            }
        }
    }
}
