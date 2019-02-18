using System.IO;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Messages
{
    internal class SyncProfileMessage : NetMessage
    {
        private SyncProfileState profileState;

        public override NetMessageType MessageType => NetMessageType.SyncProfile;

        public SyncProfileMessage() { }

        public SyncProfileMessage(SyncProfileState profileState)
        {
            this.profileState = profileState;
        }

        public override void Process(SyncerConnection con)
        {
            // TODO: Protect againts syncing the same profile multiple times at the same time
            Log.Verbose("Manual profile sync request");

            if (profileState.State != null)
            {
                con.StartProfileSync(profileState);
            }

            if (!profileState.IsResponse)
            {
                con.Owner.SyncProfile(con.GetRemoteDeviceId(), profileState.Id, true);
            }
        }

        public override void Deserialize(Stream stream)
        {
            profileState = stream.Read<SyncProfileState>();
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(profileState);
        }
    }
}
