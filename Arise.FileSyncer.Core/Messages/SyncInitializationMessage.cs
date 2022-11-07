using System;
using System.Collections.Generic;
using System.IO;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core.Messages
{
    internal sealed class SyncInitializationMessage : NetMessage
    {
        private SyncProfileState[] profileStates;

        public override NetMessageType MessageType => NetMessageType.SyncInitialization;

        public SyncInitializationMessage() { profileStates = Array.Empty<SyncProfileState>(); }

        public SyncInitializationMessage(SyncerPeer peer, Guid[] receivedIds)
        {
            var states = new List<SyncProfileState>();

            foreach (var profileId in receivedIds)
            {
                if (peer.Profiles.GetProfile(profileId, out var profile))
                {
                    if (profile.Activated && profile.AllowReceive)
                    {
                        if (SyncProfileState.Create(profileId, profile) is SyncProfileState profileState)
                        {
                            states.Add(profileState);
                        }
                        else
                        {
                            peer.Profiles.OnProfileError(profileId, profile, SyncProfileError.FailedToGetState);
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
            stream.WriteAFS(profileStates);
        }
    }
}
