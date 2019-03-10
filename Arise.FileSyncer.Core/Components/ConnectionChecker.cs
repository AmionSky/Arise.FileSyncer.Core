using System;
using System.Timers;
using Arise.FileSyncer.Core.Messages;

namespace Arise.FileSyncer.Core.Components
{
    class ConnectionChecker : IDisposable
    {
        private readonly Action<NetMessage> send;
        private readonly Timer checkerTimer;

        public ConnectionChecker(Action<NetMessage> send)
        {
            this.send = send;

            checkerTimer = new Timer();
            checkerTimer.Elapsed += CheckerTimer_Elapsed;
            checkerTimer.Interval = 60000; // 1 minute
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
