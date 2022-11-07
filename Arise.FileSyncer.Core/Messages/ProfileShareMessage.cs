using System;
using System.IO;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core.Messages
{
    internal sealed class ProfileShareMessage : NetMessage
    {
        public SyncProfileShare ProfileShare { get; set; }

        public ProfileShareMessage() { }

        public ProfileShareMessage(Guid profileId, SyncProfile profile)
        {
            ProfileShare = new SyncProfileShare
            {
                Id = profileId,
                Key = profile.Key,
                Name = profile.Name,
                CreationDate = profile.CreationDate,
                SkipHidden = profile.SkipHidden,
            };
        }

        public override NetMessageType MessageType => NetMessageType.ProfileShare;

        public override void Process(SyncerConnection con)
        {
            con.Owner.Profiles.OnProfileReceived(new ProfileReceivedEventArgs(con.GetRemoteDeviceId(), ProfileShare));
        }

        public override void Deserialize(Stream stream)
        {
            ProfileShare = stream.Read<SyncProfileShare>();
        }

        public override void Serialize(Stream stream)
        {
            stream.WriteAFS(ProfileShare);
        }
    }
}
