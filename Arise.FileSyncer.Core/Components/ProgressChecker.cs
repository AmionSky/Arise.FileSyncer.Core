using System;
using System.Timers;

namespace Arise.FileSyncer.Core.Components
{
    internal class ProgressChecker : IDisposable
    {
        private readonly Timer checkerTimer;
        private readonly Action failed;
        private readonly ProgressCounter counter;
        private double lastProgress = -1.0;

        public ProgressChecker(ProgressCounter counter, Action failed)
        {
            this.counter = counter;
            this.failed = failed;

            checkerTimer = new Timer();
            checkerTimer.Elapsed += CheckerTimer_Elapsed;
            checkerTimer.Interval = 180000; // 3 minute
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
            if (!Check()) failed();
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
