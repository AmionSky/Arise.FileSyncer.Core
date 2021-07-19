using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Arise.FileSyncer.Core.Peer
{
    public class PeerProfiles
    {
        // (Profile ID, Profile Data)
        private readonly ConcurrentDictionary<Guid, SyncProfile> profiles;

        public PeerProfiles()
        {
            profiles = new ConcurrentDictionary<Guid, SyncProfile>(2, 0);
        }

        public ICollection<Guid> Ids => profiles.Keys;

        public int Count => profiles.Count;

        /// <summary>
        /// Gets the profile for the specified ID
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="profile">SyncProfile</param>
        public bool TryGetProfile(Guid profileId, out SyncProfile profile)
        {
            return profiles.TryGetValue(profileId, out profile);
        }

        /// <summary>
        /// Adds a new profile with the specified ID
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="profile">SyncProfile</param>
        public bool TryAdd(Guid profileId, SyncProfile profile)
        {
            return profiles.TryAdd(profileId, profile);
        }

        /// <summary>
        /// Removes a profile with the specified ID and returns it
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="profile">Removed SyncProfile</param>
        public bool TryRemove(Guid profileId, out SyncProfile profile)
        {
            return profiles.TryRemove(profileId, out profile);
        }

        /// <summary>
        /// Updates a profile with the specified ID
        /// </summary>
        public bool TryUpdate(Guid profileId, SyncProfile newProfile, SyncProfile compProfile)
        {
            return profiles.TryUpdate(profileId, newProfile, compProfile);
        }
    }
}
