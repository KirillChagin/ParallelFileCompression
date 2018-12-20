using System;
using System.Collections.Generic;
using System.Threading;

namespace ZipThreading.Collections
{
    public sealed class SimpleConcurrentDictionary<TKey, TValue>
    {
        /// <summary>
        /// The maximum number of pairs that could be stored in the dictionary at one time
        /// </summary>
        public int MaximumCapacity = Environment.ProcessorCount * 4;

        private readonly Dictionary<TKey, TValue> _internalDictionary;

        private readonly object _lock = new object();

        private volatile bool _isFillingComplete = false;

        /// <summary>
        /// Initializes a new instance of a thread-safe wrapper for a <see cref="Dictionary{TKey,TValue}"/>. 
        /// </summary>
        public SimpleConcurrentDictionary()
        {
            _internalDictionary = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Adds an <see cref="TValue"/> object with <see cref="TKey"/> key to the dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
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

        /// <summary>
        /// Tries to remove and return the next item from the dictionary by specified key.
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="value">Return value</param>
        /// <param name="waitForValue">Block the thread, while the dictionary not contain specified key</param>
        /// <returns></returns>
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

        /// <summary>
        /// Indicate the dictionary, that all adding operations finished
        /// </summary>
        public void CompleteFilling()
        {
            lock (_lock)
            {
                _isFillingComplete = true;
                Monitor.PulseAll(_lock);
            }
        }

        /// <summary>
        /// Force stop and clear
        /// </summary>
        public void Flush()
        {
            lock (_lock)
            {
                _isFillingComplete = true;
                _internalDictionary.Clear();
                Monitor.PulseAll(_lock);
            }
        }
    }
}
