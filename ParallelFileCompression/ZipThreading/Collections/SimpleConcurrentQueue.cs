using System;
using System.Collections.Generic;
using System.Threading;

namespace ZipThreading.Collections
{
    /// <summary>
    /// Simple thread-safe wrapper for <see cref="Queue{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SimpleConcurrentQueue<T>
    {
        /// <summary>
        /// The maximum number of pairs that could be stored in the queue at one time
        /// </summary>
        public int MaximumCapacity = Environment.ProcessorCount * 4;

        private readonly Queue<T> _internalQueue;

        private readonly object _lock = new object();

        private volatile bool _isFillingComplete;

        /// <summary>
        /// Initializes a new instance of a thread-safe wrapper for a FIFO queue. 
        /// </summary>
        public SimpleConcurrentQueue()
        {
            _internalQueue = new Queue<T>();
        }

        /// <summary>
        /// Adds an <see cref="T"/> object to the queue
        /// </summary>
        /// <param name="item">Object to add</param>
        public void Enqueue(T item)
        {
            lock (_lock)
            {
                //For ~constant memory usage
                while (_internalQueue.Count >= MaximumCapacity)
                {
                    Monitor.Wait(_lock);
                }

                _internalQueue.Enqueue(item);
                Monitor.PulseAll(_lock);
            }
        }

        /// <summary>
        /// Tries to remove and return the next item from the queue.
        /// If <param name="waitForResult"> is true, the method will wait for the new added item</param>
        /// </summary>
        /// <param name="result">Return object</param>
        /// <param name="waitForResult">Block the thread, while collection is empty and not completed writing</param>
        /// <returns>True if success. False if collection is empty and procession is finished</returns>
        public bool TryDequeue(out T result, bool waitForResult = false)
        {
            lock (_lock)
            {
                while (waitForResult && _internalQueue.Count == 0 && !_isFillingComplete)
                {
                    Monitor.Wait(_lock);
                }

                if (_internalQueue.Count == 0)
                {
                    result = default(T);
                    return false;
                }

                result = _internalQueue.Dequeue();
                Monitor.PulseAll(_lock);
                return true;
            }
        }

        /// <summary>
        /// Indicate the collection, that all adding operations finished
        /// </summary>
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
