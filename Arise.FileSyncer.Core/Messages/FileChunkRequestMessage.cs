using System.IO;

namespace Arise.FileSyncer.Core.Messages
{
    internal class FileChunkRequestMessage : NetMessage
    {
        public override NetMessageType MessageType => NetMessageType.FileChunkRequest;

        public override void Process(SyncerConnection con)
        {
            con.OnChunkRequest();
        }

        public override void Deserialize(Stream stream) { }

        public override void Serialize(Stream stream) { }
    }
}
