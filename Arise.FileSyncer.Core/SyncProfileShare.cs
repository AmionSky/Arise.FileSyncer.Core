using System;
using System.IO;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core
{
    public struct SyncProfileShare : IBinarySerializable
    {
        public Guid Id;
        public Guid Key;
        public string Name;
        public DateTime CreationDate;
        public bool SkipHidden;

        public void Deserialize(Stream stream)
        {
            Id = stream.ReadGuid();
            Key = stream.ReadGuid();
            Name = stream.ReadString();
            CreationDate = stream.ReadDateTime();
            SkipHidden = stream.ReadBoolean();
        }

        public void Serialize(Stream stream)
        {
            stream.WriteAFS(Id);
            stream.WriteAFS(Key);
            stream.WriteAFS(Name);
            stream.WriteAFS(CreationDate);
            stream.WriteAFS(SkipHidden);
        }
    }
}
