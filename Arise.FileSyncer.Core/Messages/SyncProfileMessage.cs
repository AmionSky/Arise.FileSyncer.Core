using System.IO;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core.Messages
{
    internal sealed class SyncProfileMessage : NetMessage
    {
        private SyncProfileState profileState;
        private bool isResponse;

        public override NetMessageType MessageType => NetMessageType.SyncProfile;

        public SyncProfileMessage() { }

        public SyncProfileMessage(SyncProfileState profileState)
        {
            this.profileState = profileState;
            isResponse = false;
        }

        public SyncProfileMessage(SyncProfileState profileState, bool isResponse)
        {
            this.profileState = profileState;
            this.isResponse = isResponse;
        }

        public override void Process(SyncerConnection con)
        {
            // TODO: Protect againts syncing the same profile multiple times at the same time
            Log.Verbose("Manual profile sync request");

            if (profileState.State != null)
            {
                con.StartProfileSync(profileState);
            }

            if (!isResponse)
            {
                if (!con.Owner.SyncProfile(con.GetRemoteDeviceId(), profileState.Id, true))
                {
                    Log.Warning("Failed to send response SyncProfileMessage");
                }
            }
        }

        public override void Deserialize(Stream stream)
        {
            profileState = stream.Read<SyncProfileState>();
            isResponse = stream.ReadBoolean();
        }

        public override void Serialize(Stream stream)
        {
            stream.WriteAFS(profileState);
            stream.WriteAFS(isResponse);
        }
    }
}
