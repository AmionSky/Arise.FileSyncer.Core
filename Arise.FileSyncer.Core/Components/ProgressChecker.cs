using System;
using System.Threading;

namespace Arise.FileSyncer.Core.Components
{
    /// <summary>
    /// Periodically checks the progress of the connection.
    /// Issues a timeout if no progress was made between 2 checks.
    /// </summary>
    internal sealed class ProgressChecker : IDisposable
    {
        private readonly Timer checkerTimer;
        private readonly Action onTimeout;
        private readonly ProgressCounter counter;
        private double lastProgress = -1.0;

        public ProgressChecker(ProgressCounter counter, Action onTimeout, int interval)
        {
            this.counter = counter;
            this.onTimeout = onTimeout;

            checkerTimer = new Timer(CheckerCallback, this, interval, interval);
        }

        /// <summary>
        /// Only returns false if no progress has been made between 2 checks but it should have made.
        /// </summary>
        /// <returns></returns>
        public bool Check()
        {
            if (!counter.Indeterminate && lastProgress != 1.0)
            {
                double newProgress = counter.GetPercent();

                if (newProgress != 1.0 && lastProgress == newProgress)
                {
                    return false;
                }

                lastProgress = newProgress;
            }

            return true;
        }

        private static void CheckerCallback(object? state)
        {
            if (state == null)
            {
                throw new NullReferenceException("CheckerCallback's state was null");
            }

            var checker = (ProgressChecker)state;
            if (!checker.Check()) checker.onTimeout();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    checkerTimer.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
