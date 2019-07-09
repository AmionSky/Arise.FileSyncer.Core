using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Arise.FileSyncer.Core.FileSync;
using Arise.FileSyncer.Core.Messages;
using Arise.FileSyncer.Core.Plugins;

namespace Arise.FileSyncer.Core
{
    /// <summary>
    /// Core of the syncer. Stores connections, profiles and handles communication.
    /// </summary>
    public class SyncerPeer : IDisposable
    {
        /// <summary>
        /// Is the local device supports file timestamp get and set.
        /// </summary>
        public static bool SupportTimestamp = true;

        /// <summary>
        /// Called when a new connection is successfully added.
        /// </summary>
        public event EventHandler<ConnectionAddedEventArgs> ConnectionAdded;

        /// <summary>
        /// Called when a connection got verified and we know basic information about the remote device.
        /// </summary>
        public event EventHandler<ConnectionVerifiedEventArgs> ConnectionVerified;

        /// <summary>
        /// Called when a connection is successfully removed.
        /// </summary>
        public event EventHandler<ConnectionRemovedEventArgs> ConnectionRemoved;

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
        /// Called when received a pairing request from a remote device.
        /// Containes callback to accept/refuse the request.
        /// </summary>
        public event EventHandler<PairingRequestEventArgs> PairingRequest;

        /// <summary>
        /// Called when a new pair has been successfully created.
        /// </summary>
        public event EventHandler<NewPairAddedEventArgs> NewPairAdded;

        /// <summary>
        /// [Async] Called when the file builder completed a file.
        /// </summary>
        public event EventHandler<FileBuiltEventArgs> FileBuilt;

        /// <summary>
        /// Allow sending and receiving pairing requests.
        /// </summary>
        public bool AllowPairing { get => _allowPairing; set => _allowPairing = value; }

        /// <summary>
        /// The peer settings class.
        /// </summary>
        public SyncerPeerSettings Settings { get; }

        /// <summary>
        /// The plugin manager class.
        /// </summary>
        public PluginManager Plugins { get; }

        private readonly Lazy<FileBuilder> fileBuilder;
        private readonly ConcurrentDictionary<Guid, SyncerConnection> connections;

        private volatile bool _allowPairing;

        /// <summary>
        /// Creates a new peer with the specified settings.
        /// </summary>
        public SyncerPeer(SyncerPeerSettings settings)
        {
            Settings = settings;
            AllowPairing = false;

            Plugins = new PluginManager();

            fileBuilder = new Lazy<FileBuilder>(() => new FileBuilder(this));
            connections = new ConcurrentDictionary<Guid, SyncerConnection>();
        }

        /// <summary>
        /// Adds a new connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns>Successful</returns>
        public bool AddConnection(INetConnection connection)
        {
            if (connection == null) return false;

            SyncerConnection syncerConnection = new SyncerConnection(this, connection);
            bool added = connections.TryAdd(connection.Id, syncerConnection);

            if (added)
            {
                OnConnectionAdded(new ConnectionAddedEventArgs(connection.Id));
                syncerConnection.Start();
            }
            else
            {
                syncerConnection.Dispose();
            }

            return added;
        }

        /// <summary>
        /// Removes a connection.
        /// </summary>
        /// <param name="id">The ID of the connection. (Remote Device ID)</param>
        /// <returns>Successful</returns>
        public bool RemoveConnection(Guid id)
        {
            bool removed = connections.TryRemove(id, out SyncerConnection syncerConnection);

            if (removed)
            {
                syncerConnection.Dispose();
                OnConnectionRemoved(new ConnectionRemovedEventArgs(id));
            }

            return removed;
        }

        /// <summary>
        /// Returns an array of the connection IDs.
        /// </summary>
        /// <returns>Connections</returns>
        public ICollection<Guid> GetConnectionIds()
        {
            return connections.Keys;
        }

        /// <summary>
        /// Returns the number of connections.
        /// </summary>
        /// <returns>The number of connections</returns>
        public int GetConnectionCount()
        {
            return connections.Count;
        }

        /// <summary>
        /// Checks if the connection specified by the ID exists already.
        /// </summary>
        /// <param name="id">The ID of the connection. (Remote Device ID)</param>
        /// <returns>Connection already exists</returns>
        public bool DoesConnectionExist(Guid id)
        {
            return connections.ContainsKey(id);
        }

        /// <summary>
        /// Returns if any of the underlying systems (including connections) currently executing important logic.
        /// </summary>
        public bool IsSyncing()
        {
            if (fileBuilder.IsValueCreated)
            {
                if (!fileBuilder.Value.IsBuildQueueEmpty()) return true;
            }

            foreach (var connectionKV in connections)
            {
                if (connectionKV.Value.IsSyncing()) return true;
            }

            return false;
        }

        /// <summary>
        /// Tries getting a connection.
        /// </summary>
        /// <param name="id">ID of the connection</param>
        /// <param name="connection">The connection as a public interface</param>
        /// <returns></returns>
        public bool TryGetConnection(Guid id, out ISyncerConnection connection)
        {
            bool success = connections.TryGetValue(id, out SyncerConnection fullConnection);
            connection = fullConnection;
            return success;
        }

