using System.Collections.Generic;
using System.Threading;

namespace ZipThreading.CollectionProcessorThreadPool
{
    public class CollectionProcessorThreadPool<T>
    {
        private readonly Queue<T> _processingQueue; 

        private readonly List<Thread> _pool;

        private readonly object _queueLock = new object();

        private readonly object _poolLock = new object();

        private CollectionProcessorCallback<T> _workItem;

        private Thread _workItemScheduler;

        private int _currentThreadCount = 0;

        public CollectionProcessorThreadPool(CollectionProcessorCallback<T> workItem)
        {
            _processingQueue = new Queue<T>();
            _pool = new List<Thread>();
            _workItem = workItem;

            //InitializeScheduler();

            StartPool();
        }

        private void StartPool()
        {
            for (var i = 0; i < MultithreadingUtils.OptimalThreadsCount; i++)
            {
                var thread = new Thread(DispatchWorkItem);
                _pool.Add(thread);
                thread.Start();
            }
        }

        private void DispatchWorkItem()
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

                    collectionItem = _processingQueue.Dequeue();
                }
                
                _workItem(collectionItem);
            }
        }

        public void QueueCollectionItem(T item)
        {
            lock (_queueLock)
            {
                _processingQueue.Enqueue(item);
                //Raise event
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
