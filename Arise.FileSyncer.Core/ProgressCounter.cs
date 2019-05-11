using System.Threading;

namespace Arise.FileSyncer.Core
{
    public class ProgressCounter : ISyncProgress
    {
        public bool Indeterminate { get => indeterminate; internal set => indeterminate = value; }
        public long Current => Interlocked.Read(ref currentValue);
        public long Maximum => Interlocked.Read(ref maximumValue);

        private volatile bool indeterminate = true;
        private long currentValue = 0;
        private long maximumValue = 0;

        internal void AddProgress(long amount)
        {
            Interlocked.Add(ref currentValue, amount);
        }

        internal void AddMaximum(long amount)
        {
            Interlocked.Add(ref maximumValue, amount);
        }

        internal void RemoveProgress(long amount)
        {
            AddProgress(-amount);
        }

        internal void RemoveMaximum(long amount)
        {
            AddMaximum(-amount);
        }

        internal void Reset()
        {
            Indeterminate = true;
            Interlocked.Exchange(ref currentValue, 0);
            Interlocked.Exchange(ref maximumValue, 0);
        }
    }
}
