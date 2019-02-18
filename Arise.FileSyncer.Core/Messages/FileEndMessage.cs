using System;
using System.IO;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Messages
{
    internal class FileEndMessage : FileMessageBase
    {
        public bool Success { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime CreationTime { get; set; }

        public override NetMessageType MessageType => NetMessageType.FileEnd;

        public FileEndMessage() { }

        public override void Process(SyncerConnection con)
        {
            con.Owner.GetFileBuilder().MessageToQueue(this);
        }

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            Success = stream.ReadBoolean();
            if (Success)
            {
                LastWriteTime = stream.ReadDateTime();
                CreationTime = stream.ReadDateTime();
            }
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.Write(Success);
            if (Success)
            {
                stream.Write(LastWriteTime);
                stream.Write(CreationTime);
            }
        }
    }
}
