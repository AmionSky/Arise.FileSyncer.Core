using System;
using System.IO;
using Arise.FileSyncer.Helpers;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Messages
{
    internal class VerificationDataMessage : NetMessage
    {
        public Guid Key { get; set; }

        public override NetMessageType MessageType => NetMessageType.VerificationData;

        public VerificationDataMessage() { }

        public static void Send(SyncerConnection con)
        {
            SyncerPeerSettings peerSettings = con.Owner.Settings;

            if (peerSettings.DeviceKeys.TryGetValue(con.GetRemoteDeviceId(), out Guid key))
            {
                VerificationDataMessage message = new VerificationDataMessage()
                {
                    Key = Security.KeyGenerator(key, peerSettings.DeviceId)
                };

                con.Send(message);
            }
            else if (con.Owner.AllowPairing)
            {
                con.Pair();
            }
            else
            {
                Log.Verbose("Non paired device connection. Disconnecting...");
                con.Disconnect();
            }
        }

        public override void Process(SyncerConnection con)
        {
            if (con.Owner.Settings.DeviceKeys.TryGetValue(con.GetRemoteDeviceId(), out Guid key)
                && Security.KeyGenerator(key, con.GetRemoteDeviceId()) == Key)
            {
                Log.Info("Verification successful");
                con.Verified = true;
                con.Send(new VerificationResponseMessage(true, con.Owner.Settings));
            }
            else if (con.Owner.AllowPairing)
            {
                // Do not disconnect so it can re-pair with device
                Log.Info("Verification failed - But allow pairing enabled");
                con.Send(new VerificationResponseMessage(false, con.Owner.Settings));
            }
            else
            {
                Log.Info("Verification failed");
                con.SendAndDisconnect(new VerificationResponseMessage(false, con.Owner.Settings));
            }
        }

        public override void Deserialize(Stream stream)
        {
            Key = stream.ReadGuid();
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(Key);
        }
    }
}
