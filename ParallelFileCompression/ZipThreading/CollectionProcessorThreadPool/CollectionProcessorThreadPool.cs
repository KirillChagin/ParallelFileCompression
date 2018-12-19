using System;
using System.Collections.Generic;
using System.Threading;
using ZipThreading.Collections;

namespace ZipThreading.CollectionProcessorThreadPool
{
    public class CollectionProcessorThreadPool<T>
    {
        public static int OptimalThreadsCount => Environment.ProcessorCount;

        private readonly SimpleConcurrentQueue<T> _processingQueue;

        private readonly CollectionItemProcessorCallback<T> _workItem;

        private List<Thread> _pool;

        private WaitHandle[] _waitHandles;

        public CollectionProcessorThreadPool(CollectionItemProcessorCallback<T> workItem) : this(workItem, null)
        {
        }

        public CollectionProcessorThreadPool(CollectionItemProcessorCallback<T> workItem, SimpleConcurrentQueue<T> processingQueue)
        {
            _processingQueue = processingQueue ?? new SimpleConcurrentQueue<T>();

            _workItem = workItem;

            InitPool();
        }

        //TODO: add threads when needed
        public void StartPool()
        {
            for (var i = 0; i < OptimalThreadsCount; i++)
            {
                var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                _waitHandles[i] = waitHandle;

                _pool[i].Start(waitHandle);
            }
        }

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
