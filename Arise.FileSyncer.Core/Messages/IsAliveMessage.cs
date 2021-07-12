using System.IO;

namespace Arise.FileSyncer.Core.Messages
{
    internal class IsAliveMessage : NetMessage
    {
        public override NetMessageType MessageType => NetMessageType.IsAlive;

        public override void Deserialize(Stream stream) { }
        public override void Process(SyncerConnection con) { }
        public override void Serialize(Stream stream) { }
    }
}
