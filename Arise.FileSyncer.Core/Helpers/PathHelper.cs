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

            StringBuilder pathBuilder = new StringBuilder(path);
            pathBuilder.Replace(OtherSeparatorChar, Path.DirectorySeparatorChar);

            if (isDirectory)
            {
                char lastChar = path[path.Length - 1];
                if (lastChar != Path.DirectorySeparatorChar && lastChar != OtherSeparatorChar)
                {
                    pathBuilder.Append(Path.DirectorySeparatorChar);
                }
            }

            return pathBuilder.ToString();
        }

        /// <summary>
        /// Gets the relative path. Automatically converts it. Returns empty string on error
        /// </summary>
        /// <param name="fullPath">The absolute path you want to get the relative of</param>
        /// <param name="rootPath">The parent folder you want to use as the relative root</param>
        /// <param name="fullPathIsDirectory">Is the fullPath a directory</param>
        /// <returns>Relative path</returns>
        public static string GetRelative(string fullPath, string rootPath, bool fullPathIsDirectory)
        {
            fullPath = GetCorrect(fullPath, fullPathIsDirectory);
            rootPath = GetCorrect(rootPath, true);

            if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(rootPath) ||
                !fullPath.StartsWith(rootPath, StringComparison.Ordinal))
            {
                return string.Empty;
            }

            return fullPath.Substring(rootPath.Length);
        }
    }
}
