using System;
using System.IO;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core.Messages
{
    internal sealed class ProfileShareMessage : NetMessage
    {
        public Guid Id { get; set; }
        public Guid Key { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public bool SkipHidden { get; set; }

        public ProfileShareMessage() { }

        public ProfileShareMessage(Guid profileId, SyncProfile profile)
        {
            Id = profileId;
            Key = profile.Key;
            Name = profile.Name;
            CreationDate = profile.CreationDate;
            SkipHidden = profile.SkipHidden;
        }

        public override NetMessageType MessageType => NetMessageType.ProfileShare;

        public override void Process(SyncerConnection con)
        {
            con.Owner.Profiles.OnProfileReceived(new ProfileReceivedEventArgs(con.GetRemoteDeviceId(), this));
        }

        public override void Deserialize(Stream stream)
        {
            Id = stream.ReadGuid();
            Key = stream.ReadGuid();
            Name = stream.ReadString();
            CreationDate = stream.ReadDateTime();
            SkipHidden = stream.ReadBoolean();
        }

        public override void Serialize(Stream stream)
        {
            stream.WriteAFS(Id);
            stream.WriteAFS(Key);
            stream.WriteAFS(Name);
            stream.WriteAFS(CreationDate);
            stream.WriteAFS(SkipHidden);
        }
    }
}
