using System;
using System.Collections.Generic;
using System.IO;
using Arise.FileSyncer.Core.Serializer;

namespace Arise.FileSyncer.Core.Messages
{
    internal class SyncInitializationMessage : NetMessage
    {
        private SyncProfileState[] profileStates;

        public override NetMessageType MessageType => NetMessageType.SyncInitialization;

        public SyncInitializationMessage() { }

        public SyncInitializationMessage(SyncerPeer peer, Guid[] receivedIds)
        {
            var states = new List<SyncProfileState>();

            foreach (var profileId in receivedIds)
            {
                if (peer.Settings.Profiles.TryGetValue(profileId, out var profile))
                {
                    if (profile.Activated && profile.AllowReceive)
                    {
                        if (SyncProfileState.Create(profileId, profile) is SyncProfileState profileState)
                        {
                            states.Add(profileState);
                        }
                        else
                        {
                            peer.OnProfileError(new ProfileErrorEventArgs(profileId, profile, SyncProfileError.FailedToGetState));
                        }
                    }
                }
            }

            profileStates = states.ToArray();
        }

        public override void Process(SyncerConnection con)
        {
            Log.Verbose("Received SyncInitialization Message");

            foreach (SyncProfileState remoteProfile in profileStates)
            {
                con.StartProfileSync(remoteProfile);
            }

            con.Send(new SyncInitFinishedMessage());
        }

        public override void Deserialize(Stream stream)
        {
            profileStates = stream.ReadArray<SyncProfileState>();
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(profileStates);
        }
    }
}
