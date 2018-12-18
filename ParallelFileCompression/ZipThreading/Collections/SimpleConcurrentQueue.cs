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
        private readonly Queue<T> _internalQueue;

        private readonly object _lock = new object();

        private volatile bool _isExecutionAborted = false;

        public SimpleConcurrentQueue()
        {
            _internalQueue = new Queue<T>();
        }

        public void Enqueue(T item)
        {
            lock (_lock)
            {
                _internalQueue.Enqueue(item);
            }
        }

        public bool TryDequeue(out T result, bool waitForResult = false)
        {
            lock (_lock)
            {
                while (waitForResult && _internalQueue.Count == 0 && !_isExecutionAborted)
                {
                    Monitor.Wait(_lock);
                }

                if (_internalQueue.Count == 0)
                {
                    result = default(T);
                    return false;
                }

                result = _internalQueue.Dequeue();
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
