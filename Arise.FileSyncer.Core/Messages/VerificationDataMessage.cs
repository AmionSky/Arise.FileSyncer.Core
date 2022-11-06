using System;
using System.IO;
using Arise.FileSyncer.Core.Helpers;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core.Messages
{
    internal sealed class VerificationDataMessage : NetMessage
    {
        public Guid Key { get; set; }

        public override NetMessageType MessageType => NetMessageType.VerificationData;

        public VerificationDataMessage() { }

        public static void Send(SyncerConnection connection)
        {
            var deviceKeys = connection.Owner.DeviceKeys;
            var deviceId = connection.Owner.Settings.DeviceId;
            var remoteDeviceId = connection.GetRemoteDeviceId();

            if (deviceKeys.GetVerificationKey(remoteDeviceId, out Guid key))
            {
                VerificationDataMessage message = new()
                {
                    Key = Security.KeyGenerator(key, deviceId)
                };

                connection.Send(message);
            }
            else if (connection.Owner.AllowPairing)
            {
                // TODO proper paring
                if (remoteDeviceId.ToString().CompareTo(deviceId.ToString()) > 0)
                {
                    connection.Pair();
                }
            }
            else
            {
                Log.Verbose("Non paired device connection. Disconnecting...");
                connection.Disconnect();
            }
        }

        public override void Process(SyncerConnection con)
        {
            if (con.Owner.DeviceKeys.GetVerificationKey(con.GetRemoteDeviceId(), out Guid key)
                && Security.KeyGenerator(key, con.GetRemoteDeviceId()) == Key)
            {
                Log.Info("Verification successful");
                con.Verified = true;
                con.Send(new VerificationResponseMessage(true, con.Owner));
            }
            else if (con.Owner.AllowPairing)
            {
                // Do not disconnect so it can re-pair with device
                Log.Info("Verification failed - But allow pairing enabled");
                con.Send(new VerificationResponseMessage(false, con.Owner));
            }
            else
            {
                Log.Info("Verification failed");
                con.SendAndDisconnect(new VerificationResponseMessage(false, con.Owner));
            }
        }

        public override void Deserialize(Stream stream)
        {
            Key = stream.ReadGuid();
        }

        public override void Serialize(Stream stream)
        {
            stream.WriteAFS(Key);
        }
    }
}
