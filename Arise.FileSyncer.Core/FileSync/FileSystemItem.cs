using System;
using System.IO;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.FileSync
{
    public struct FileSystemItem : IBinarySerializable
    {
        public bool IsDirectory;
        public string RelativePath;
        public long FileSize;
        public DateTime LastWriteTime;

        public FileSystemItem(bool isDirectory, string relativePath, long fileSize, DateTime lastWriteTime)
        {
            IsDirectory = isDirectory;
            RelativePath = relativePath;
            FileSize = fileSize;
            LastWriteTime = lastWriteTime;
        }

        public void Deserialize(Stream stream)
        {
            IsDirectory = stream.ReadBoolean();
            RelativePath = stream.ReadString();

            if (!IsDirectory)
            {
                FileSize = stream.ReadInt64();
                LastWriteTime = stream.ReadDateTime();
            }
            else
            {
                FileSize = 0;
                LastWriteTime = new DateTime();
            }
        }

        public void Serialize(Stream stream)
        {
            stream.Write(IsDirectory);
            stream.Write(RelativePath);

            if (!IsDirectory)
            {
                stream.Write(FileSize);
                stream.Write(LastWriteTime);
            }
        }
    }
}
