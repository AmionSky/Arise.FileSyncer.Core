using System;
using System.Collections.Generic;
using System.IO;
using Arise.FileSyncer.Core.FileSync;
using Arise.FileSyncer.Core.Helpers;
using Arise.FileSyncer.Core.Serializer;

namespace Arise.FileSyncer.Core.Messages
{
    internal class CreateDirectoriesMessage : NetMessage
    {
        public Guid ProfileId { get; set; }
        public Guid Key { get; set; }
        public IList<string> Directories { get; set; }

        public override NetMessageType MessageType => NetMessageType.CreateDirectories;

        public CreateDirectoriesMessage() { }

        public CreateDirectoriesMessage(Guid profileId, SyncProfile syncProfile, string[] directories)
        {
            ProfileId = profileId;
            Key = syncProfile.Key;
            Directories = directories;
        }

        public override void Process(SyncerConnection con)
        {
            if (con.Owner.Settings.Profiles.TryGetValue(ProfileId, out var profile))
            {
                if (profile.AllowReceive && profile.Key == Key)
                {
                    for (int i = 0; i < Directories.Count; i++)
                    {
                        Utility.DirectoryCreate(ProfileId, profile.RootDirectory, PathHelper.GetCorrect(Directories[i], true));
                    }

                    if (Directories.Count > 0)
                    {
                        con.Owner.OnProfileChanged(new ProfileEventArgs(ProfileId, profile));
                    }
                }
            }
        }

        public override void Deserialize(Stream stream)
        {
            ProfileId = stream.ReadGuid();
            Key = stream.ReadGuid();
            Directories = stream.ReadStringArray();
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(ProfileId);
            stream.Write(Key);
            stream.Write(Directories);
        }
    }
}
