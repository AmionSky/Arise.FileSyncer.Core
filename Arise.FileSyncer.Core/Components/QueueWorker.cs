using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Arise.FileSyncer.Components
{
    internal class QueueWorker<T> : IDisposable
    {
        private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        private volatile AutoResetEvent signal = new AutoResetEvent(false);
        private readonly Action<T> action;
        private volatile Task task = null;
        private volatile bool exit = true;

        /// <summary>
        /// Checks if the queue is empty
        /// </summary>
        public bool IsEmpty => queue.IsEmpty;

        /// <summary>
        /// Checks if the queue is empty
        /// </summary>
        public bool Exiting => exit;

        /// <summary>
        /// Creates a QueueWorker
        /// </summary>
        /// <param name="work">Work to do on dequeued items</param>
        public QueueWorker(Action<T> work)
        {
            action = work;
        }

        /// <summary>
        /// Starts the queue worker task.
        /// Don't call from the worker's action.
        /// </summary>
        public void Start()
        {
            if (task != null)
            {
                exit = true;
                signal?.Set();
                task.Wait();
            }

            signal?.Reset();
            exit = false;
            task = Task.Factory.StartNew(Worker, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Stops the queue worker task.
        /// </summary>
        public void Stop()
        {
            if (task != null)
            {
                exit = true;
                signal?.Set();
            }
        }

        /// <summary>
        /// Enqueues the data and optionally signals the worker.
        /// </summary>
        /// <param name="data"></param>
        public void Enqueue(T data, bool signal)
        {
            queue.Enqueue(data);
            if (signal) this.signal?.Set();
        }

        /// <summary>
        /// Signals the worker.
        /// </summary>
        public void Signal()
        {
            signal?.Set();
        }

        /// <summary>
        /// Waits for the inner task to complete execution
        /// </summary>
        public void Wait()
        {
            task.Wait();
        }

        /// <summary>
        /// Waits for the inner task to complete execution
        /// </summary>
        public void Wait(TimeSpan timeout)
        {
            task.Wait(timeout);
        }

        private void Worker()
        {
            while (!exit)
            {
                while (!exit && !queue.IsEmpty)
                {
                    if (queue.TryDequeue(out T data))
                    {
                        action.Invoke(data);
                    }
                }

                if (!exit) signal?.WaitOne();
            }

            task = null;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    exit = true;

                    if (signal != null)
                    {
                        signal.Set();
                        signal.Dispose();
                        signal = null;
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
