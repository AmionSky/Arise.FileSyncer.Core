using System;

namespace Arise.FileSyncer.Core.Helpers
{
    public static class Random
    {
        private static readonly System.Random random = new System.Random();

        public static long GetInt64()
        {
            byte[] buffer = new byte[sizeof(long)];
            random.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }
    }
}
