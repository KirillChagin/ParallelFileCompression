using System;
using System.Collections.Generic;
using System.Threading;
using ZipThreading.Collections;

namespace ZipThreading.CollectionProcessorThreadPool
{
    /// <summary>
    /// Represents a simple threadpool-like collection processor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CollectionProcessorThreadPool<T>
    {
        /// <summary>
        /// The <i>maximum</i> number of threads in a pool
        /// </summary>
        public static int OptimalThreadsCount => Environment.ProcessorCount;

        private readonly SimpleConcurrentQueue<T> _processingQueue;

        private readonly CollectionItemProcessorCallback<T> _workItem;

        private List<Thread> _pool;

        private WaitHandle[] _waitHandles;

        /// <summary>
        /// Initializes a new instance of the collection processor
        /// </summary>
        /// <param name="workItem">Callback for each thread, that processes collection item</param>
        public CollectionProcessorThreadPool(CollectionItemProcessorCallback<T> workItem) : this(workItem, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the collection processor
        /// </summary>
        /// <param name="workItem">Callback for each thread, that processes collection item</param>
        /// <param name="processingQueue">Processing collection</param>
        public CollectionProcessorThreadPool(CollectionItemProcessorCallback<T> workItem, SimpleConcurrentQueue<T> processingQueue)
        {
            _processingQueue = processingQueue ?? new SimpleConcurrentQueue<T>();

            _workItem = workItem;

            InitPool();
        }

        //TODO: add threads when needed = change waiting mechanism
        /// <summary>
        /// Starts all threads in the pool
        /// </summary>
        public void StartPool()
        {
            for (var i = 0; i < OptimalThreadsCount; i++)
            {
                var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                _waitHandles[i] = waitHandle;

                _pool[i].Start(waitHandle);
            }
        }

        /// <summary>
        /// Wait for all threads completed
        /// </summary>
        public void WaitAll()
        {
            WaitHandle.WaitAll(_waitHandles);
        }

        private void InitPool()
        {
            _pool = new List<Thread>(OptimalThreadsCount);
            for (var i = 0; i < OptimalThreadsCount; i++)
            {
                var thread = new Thread(DispatchWorkItem);
                _pool.Add(thread);
            }

            _waitHandles = new WaitHandle[OptimalThreadsCount];
        }

        private void DispatchWorkItem(object o)
        {
            while (true)
            {
                var dequeueResult = _processingQueue.TryDequeue(out var collectionItem, true);

                if (dequeueResult)
                {
                    _workItem(collectionItem);
                }
                else
                {
                    var waitHandle = (EventWaitHandle)o;
                    waitHandle.Set();

                    break;
                }
            }
        }
    }
}
