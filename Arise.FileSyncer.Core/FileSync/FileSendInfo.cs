using System;
using System.Collections.Generic;
using System.IO;

namespace Arise.FileSyncer.Core.FileSync
{
    public sealed class FileSendInfo
    {
        public Guid ProfileId { get; private set; }
        public Guid ProfileKey { get; private set; }

        public long Size { get; private set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Root identifier. Ususally the profile's root directory.
        /// </summary>
        public string RootPath { get; private set; }

        /// <summary>
        /// Relative path from the sync profile's root directory.
        /// </summary>
        public string RelativePath { get; private set; }

        /// <summary>
        /// Gets called when the file had been sent or encountured an error and was dropped from the send list.
        /// On events not related to file sending (ex: Disconnect), it does not get called.
        /// </summary>
        public Action<FileSendInfo> FinishedCallback { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="profileKey"></param>
        /// <param name="fileInfo"></param>
        /// <param name="relativePath">Relative path from the sync profile's root directory.</param>
        /// <param name="finishedCallback">Gets called when the file had been sent or encountured an error and was dropped from the send list.
        /// On events not related to file sending (ex: Disconnect), it does not get called.</param>
        public FileSendInfo(Guid profileId, Guid profileKey, long size, DateTime lastWriteTime, DateTime creationTime,
                            string rootPath, string relativePath, Action<FileSendInfo> finishedCallback)
        {
            ProfileId = profileId;
            ProfileKey = profileKey;

            Size = size;
            LastWriteTime = lastWriteTime;
            CreationTime = creationTime;

            RootPath = rootPath;
            RelativePath = relativePath;
            FinishedCallback = finishedCallback;
        }

        /// <summary>
        /// Creates a FileSendInfo. Returns null if encountered an error.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="relativePath">Relative path from the sync profile's root directory.</param>
        /// <param name="absolutePath">Optional: Overrides the absolute path of the file to send.</param>
        /// <param name="finishedCallback">Optional: Gets called when the file had been sent or encountured an error and was dropped from the send list.
        /// On events not related to file sending (ex: Disconnect), it does not get called.</param>
        /// <returns>A FileSendInfo or null if encountered an error.</returns>
        public static FileSendInfo Create(Guid profileId, SyncProfile profile, string relativePath, Action<FileSendInfo> finishedCallback = null)
        {
            var fileInfo = Utility.FileInfo(profile.RootDirectory, relativePath);

            if (fileInfo.HasValue)
            {
                (long size, DateTime lwt, DateTime ct) = fileInfo.Value;
                return new FileSendInfo(profileId, profile.Key, size, lwt, ct, profile.RootDirectory, relativePath, finishedCallback);
            }
            else
            {
                string fullPath = Path.Combine(profile.RootDirectory, relativePath);
                Log.Warning($"FileSendInfo: failed to get file info for: {fullPath}");
                return null;
            }
        }

        /// <summary>
        /// Creates a list of FileSendInfo.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="relativePaths">IList of relative path from the sync profile's root directory.</param>
        /// <param name="absolutePaths">Optional: Overrides the absolute path of the file to send. Have to be the same length as the relative path IList.</param>
        /// <param name="finishedCallback">Optional: Gets called when the file had been sent or encountured an error and was dropped from the send list.
        /// On events not related to file sending (ex: Disconnect), it does not get called.</param>
        /// <returns>A list of FileSendInfo.</returns>
        public static List<FileSendInfo> Create(Guid profileId, SyncProfile profile, IList<string> relativePaths, Action<FileSendInfo> finishedCallback = null)
        {
            List<FileSendInfo> sendInfos = new();

            for (int i = 0; i < relativePaths.Count; i++)
            {
                FileSendInfo sendInfo = Create(profileId, profile, relativePaths[i], finishedCallback);
                if (sendInfo != null) sendInfos.Add(sendInfo);
            }

            return sendInfos;
        }

        public void Finished()
        {
            FinishedCallback?.Invoke(this);
        }
    }
}