        /// <summary>
        /// Shares the specified profile with the given connection.
        /// </summary>
        /// <param name="connectionId">ID of the connection</param>
        /// <param name="profileId">ID of the profile to share</param>
        /// <returns></returns>
        public bool ShareProfile(Guid connectionId, Guid profileId)
        {
            if (Settings.Profiles.TryGetValue(profileId, out var profile))
            {
                return TrySend(connectionId, new ProfileShareMessage(profileId, profile));
            }

            return false;
        }

        /// <summary>
        /// Starts the sync process on a given profile with a given connection.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="profileId">ID of the profile to sync</param>
        /// <param name="isResponse">Is it a response message. Should always be false.</param>
        public bool SyncProfile(Guid connectionId, Guid profileId, bool isResponse = false)
        {
            if (!Settings.Profiles.TryGetValue(profileId, out var profile))
            {
                return false;
            }

            SyncProfileState profileState;

            if (profile.AllowReceive)
            {
                profileState = SyncProfileState.Create(profileId, profile);

                if (profileState == null)
                {
                    OnProfileError(new ProfileErrorEventArgs(profileId, profile, SyncProfileError.FailedToGetState));
                    return false;
                }
            }
            else
            {
                profileState = new SyncProfileState(profileId, profile.Key, profile.AllowDelete, null);
            }

            return TrySend(connectionId, new SyncProfileMessage(profileState, isResponse));
        }

        /// <summary>
        /// Adds a new profile to the peer settings.
        /// </summary>
        /// <param name="profileId">ID of the profile to add</param>
        /// <param name="newProfile">The new profile</param>
        /// <returns></returns>
        public bool AddProfile(Guid profileId, SyncProfile newProfile)
        {
            if (Settings.Profiles.TryAdd(profileId, newProfile))
            {
                Log.Info($"Profile added: {newProfile.Name}");
                OnProfileAdded(new ProfileEventArgs(profileId, newProfile));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a profile form the peer settings.
        /// </summary>
        /// <param name="profileId">ID of the profile to remove</param>
        /// <returns></returns>
        public bool RemoveProfile(Guid profileId)
        {
            if (Settings.Profiles.TryRemove(profileId, out var profile))
            {
                Log.Info($"Profile removed: {profile.Name}");
                OnProfileRemoved(new ProfileEventArgs(profileId, profile));
                return true;
            }

            Log.Warning($"Profile remove failed! ID: {profileId}");
            return false;
        }

        /// <summary>
        /// Updates a selected profile
        /// </summary>
        /// <param name="profileId">ID of the profile to update</param>
        /// <param name="newProfile">Updated profile</param>
        /// <returns></returns>
        public bool UpdateProfile(Guid profileId, SyncProfile newProfile)
        {
            if (Settings.Profiles.TryGetValue(profileId, out var profile))
            {
                if (Settings.Profiles.TryUpdate(profileId, newProfile, profile))
                {
                    Log.Info($"Profile updated: {profileId} - {profile.Name}");
                    OnProfileChanged(new ProfileEventArgs(profileId, newProfile));
                    return true;
                }
            }

            Log.Warning($"Profile update failed! ID: {profileId}");
            return false;
        }

        internal bool TrySend(Guid connectionId, NetMessage message)
        {
            bool found = connections.TryGetValue(connectionId, out SyncerConnection connection);
            if (found) connection.Send(message);
            return found;
        }

        internal FileBuilder GetFileBuilder()
        {
            return fileBuilder.Value;
        }

        internal virtual void OnConnectionAdded(ConnectionAddedEventArgs e)
        {
            ConnectionAdded?.Invoke(this, e);
        }

        internal virtual void OnConnectionVerified(ConnectionVerifiedEventArgs e)
        {
            ConnectionVerified?.Invoke(this, e);
        }

        internal virtual void OnConnectionRemoved(ConnectionRemovedEventArgs e)
        {
            ConnectionRemoved?.Invoke(this, e);
        }

        internal virtual void OnProfileChanged(ProfileEventArgs e)
        {
            ProfileChanged?.Invoke(this, e);
        }

        internal virtual void OnProfileAdded(ProfileEventArgs e)
        {
            ProfileAdded?.Invoke(this, e);
        }

        internal virtual void OnProfileRemoved(ProfileEventArgs e)
        {
            ProfileRemoved?.Invoke(this, e);
        }

        internal virtual void OnProfileError(ProfileErrorEventArgs e)
        {
            ProfileError?.Invoke(this, e);
        }

        internal virtual void OnProfileReceived(ProfileReceivedEventArgs e)
        {
            ProfileReceived?.Invoke(this, e);
        }

        internal virtual void OnPairingRequest(PairingRequestEventArgs e)
        {
            PairingRequest?.Invoke(this, e);
        }

        internal virtual void OnNewPairAdded(NewPairAddedEventArgs e)
        {
            NewPairAdded?.Invoke(this, e);
        }

        internal virtual void OnFileBuilt(FileBuiltEventArgs e)
        {
            FileBuilt?.Invoke(this, e);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    if (fileBuilder.IsValueCreated)
                    {
                        fileBuilder.Value.Dispose();
                    }

                    foreach (KeyValuePair<Guid, SyncerConnection> con in connections)
                    {
                        con.Value.Dispose();
                    }

                    connections.Clear();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // Override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SyncerPeer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // Uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
