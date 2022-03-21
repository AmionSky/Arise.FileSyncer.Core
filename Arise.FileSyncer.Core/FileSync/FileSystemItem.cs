using System;
using System.Diagnostics;
using System.IO;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core.FileSync
{
    public struct FileSystemItem : IBinarySerializable
    {
        public bool IsDirectory { get => isDirectory; }
        public string RelativePath { get => relativePath; }
        public long FileSize { get => fileSize; }
        public DateTime LastWriteTime { get => lastWriteTime; }

        private bool isDirectory;
        private string relativePath;
        private long fileSize;
        private DateTime lastWriteTime;

        public FileSystemItem(bool isDirectory, string relativePath, long fileSize, DateTime lastWriteTime)
        {
            Debug.Assert(lastWriteTime.Kind == DateTimeKind.Utc);

            this.isDirectory = isDirectory;
            this.relativePath = relativePath;
            this.fileSize = fileSize;
            this.lastWriteTime = lastWriteTime;
        }

        public void Deserialize(Stream stream)
        {
            isDirectory = stream.ReadBoolean();
            relativePath = stream.ReadString();

            if (!IsDirectory)
            {
                fileSize = stream.ReadInt64();
                lastWriteTime = stream.ReadDateTime();
            }
            else
            {
                fileSize = 0;
                lastWriteTime = new DateTime(0, DateTimeKind.Utc);
            }
        }

        public void Serialize(Stream stream)
        {
            stream.WriteAFS(IsDirectory);
            stream.WriteAFS(RelativePath);

            if (!IsDirectory)
            {
                stream.WriteAFS(FileSize);
                stream.WriteAFS(LastWriteTime);
            }
        }
    }
}
