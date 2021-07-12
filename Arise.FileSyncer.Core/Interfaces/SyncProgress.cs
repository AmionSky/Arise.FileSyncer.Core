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
        public static double GetPercent(this ISyncProgress progress)
        {
            long current = progress.Current;
            long maximum = progress.Maximum;

            if (current <= 0 || maximum < 0 || current > maximum) return 0.0;
            if (maximum == 0) return 1.0;

            return current / (double)maximum;
        }

        public static long GetRemaining(this ISyncProgress progress)
        {
            long remaining = progress.Maximum - progress.Current;
            return remaining < 0 ? 0 : remaining;
        }
    }
}
