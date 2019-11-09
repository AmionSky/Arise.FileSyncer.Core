using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Arise.FileSyncer.Core.Serializer;
using Arise.FileSyncer.Core.FileSync;
using Arise.FileSyncer.Core.Helpers;

namespace Arise.FileSyncer.Core.Messages
{
    internal class DeleteFilesMessage : NetMessage
    {
        public Guid ProfileId { get; set; }
        public Guid Key { get; set; }
        public IList<string> Files { get; set; }

        public override NetMessageType MessageType => NetMessageType.DeleteFiles;

        public override void Process(SyncerConnection con)
        {
            if (con.Owner.Settings.Profiles.TryGetValue(ProfileId, out var profile))
            {
                if (profile.AllowDelete && profile.Key == Key)
                {
                    for (int i = 0; i < Files.Count; i++)
                    {
                        Utility.FileDelete(ProfileId, profile.RootDirectory, PathHelper.GetCorrect(Files[i], false));
                    }

                    if (Files.Count > 0)
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
            Files = stream.ReadStringArray();
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(ProfileId);
            stream.Write(Key);
            stream.Write(Files);
        }
    }
}
