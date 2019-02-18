using System;
using System.IO;
using Arise.FileSyncer.FileSync;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer
{
    internal class SyncProfileState : IBinarySerializable
    {
        public Guid Id;
        public Guid Key;
        public bool AllowDelete;
        public FileSystemItem[] State;

        public bool IsResponse = false;

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

            IsResponse = stream.ReadBoolean();
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

            stream.Write(IsResponse);
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
