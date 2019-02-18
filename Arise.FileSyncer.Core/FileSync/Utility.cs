using System;
using System.IO;

namespace Arise.FileSyncer.Core.FileSync
{
    public static class Utility
    {
        public delegate bool DelegateFileCreate(Guid profileId, string rootPath, string relativePath);
        public delegate bool DelegateFileDelete(Guid profileId, string rootPath, string relativePath);
        public delegate bool DelegateFileRename(Guid profileId, string rootPath, string relativePath, string targetName);
        public delegate bool DelegateFileSetTime(Guid profileId, string rootPath, string relativePath, DateTime lastWriteTime, DateTime creationTime);
        public delegate bool DelegateFileCreateWriteStream(Guid profileId, string rootPath, string relativePath, out Stream fileStream, FileMode fileMode = FileMode.Append);
        public delegate bool DelegateFileCreateReadStream(Guid profileId, string rootPath, string relativePath, out Stream fileStream);
        public delegate bool DelegateDirectoryCreate(Guid profileId, string rootPath, string relativePath);

        public static DelegateFileCreate FileCreate = (_, r, p) => DefaultFileCreate(Path.Combine(r, p));
        public static DelegateFileDelete FileDelete = (_, r, p) => DefaultFileDelete(Path.Combine(r, p));
        public static DelegateFileRename FileRename = (_, r, p, t) => DefaultFileRename(Path.Combine(r, p), t);
        public static DelegateFileSetTime FileSetTime = (_, r, p, lt, ct) => DefaultFileSetTime(Path.Combine(r, p), lt, ct);
        public static DelegateFileCreateWriteStream FileCreateWriteStream = DefaultFileCreateWriteStream;
        public static DelegateFileCreateReadStream FileCreateReadStream = DefaultFileCreateReadStream;
        public static DelegateDirectoryCreate DirectoryCreate = (_, r, p) => DefaultDirectoryCreate(Path.Combine(r, p));

        private const string LogName = "Utility";

        #region Default Methods

        private static bool DefaultFileCreate(string path)
        {
            try
            {
                FileStream stream = File.Create(path);
                stream.Flush(true);
                stream.Dispose();
            }
            catch (Exception ex)
            {
                Log.Warning(LogName + ": exception when creating file. MSG: " + ex.Message);
                return false;
            }

            return true;
        }

        private static bool DefaultFileDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (Exception ex)
            {
                Log.Warning(LogName + ": exception when deleting file. MSG: " + ex.Message);
                return false;
            }

            return true;
        }

        private static bool DefaultFileRename(string path, string targetName)
        {
            try
            {
                File.Move(path, Path.Combine(Path.GetDirectoryName(path), targetName));
            }
            catch
            {
                Log.Warning($"{LogName}: Unable to rename \"{targetName}\"!");
                return false;
            }

            return true;
        }

        private static bool DefaultFileSetTime(string path, DateTime lastWriteTime, DateTime creationTime)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(path)
                {
                    CreationTime = creationTime,
                    LastWriteTime = lastWriteTime
                };
            }
            catch
            {
                Log.Warning($"{LogName}: Unable to update \"{path}\" info!");
                return false;
            }

            return true;
        }

        private static bool DefaultFileCreateWriteStream(Guid _, string rootPath, string relativePath, out Stream fileStream, FileMode fileMode)
        {
            string path = Path.Combine(rootPath, relativePath);

            try
            {
                fileStream = new FileStream(path, fileMode);
            }
            catch (Exception ex)
            {
                Log.Warning(LogName + ": exception when opening file for write. MSG: " + ex.Message);
                fileStream = null;
                return false;
            }

            return true;
        }

        private static bool DefaultFileCreateReadStream(Guid _, string rootPath, string relativePath, out Stream fileStream)
        {
            string path = Path.Combine(rootPath, relativePath);

            try
            {
                fileStream = File.OpenRead(path);
            }
            catch (Exception ex)
            {
                Log.Warning(LogName + ": exception when opening file for read. MSG: " + ex.Message);
                fileStream = null;
                return false;
            }

            return true;
        }

        private static bool DefaultDirectoryCreate(string path)
        {
            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Log.Warning(LogName + ": exception when creating directory. MSG: " + ex.Message);
                return false;
            }

            return true;
        }

        #endregion
    }
}
