using System;
using System.Collections.Generic;
using System.IO;

namespace Arise.FileSyncer.Core.FileSync
{
    public class FileSendInfo
    {
        public Guid ProfileId { get; private set; }
        public Guid ProfileKey { get; private set; }
        public FileInfo File { get; private set; }

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
        public FileSendInfo(Guid profileId, Guid profileKey, FileInfo fileInfo, string rootPath, string relativePath, Action<FileSendInfo> finishedCallback)
        {
            ProfileId = profileId;
            ProfileKey = profileKey;
            File = fileInfo;
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
        public static FileSendInfo Create(Guid profileId, SyncProfile profile, string relativePath,
            string absolutePath = null, Action<FileSendInfo> finishedCallback = null)
        {
            string localPath = string.IsNullOrEmpty(absolutePath) ? profile.RootDirectory + relativePath : absolutePath;

            FileInfo fileInfo;
            try { fileInfo = new FileInfo(localPath); }
            catch
            {
                Log.Warning($"FileSendInfo: Failed to get info for file: {localPath}");
                return null;
            }

            if (fileInfo.Exists)
            {
                return new FileSendInfo(profileId, profile.Key, fileInfo, profile.RootDirectory, relativePath, finishedCallback);
            }
            else
            {
                Log.Warning($"FileSendInfo: File does not exist: {localPath}");
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
        public static List<FileSendInfo> Create(Guid profileId, SyncProfile profile, IList<string> relativePaths,
            IList<string> absolutePaths = null, Action<FileSendInfo> finishedCallback = null)
        {
            bool useAbsolutePaths = absolutePaths != null;
            List<FileSendInfo> sendInfos = new List<FileSendInfo>();

            for (int i = 0; i < relativePaths.Count; i++)
            {
                FileSendInfo sendInfo = Create(profileId, profile, relativePaths[i], (useAbsolutePaths) ? absolutePaths[i] : null, finishedCallback);
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
