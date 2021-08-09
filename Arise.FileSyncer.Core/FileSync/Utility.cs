using System;
using System.Collections.Generic;
using System.IO;
using Arise.FileSyncer.Core.Helpers;

namespace Arise.FileSyncer.Core.FileSync
{
    public static class Utility
    {
        public delegate bool DelegateFileCreate(string rootPath, string relativePath);
        public delegate bool DelegateFileDelete(string rootPath, string relativePath);
        public delegate bool DelegateFileRename(string rootPath, string relativePath, string targetName);
        public delegate bool DelegateFileSetTime(string rootPath, string relativePath, DateTime lastWriteTime, DateTime creationTime);
        public delegate bool DelegateDirectoryCreate(string rootPath, string relativePath);
        public delegate bool DelegateDirectoryDelete(string rootPath, string relativePath);
        public delegate Stream DelegateFileCreateWriteStream(string rootPath, string relativePath, FileMode fileMode);
        public delegate Stream DelegateFileCreateReadStream(string rootPath, string relativePath);
        public delegate FileSystemItem[] DelegateGenerateTree(string rootPath, bool skipHidden);

#pragma warning disable CA2211 // Non-constant fields should not be visible
        /// <summary>
        /// Creates or overwrites file
        /// </summary>
        /// <returns>True on success</returns>
        public static DelegateFileCreate FileCreate = (r, p) => DefaultFileCreate(Path.Combine(r, p));
        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <returns>True on success or if the file does not exist</returns>
        public static DelegateFileDelete FileDelete = (r, p) => DefaultFileDelete(Path.Combine(r, p));
        /// <summary>
        /// Renames a file
        /// </summary>
        public static DelegateFileRename FileRename = (r, p, t) => DefaultFileRename(Path.Combine(r, p), t);
        /// <summary>
        /// Sets a file creation and last write time
        /// </summary>
        public static DelegateFileSetTime FileSetTime = (r, p, wt, ct) => DefaultFileSetTime(Path.Combine(r, p), wt, ct);
        /// <summary>
        /// Creates a directory
        /// </summary>
        /// <returns>True on success or if the directory already exists</returns>
        public static DelegateDirectoryCreate DirectoryCreate = (r, p) => DefaultDirectoryCreate(Path.Combine(r, p));
        /// <summary>
        /// Deletes a directory
        /// </summary>
        /// <returns>True on success or if the directory does not exist</returns>
        public static DelegateDirectoryDelete DirectoryDelete = (r, p) => DefaultDirectoryDelete(Path.Combine(r, p));
        /// <summary>
        /// Creates a stream for writing to a file
        /// </summary>
        /// <returns>Stream to the file on success, null on failure</returns>
        public static DelegateFileCreateWriteStream FileCreateWriteStream = (r, p, fm) => DefaultFileCreateWriteStream(Path.Combine(r, p), fm);
        /// <summary>
        /// Creates a stream for reading a file
        /// </summary>
        /// <returns>Stream of the file on success, null on failure</returns>
        public static DelegateFileCreateReadStream FileCreateReadStream = (r, p) => DefaultFileCreateReadStream(Path.Combine(r, p));
        /// <summary>
        /// Generates a directory tree including all the folders and files from the root directory and sub directories
        /// </summary>
        public static DelegateGenerateTree GenerateTree = DefaultGenerateTree;
#pragma warning restore CA2211 // Non-constant fields should not be visible

        private const string LogName = nameof(Utility);

        #region Default Methods

        private static bool DefaultFileCreate(string path)
        {
            try
            {
                FileStream stream = File.Create(path);
                stream.Flush(true);
                stream.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"{LogName}: exception while creating file. {ex.Message}");
                return false;
            }
        }

        private static bool DefaultFileDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"{LogName}: exception while deleting file. {ex.Message}");
                return false;
            }
        }

        private static bool DefaultFileRename(string path, string targetName)
        {
            try
            {
                File.Move(path, Path.Combine(Path.GetDirectoryName(path), targetName));
                return true;
            }
            catch
            {
                Log.Warning($"{LogName}: Unable to rename \"{targetName}\"!");
                return false;
            }
        }

        private static bool DefaultFileSetTime(string path, DateTime lastWriteTime, DateTime creationTime)
        {
            try
            {
                FileInfo fileInfo = new(path)
                {
                    CreationTime = creationTime,
                    LastWriteTime = lastWriteTime
                };
                return true;
            }
            catch
            {
                Log.Warning($"{LogName}: Unable to update \"{path}\" info!");
                return false;
            }
        }

        private static bool DefaultDirectoryCreate(string path)
        {
            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"{LogName}: exception while creating directory. {ex.Message}");
                return false;
            }
        }

        private static bool DefaultDirectoryDelete(string path)
        {
            try
            {
                if (Directory.Exists(path)) Directory.Delete(path, true);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"{LogName}: exception while deleting directory. {ex.Message}");
                return false;
            }
        }

        private static Stream DefaultFileCreateWriteStream(string path, FileMode fileMode)
        {
            try
            {
                return new FileStream(path, fileMode);
            }
            catch (Exception ex)
            {
                Log.Warning($"{LogName}: exception while opening file for write. {ex.Message}");
                return null;
            }
        }

        private static Stream DefaultFileCreateReadStream(string path)
        {
            try
            {
                return File.OpenRead(path);
            }
            catch (Exception ex)
            {
                Log.Warning($"{LogName}: exception while opening file for read. {ex.Message}");
                return null;
            }
        }

        private static FileSystemItem[] DefaultGenerateTree(string rootPath, bool skipHidden)
        {
            if (!Directory.Exists(rootPath))
            {
                Log.Warning($"{LogName}: {nameof(DefaultGenerateTree)}: root directory ({rootPath}) does not exist.");
                return null;
            }

            List<FileSystemItem> fsItemList = new();
            DirectoryInfo rootDirInfo;

            try
            {
                rootDirInfo = new DirectoryInfo(PathHelper.GetCorrect(rootPath, true));
            }
            catch (Exception ex)
            {
                Log.Warning($"{LogName}: {nameof(DefaultGenerateTree)}: Unable to access root directory ({rootPath}): {ex}");
                return null;
            }

            var options = new EnumerationOptions()
            {
                AttributesToSkip = FileAttributes.System,
                RecurseSubdirectories = true,
                MatchType = MatchType.Simple,
            };
            if (skipHidden) options.AttributesToSkip |= FileAttributes.Hidden;

            try
            {
                foreach (var fsItem in rootDirInfo.EnumerateFileSystemInfos("*", options))
                {
                    if (string.Equals(fsItem.Extension, FileBuilder.TemporaryFileExtension, StringComparison.Ordinal))
                        continue;

                    string relativePath = Path.GetRelativePath(rootPath, fsItem.FullName);
                    bool isDirectory = fsItem.Attributes.HasFlag(FileAttributes.Directory);
                    long size = isDirectory ? 0 : new FileInfo(fsItem.FullName).Length;
                    fsItemList.Add(new FileSystemItem(isDirectory, relativePath, size, fsItem.LastWriteTime));
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"{LogName}: {nameof(DefaultGenerateTree)}: Tree enumeration failed: {ex}");
                return null;
            }

            return fsItemList.ToArray();
        }

        #endregion
    }
}
