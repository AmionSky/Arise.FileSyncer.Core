using System;
using System.Timers;

namespace Arise.FileSyncer.Core.Components
{
    /// <summary>
    /// Periodically checks the progress of the connection.
    /// Issues a timeout if no progress was made between 2 checks.
    /// </summary>
    internal class ProgressChecker : IDisposable
    {
        private readonly Timer checkerTimer;
        private readonly Action onTimeout;
        private readonly ProgressCounter counter;
        private double lastProgress = -1.0;

        public ProgressChecker(ProgressCounter counter, Action onTimeout, int interval)
        {
            this.counter = counter;
            this.onTimeout = onTimeout;

            checkerTimer = new Timer();
            checkerTimer.Elapsed += CheckerTimer_Elapsed;
            checkerTimer.Interval = interval;
            checkerTimer.AutoReset = true;
            checkerTimer.Start();
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

        private void CheckerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!Check()) onTimeout();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
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
        }
        #endregion
    }
}
