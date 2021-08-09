using System;
using System.IO;
using System.Text;

namespace Arise.FileSyncer.Core.Helpers
{
    public static class PathHelper
    {
        public static readonly char OtherSeparatorChar = (Path.DirectorySeparatorChar == '/') ? '\\' : '/';

        /// <summary>
        /// Converts and corrects the path to be consistent
        /// </summary>
        /// <param name="path">The path to convert</param>
        /// <param name="isDirectory">Place a path separator at the end of the path</param>
        /// <returns>Corrected path</returns>
        public static string GetCorrect(string path, bool isDirectory)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            StringBuilder pathBuilder = new(path);
            pathBuilder.Replace(OtherSeparatorChar, Path.DirectorySeparatorChar);

            if (isDirectory)
            {
                char lastChar = path[^1];
                if (lastChar != Path.DirectorySeparatorChar && lastChar != OtherSeparatorChar)
                {
                    pathBuilder.Append(Path.DirectorySeparatorChar);
                }
            }

            return pathBuilder.ToString();
        }
    }
}
