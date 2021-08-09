using System;
using System.Collections.Generic;
using System.IO;
using Arise.FileSyncer.Core.Helpers;

namespace Arise.FileSyncer.Core.FileSync
{
    public static class DirectoryTreeQuery
    {
        public delegate FileSystemItem[] DelegateGenerateTree(string rootDirectory, bool skipHidden);
        public static DelegateGenerateTree GenerateTree = (rd, sh) =>
        {
            GenerateTreeDefault(out var tree, rd, sh);
            return tree;
        };

        /// <summary>
        /// Creates a DirectoryTree with all folders and files inside of it. With an optional cleanup utility (remove var).
        /// </summary>
        /// <param name="treeInfo">DirectoryTreeInfo array.</param>
        /// <param name="folder">Root folder for the directory tree creation.</param>
        /// <param name="remove">The extension of files marked for delete.</param>
        /// <returns>Success</returns>
        public static bool GenerateTreeDefault(out FileSystemItem[] treeInfo, string folder, bool skipHidden, string remove = null)
        {
            if (!Directory.Exists(folder))
            {
                treeInfo = null;
                return false;
            }

            bool doRemove = !string.IsNullOrEmpty(remove);
            List<FileSystemItem> treeInfoList = new();
            List<string> hiddenList = new();

            DirectoryInfo rootDirInfo;
            try { rootDirInfo = new DirectoryInfo(PathHelper.GetCorrect(folder, true)); }
            catch
            {
                Log.Warning("DirectoryTreeQuery: Unable to access directory: " + folder);
                treeInfo = null;
                return false;
            }

            foreach (DirectoryInfo d in rootDirInfo.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                // Skip Hidden
                if (skipHidden)
                {
                    if (d.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        hiddenList.Add(d.FullName + Path.DirectorySeparatorChar);
                        continue;
                    }
                    else
                    {
                        bool found = false;

                        for (int i = 0; i < hiddenList.Count; i++)
                        {
                            if (d.FullName.StartsWith(hiddenList[i], StringComparison.Ordinal))
                            {
                                found = true;
                                break;
                            }
                        }

                        if (found) continue;
                    }
                }

                // Add directory to the tree info list
                string relativePath = PathHelper.GetRelative(d.FullName, folder, true);
                treeInfoList.Add(new FileSystemItem(true, relativePath, 0, d.LastWriteTime));
            }

            foreach (FileInfo f in rootDirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                if (doRemove && string.Equals(Path.GetExtension(f.FullName), remove, StringComparison.Ordinal))
                {
                    try { File.Delete(f.FullName); }
                    catch { Log.Warning("DirectoryTreeQuery: Failed to delete file"); }
                }
                else
                {
                    // Skip Hidden
                    if (skipHidden)
                    {
                        if (f.Attributes.HasFlag(FileAttributes.Hidden))
                        {
                            continue;
                        }
                        else
                        {
                            bool found = false;

                            for (int i = 0; i < hiddenList.Count; i++)
                            {
                                if (f.FullName.StartsWith(hiddenList[i], StringComparison.Ordinal))
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (found) continue;
                        }
                    }

                    // Add file to the tree info list
                    string relativePath = PathHelper.GetRelative(f.FullName, folder, false);
                    treeInfoList.Add(new FileSystemItem(false, relativePath, f.Length, f.LastWriteTime));
                }
            }

            treeInfo = treeInfoList.ToArray();
            return true;
        }
    }
}
