using System;
using System.Collections.Generic;
using Arise.FileSyncer.Core.Helpers;

namespace Arise.FileSyncer.Core.FileSync
{
    public class DirectoryTreeDifference
    {
        public List<string> RemoteMissingFiles { get; private set; }
        public List<string> RemoteMissingDirectories { get; private set; }

        public DirectoryTreeDifference(FileSystemItem[] localTree, FileSystemItem[] remoteTree, bool supportTimestamp, bool preferLocal = true)
        {
            List<string> exclude = new List<string>();

            RemoteMissingFiles = new List<string>();
            RemoteMissingDirectories = new List<string>();

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
                                double timeDiff = (localItem.LastWriteTime - remoteItem.LastWriteTime).TotalSeconds;

                                if (timeDiff > 1.0)
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
