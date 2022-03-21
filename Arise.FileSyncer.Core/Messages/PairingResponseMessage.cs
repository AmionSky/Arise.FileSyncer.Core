using System;
using System.IO;
using Arise.FileSyncer.Core.Components;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core.Messages
{
    internal class PairingResponseMessage : NetMessage
    {
        public bool Accepted { get; set; }
        public Guid RawKey { get; set; }

        public override NetMessageType MessageType => NetMessageType.PairingResponse;

        public PairingResponseMessage() { }

        public static PairingResponseMessage Accept(Guid rawKey)
        {
            return new PairingResponseMessage()
            {
                Accepted = true,
                RawKey = rawKey,
            };
        }

        public static PairingResponseMessage Refuse()
        {
            return new PairingResponseMessage() { Accepted = false };
        }

        public override void Process(SyncerConnection con)
        {
            if (Accepted && con.Pairing)
            {
                con.AddDeviceKey(con.GetRemoteDeviceId(), RawKey);

                // Start verification
                VerificationDataMessage.Send(con);
            }

            con.Pairing = false;
        }

        public override void Deserialize(Stream stream)
        {
            Accepted = stream.ReadBoolean();
            if (Accepted) RawKey = stream.ReadGuid();
        }

        public override void Serialize(Stream stream)
        {
            stream.WriteAFS(Accepted);
            if (Accepted) stream.WriteAFS(RawKey);
        }
    }
}
