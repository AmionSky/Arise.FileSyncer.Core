using System;
using System.Collections.Generic;
using System.Text;

namespace Arise.FileSyncer.Core
{
    public interface ISyncProgress
    {
        bool Indeterminate { get; }
        long Current { get; }
        long Maximum { get; }
    }

    public static class SyncProgress
    {
        public static double GetPercent(this ISyncProgress syncProgress)
        {
            long current = syncProgress.Current;
            long maximum = syncProgress.Maximum;

            if (current < 0 || maximum < 0 || current > maximum)
            {
                Log.Error("ProgressCounter GetPercent() error!");
                return 0.0;
            }

            if (maximum == 0) return 1.0;
            if (current == 0) return 0.0;

            return current / (double)maximum;
        }
    }
}
