using System;
using System.IO;
using System.Linq;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core.Messages
{
    internal sealed class VerificationResponseMessage : NetMessage
    {
        public bool Success { get; set; }

        public string DisplayName { get; set; }
        public bool SupportTimestamp { get; set; }
        public Guid[] ProfileIds { get; set; }

        public override NetMessageType MessageType => NetMessageType.VerificationResponse;

        public VerificationResponseMessage() { }

        public VerificationResponseMessage(bool success, SyncerPeer peer)
        {
            Success = success;

            if (success)
            {
                Success = success;
                DisplayName = peer.Settings.DisplayName;
                SupportTimestamp = peer.Settings.SupportTimestamp;
                ProfileIds = peer.Profiles.Ids.ToArray();
            }
        }

        public override void Process(SyncerConnection con)
        {
            if (Success)
            {
                con.DisplayName = DisplayName;
                con.SupportTimestamp = SupportTimestamp;
                con.Owner.Connections.OnConnectionVerified(con.GetRemoteDeviceId(), DisplayName);
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
            stream.WriteAFS(Success);

            if (Success)
            {
                stream.WriteAFS(DisplayName);
                stream.WriteAFS(SupportTimestamp);
                stream.WriteAFS(ProfileIds);
            }
        }
    }
}
