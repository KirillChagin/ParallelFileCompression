using System;
using System.Collections.Generic;
using System.Threading;

namespace ZipThreading.Collections
{
    public sealed class SimpleConcurrentDictionary<TKey, TValue>
    {
        public int MaximumCapacity = Environment.ProcessorCount * 4;

        private readonly Dictionary<TKey, TValue> _internalDictionary;

        private readonly object _lock = new object();

        private volatile bool _isFillingComplete = false;

        public SimpleConcurrentDictionary()
        {
            _internalDictionary = new Dictionary<TKey, TValue>();
        }

        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                //For ~constant memory usage
                while (_internalDictionary.Count >= MaximumCapacity)
                {
                    Monitor.Wait(_lock);
                }

                _internalDictionary.Add(key, value);
                Monitor.PulseAll(_lock);
            }
        }

        public bool TryTakeAndRemove(TKey key, out TValue value, bool waitForValue = false)
        {
            lock (_lock)
            {
                while (waitForValue && !_internalDictionary.ContainsKey(key) && !_isFillingComplete)
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
                Monitor.PulseAll(_lock);
                return true;
            }
        }

        public void CompleteFilling()
        {
            lock (_lock)
            {
                _isFillingComplete = true;
                Monitor.PulseAll(_lock);
            }
        }
    }
}
