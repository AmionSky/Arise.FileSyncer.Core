using System;
using System.Timers;
using Arise.FileSyncer.Core.Messages;

namespace Arise.FileSyncer.Core.Components
{
    /// <summary>
    /// Periodically sends a message to check if the connection is still alive.
    /// </summary>
    class ConnectionChecker : IDisposable
    {
        private readonly Action<NetMessage> send;
        private readonly Timer checkerTimer;

        public ConnectionChecker(Action<NetMessage> send, int interval)
        {
            this.send = send;

            checkerTimer = new Timer();
            checkerTimer.Elapsed += CheckerTimer_Elapsed;
            checkerTimer.Interval = interval;
            checkerTimer.AutoReset = true;
            checkerTimer.Start();
        }

        private void CheckerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            send(new IsAliveMessage());
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
