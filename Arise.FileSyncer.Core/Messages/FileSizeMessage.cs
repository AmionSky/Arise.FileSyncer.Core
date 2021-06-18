using System.IO;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core.Messages
{
    internal class FileSizeMessage : NetMessage
    {
        public long OverallSendSize { get; set; }

        public override NetMessageType MessageType => NetMessageType.FileSize;

        public FileSizeMessage() { }

        public FileSizeMessage(long overallSendSize)
        {
            OverallSendSize = overallSendSize;
        }

        public override void Process(SyncerConnection con)
        {
            con.Progress.AddMaximum(OverallSendSize);
        }

        public override void Deserialize(Stream stream)
        {
            OverallSendSize = stream.ReadInt64();
        }

        public override void Serialize(Stream stream)
        {
            stream.WriteAFS(OverallSendSize);
        }
    }
}
