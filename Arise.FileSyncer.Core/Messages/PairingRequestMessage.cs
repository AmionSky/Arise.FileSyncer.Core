using System.IO;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core.Messages
{
    internal class PairingRequestMessage : NetMessage
    {
        public string DisplayName { get; set; }

        public override NetMessageType MessageType => NetMessageType.PairingRequest;

        public PairingRequestMessage() { }

        public PairingRequestMessage(string displayName)
        {
            DisplayName = displayName;
        }

        public override void Process(SyncerConnection con)
        {
            if (!con.Verified && con.Owner.AllowPairing)
            {
                con.Owner.OnPairingRequest(new PairingRequestEventArgs(
                    DisplayName, con.GetRemoteDeviceId(), con.OnPairingRequestCallback));
            }
        }

        public override void Deserialize(Stream stream)
        {
            DisplayName = stream.ReadString();
        }

        public override void Serialize(Stream stream)
        {
            stream.WriteAFS(DisplayName);
        }
    }
}
