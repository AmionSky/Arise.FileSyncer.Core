using System;
using System.Collections.Generic;
using Arise.FileSyncer.Core.Helpers;

namespace Arise.FileSyncer.Core.FileSync
{
    public class DirectoryTreeDifference
    {
        public List<string> RemoteMissingFiles { get; private set; }
        public List<string> RemoteMissingDirectories { get; private set; }
        public List<string> LocalMissingFiles { get; private set; }
        public List<string> LocalMissingDirectories { get; private set; }

        public DirectoryTreeDifference(FileSystemItem[] localTree, FileSystemItem[] remoteTree, bool supportTimestamp, bool preferLocal = true)
        {
            List<string> exclude = new();

            RemoteMissingFiles = new List<string>();
            RemoteMissingDirectories = new List<string>();
            LocalMissingFiles = new List<string>();
            LocalMissingDirectories = new List<string>();

            foreach (FileSystemItem localItem in localTree)
            {
                string relativeName = PathHelper.GetCorrect(localItem.RelativePath, localItem.IsDirectory);

                if (IsContainBeginning(exclude, relativeName))
                {
                    if (localItem.IsDirectory) RemoteMissingDirectories.Add(relativeName);
                    else RemoteMissingFiles.Add(relativeName);
                    continue;
                }

                bool found = false;
                foreach (FileSystemItem remoteItem in remoteTree)
                {
                    if (relativeName.Equals(PathHelper.GetCorrect(remoteItem.RelativePath, remoteItem.IsDirectory), StringComparison.Ordinal))
                    {
                        if (!localItem.IsDirectory)
                        {
                            if (supportTimestamp)
                            {
                                double timeDiff = Math.Abs((localItem.LastWriteTime - remoteItem.LastWriteTime).TotalSeconds);

                                if (timeDiff > 2.0)
                                {
                                    RemoteMissingFiles.Add(relativeName);
                                }
                            }
                            else
                            {
                                if (localItem.FileSize != remoteItem.FileSize)
                                {
                                    if (preferLocal)
                                    {
                                        RemoteMissingFiles.Add(relativeName);
                                    }
                                }
                            }
                        }

                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    if (localItem.IsDirectory)
                    {
                        RemoteMissingDirectories.Add(relativeName);
                        exclude.Add(relativeName);
                    }
                    else
                    {
                        RemoteMissingFiles.Add(relativeName);
                    }
                }
            }

            // Clear exclude to be able to use the same list for remoteTree check
            exclude.Clear();

            // Find all remote items that was not handled in the localTree test. And add them to local missing lists.
            foreach (var remoteItem in remoteTree)
            {
                string relativeName = PathHelper.GetCorrect(remoteItem.RelativePath, remoteItem.IsDirectory);

                if (IsContainBeginning(exclude, relativeName))
                {
                    if (remoteItem.IsDirectory) LocalMissingDirectories.Add(relativeName);
                    else LocalMissingFiles.Add(relativeName);
                    continue;
                }

                bool found = false;
                foreach (var localItem in localTree)
                {
                    if (relativeName.Equals(PathHelper.GetCorrect(localItem.RelativePath, localItem.IsDirectory), StringComparison.Ordinal))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    if (remoteItem.IsDirectory)
                    {
                        LocalMissingDirectories.Add(relativeName);
                        exclude.Add(relativeName);
                    }
                    else
                    {
                        LocalMissingFiles.Add(relativeName);
                    }
                }
            }

        }

        private static bool IsContainBeginning(List<string> list, string item)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (item.StartsWith(list[i], StringComparison.Ordinal)) return true;
            }
            return false;
        }
    }
}
