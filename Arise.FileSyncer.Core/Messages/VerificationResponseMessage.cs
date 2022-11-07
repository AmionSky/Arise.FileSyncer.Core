using System;
using System.IO;
using System.Linq;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core.Messages
{
    internal sealed class VerificationResponseMessage : NetMessage
    {
        class Data
        {
            public string DisplayName;
            public bool SupportTimestamp;
            public Guid[] ProfileIds;

            public Data(string displayName, bool supportTimestamp, Guid[] profileIds)
            {
                DisplayName = displayName;
                SupportTimestamp = supportTimestamp;
                ProfileIds = profileIds;
            }
        }

        private Data? data { get; set; }


        public override NetMessageType MessageType => NetMessageType.VerificationResponse;

        public VerificationResponseMessage() { }

        public VerificationResponseMessage(bool success, SyncerPeer peer)
        {
            if (success)
            {
                data = new Data(peer.Settings.DisplayName, peer.Settings.SupportTimestamp, peer.Profiles.Ids.ToArray());
            }
        }

        public override void Process(SyncerConnection con)
        {
            if (data != null)
            {
                con.DisplayName = data.DisplayName;
                con.SupportTimestamp = data.SupportTimestamp;
                con.Owner.Connections.OnConnectionVerified(con.GetRemoteDeviceId(), data.DisplayName);
                Log.Verbose("Sending Initialization Data");
                con.Send(new SyncInitializationMessage(con.Owner, data.ProfileIds));
            }
            else
            {
                Log.Verbose("Verification refused");
            }
        }

        public override void Deserialize(Stream stream)
        {
            bool success = stream.ReadBoolean();

            if (success)
            {
                var displayName = stream.ReadString();
                var supportTimestamp = stream.ReadBoolean();
                var profileIds = stream.ReadGuidArray();

                data = new Data(displayName, supportTimestamp, profileIds);
            }
        }

        public override void Serialize(Stream stream)
        {
            stream.WriteAFS(data != null);

            if (data != null)
            {
                stream.WriteAFS(data.DisplayName);
                stream.WriteAFS(data.SupportTimestamp);
                stream.WriteAFS(data.ProfileIds);
            }
        }
    }
}
