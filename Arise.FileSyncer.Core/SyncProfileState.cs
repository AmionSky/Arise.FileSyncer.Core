using System;
using System.IO;
using Arise.FileSyncer.Core.FileSync;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core
{
    internal sealed class SyncProfileState : IBinarySerializable
    {
        public Guid Id;
        public Guid Key;
        public bool AllowDelete;
        public FileSystemItem[]? State;

        public SyncProfileState() { }

        public SyncProfileState(Guid id, Guid key, bool allowDelete, FileSystemItem[]? state)
        {
            Id = id;
            Key = key;
            AllowDelete = allowDelete;
            State = state;
        }

        public static SyncProfileState? Create(Guid id, SyncProfile profile)
        {
            FileSystemItem[]? state = profile.GenerateState();
            if (state != null)
            {
                return new SyncProfileState(id, profile.Key, profile.AllowDelete, state);
            }

            return null;
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
            stream.WriteAFS(Id);
            stream.WriteAFS(Key);
            stream.WriteAFS(AllowDelete);

            if (State != null)
            {
                stream.WriteAFS(true);
                stream.WriteAFS(State);
            }
            else
            {
                stream.WriteAFS(false);
            }
        }
    }
}
