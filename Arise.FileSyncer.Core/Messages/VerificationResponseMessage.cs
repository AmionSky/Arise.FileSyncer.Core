using System;
using System.IO;
using System.Linq;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Messages
{
    internal class VerificationResponseMessage : NetMessage
    {
        public bool Success { get; set; }

        public string DisplayName { get; set; }
        public bool SupportTimestamp { get; set; }

        public Guid[] ProfileIds { get; set; }

        public override NetMessageType MessageType => NetMessageType.VerificationResponse;

        public VerificationResponseMessage() { }

        public VerificationResponseMessage(bool success, SyncerPeerSettings peerSettings)
        {
            Success = success;

            if (success)
            {
                Success = success;
                DisplayName = peerSettings.DisplayName;
                SupportTimestamp = SyncerPeer.SupportTimestamp;
                ProfileIds = peerSettings.Profiles.Keys.ToArray();
            }
        }

        public override void Process(SyncerConnection con)
        {
            if (Success)
            {
                con.DisplayName = DisplayName;
                con.SupportTimestamp = SupportTimestamp;
                con.Owner.OnConnectionVerified(new ConnectionVerifiedEventArgs(con.GetRemoteDeviceId(), DisplayName));
                Log.Verbose("Sending Initialization Data");
                con.Send(new SyncInitializationMessage(con.Owner, ProfileIds));
            }
            else
            {
                Log.Verbose("Verification refused");
            }
        }

        public override void Deserialize(Stream stream)
        {
            Success = stream.ReadBoolean();

            if (Success)
            {
                DisplayName = stream.ReadString();
                SupportTimestamp = stream.ReadBoolean();

                ProfileIds = stream.ReadGuidArray();
            }
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(Success);

            if (Success)
            {
                stream.Write(DisplayName);
                stream.Write(SupportTimestamp);

                stream.Write(ProfileIds);
            }
        }
    }
}
