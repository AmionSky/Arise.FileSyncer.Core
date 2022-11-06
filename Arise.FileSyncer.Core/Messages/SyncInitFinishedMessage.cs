using System.IO;

namespace Arise.FileSyncer.Core.Messages
{
    internal sealed class SyncInitFinishedMessage : NetMessage
    {
        public override NetMessageType MessageType => NetMessageType.SyncInitFinished;

        public override void Process(SyncerConnection con)
        {
            // Init finished and all progress info was sent, so Progress.Max was set
            con.Progress.Indeterminate = false;
        }

        public override void Deserialize(Stream stream) { }
        public override void Serialize(Stream stream) { }
    }
}
