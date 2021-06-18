using System;
using System.IO;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core.Messages
{
    internal class FileStartMessage : FileMessageBase
    {
        public Guid ProfileId { get; set; }
        public Guid ProfileKey { get; set; }
        public string RelativePath { get; set; }
        public long FileSize { get; set; }

        // Non serialized
        public Guid RemoteDeviceId { get; set; }

        public override NetMessageType MessageType => NetMessageType.FileStart;

        public override void Process(SyncerConnection con)
        {
            RemoteDeviceId = con.GetRemoteDeviceId();
            con.Owner.GetFileBuilder().MessageToQueue(this);
        }

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            ProfileId = stream.ReadGuid();
            ProfileKey = stream.ReadGuid();
            RelativePath = stream.ReadString();
            FileSize = stream.ReadInt64();
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.WriteAFS(ProfileId);
            stream.WriteAFS(ProfileKey);
            stream.WriteAFS(RelativePath);
            stream.WriteAFS(FileSize);
        }
    }
}
