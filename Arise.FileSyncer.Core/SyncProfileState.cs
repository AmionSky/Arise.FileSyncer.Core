using System;
using System.IO;
using Arise.FileSyncer.Core.FileSync;
using Arise.FileSyncer.Core.Serializer;

namespace Arise.FileSyncer.Core
{
    internal class SyncProfileState : IBinarySerializable
    {
        public Guid Id;
        public Guid Key;
        public bool AllowDelete;
        public FileSystemItem[] State;

        public SyncProfileState() { }

        public SyncProfileState(Guid id, Guid key, bool allowDelete, FileSystemItem[] state)
        {
            Id = id;
            Key = key;
            AllowDelete = allowDelete;
            State = state;
        }

        public void Deserialize(Stream stream)
        {
            Id = stream.ReadGuid();
            Key = stream.ReadGuid();
            AllowDelete = stream.ReadBoolean();

            if (stream.ReadBoolean())
            {
                State = stream.ReadArray<FileSystemItem>();
            }
            else
            {
                State = null;
            }
        }

        public void Serialize(Stream stream)
        {
            stream.Write(Id);
            stream.Write(Key);
            stream.Write(AllowDelete);

            if (State != null)
            {
                stream.Write(true);
                stream.Write(State);
            }
            else
            {
                stream.Write(false);
            }
        }

        public static SyncProfileState Create(Guid id, SyncProfile profile)
        {
            if (profile.GenerateState(out var state))
            {
                return new SyncProfileState(id, profile.Key, profile.AllowDelete, state);
            }

            return null;
        }
    }
}
