using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Arise.FileSyncer.Core.Peer
{
    public class PeerProfiles
    {
        /// <summary>
        /// Called when a profile got changed or updated.
        /// </summary>
        public event EventHandler<ProfileEventArgs> ProfileChanged;
        /// <summary>
        /// Called when a new profile got added.
        /// </summary>
        public event EventHandler<ProfileEventArgs> ProfileAdded;
        /// <summary>
        /// Called when a profile got removed.
        /// </summary>
        public event EventHandler<ProfileEventArgs> ProfileRemoved;
        /// <summary>
        /// Called when a profile encountered an error.
        /// </summary>
        public event EventHandler<ProfileErrorEventArgs> ProfileError;
        /// <summary>
        /// Called when a new profile got received from a remote device.
        /// </summary>
        public event EventHandler<ProfileReceivedEventArgs> ProfileReceived;

        /// <summary>
        /// Collection of the currently available IDs
        /// </summary>
        public ICollection<Guid> Ids => profiles.Keys;
        /// <summary>
        /// Number of saved profiles
        /// </summary>
        public int Count => profiles.Count;

        // Profile ID, Profile Data
        private readonly ConcurrentDictionary<Guid, SyncProfile> profiles;

        public PeerProfiles()
        {
            profiles = new ConcurrentDictionary<Guid, SyncProfile>(2, 0);
        }

        /// <summary>
        /// Adds a new profile to the peer profiles
        /// </summary>
        /// <param name="profileId">ID of the profile to add</param>
        /// <param name="newProfile">The new profile</param>
        public bool AddProfile(Guid profileId, SyncProfile newProfile)
        {
            if (profiles.TryAdd(profileId, newProfile))
            {
                Log.Info($"Profile added: {newProfile.Name}");
                OnProfileAdded(profileId, newProfile);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a profile form the peer profiles
        /// </summary>
        /// <param name="profileId">ID of the profile to remove</param>
        public bool RemoveProfile(Guid profileId)
        {
            if (profiles.TryRemove(profileId, out var profile))
            {
                Log.Info($"Profile removed: {profile.Name}");
                OnProfileRemoved(profileId, profile);
                return true;
            }

            Log.Warning($"Profile remove failed! ID: {profileId}");
            return false;
        }

        /// <summary>
        /// Updates the specified profile
        /// </summary>
        /// <param name="profileId">ID of the profile to update</param>
        /// <param name="newProfile">Updated profile</param>
        /// <returns></returns>
        public bool UpdateProfile(Guid profileId, SyncProfile newProfile)
        {
            if (GetProfile(profileId, out var profile))
            {
                if (profiles.TryUpdate(profileId, newProfile, profile))
                {
                    Log.Info($"Profile updated: {profileId} - {newProfile.Name}");
                    OnProfileChanged(profileId, newProfile);
                    return true;
                }
            }

            Log.Warning($"Profile update failed! ID: {profileId}");
            return false;
        }

        /// <summary>
        /// Gets the profile for the specified ID
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="profile">SyncProfile</param>
        public bool GetProfile(Guid profileId, out SyncProfile profile)
        {
            return profiles.TryGetValue(profileId, out profile);
        }

        // Events
        private void OnProfileAdded(Guid profileId, SyncProfile profile)
        {
            ProfileAdded?.Invoke(this, new ProfileEventArgs()
            {
                Id = profileId,
                Profile = profile,
            });
        }

        private void OnProfileRemoved(Guid profileId, SyncProfile profile)
        {
            ProfileRemoved?.Invoke(this, new ProfileEventArgs()
            {
                Id = profileId,
                Profile = profile,
            });
        }

        internal virtual void OnProfileChanged(Guid profileId, SyncProfile profile)
        {
            ProfileChanged?.Invoke(this, new ProfileEventArgs()
            {
                Id = profileId,
                Profile = profile,
            });
        }

        internal virtual void OnProfileError(Guid profileId, SyncProfile profile, SyncProfileError error)
        {
            ProfileError?.Invoke(this, new ProfileErrorEventArgs()
            {
                Id = profileId,
                Profile = profile,
                Error = error,
            });
        }

        internal virtual void OnProfileReceived(ProfileReceivedEventArgs e)
        {
            ProfileReceived?.Invoke(this, e);
        }
    }
}
