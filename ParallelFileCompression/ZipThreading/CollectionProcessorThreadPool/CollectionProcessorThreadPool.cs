using System.Collections.Generic;
using System.Threading;
using ZipThreading.Collections;

namespace ZipThreading.CollectionProcessorThreadPool
{
    public class CollectionProcessorThreadPool<T>
    {
        private readonly SimpleConcurrentQueue<T> _processingQueue;

        private readonly CollectionItemProcessorCallback<T> _workItem;

        private List<Thread> _pool;

        private WaitHandle[] _waitHandles;

        //private Thread _workItemScheduler;

        //private int _currentThreadCount = 0;

        public CollectionProcessorThreadPool(CollectionItemProcessorCallback<T> workItem) : this(workItem, null)
        {
        }

        public CollectionProcessorThreadPool(CollectionItemProcessorCallback<T> workItem, SimpleConcurrentQueue<T> processingQueue)
        {
            _processingQueue = processingQueue ?? new SimpleConcurrentQueue<T>();

            _workItem = workItem;

            InitPool();
        }


        //TODO: clean pool.
        //TODO: add threads when needed
        public void StartPool()
        {
            for (var i = 0; i < MultithreadingUtils.OptimalThreadsCount; i++)
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
            _pool = new List<Thread>(MultithreadingUtils.OptimalThreadsCount);
            for (var i = 0; i < MultithreadingUtils.OptimalThreadsCount; i++)
            {
                var thread = new Thread(DispatchWorkItem);
                _pool.Add(thread);
            }

            _waitHandles = new WaitHandle[MultithreadingUtils.OptimalThreadsCount];
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
       
        /*private void InitializeScheduler()
        {
            _workItemScheduler = new Thread(() =>
            {
                while (true)
                {
                    T collectionItem;
                    lock (_queueLock)
                    {
                        //TODO: add exit point
                        while (_processingQueue.Count == 0)
                        {
                            Monitor.Wait(_queueLock);
                        }

                        //collectionItem = _processingQueue.Dequeue();
                    }


                    DispatchWorkItem(collectionItem);
                }
            });

            _workItemScheduler.Priority = ThreadPriority.AboveNormal;
            _workItemScheduler.Start();
        }

        private void DispatchWorkItem(T collectionItem)
        {
            
        }*/
    }
}
