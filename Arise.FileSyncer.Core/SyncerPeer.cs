using System;
using System.Threading;
using System.Threading.Tasks;
using Arise.FileSyncer.Core.FileSync;
using Arise.FileSyncer.Core.Messages;
using Arise.FileSyncer.Core.Peer;
using Arise.FileSyncer.Core.Plugins;

namespace Arise.FileSyncer.Core
{
    /// <summary>
    /// Core of the syncer. Stores connections, profiles and handles communication.
    /// </summary>
    public class SyncerPeer : IDisposable
    {
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
        public bool AllowPairing
        {
            get => Interlocked.Read(ref allowPairing) == 1;
            set => Interlocked.Exchange(ref allowPairing, Convert.ToInt64(value));
        }

        /// <summary>
        /// Manager of the peer's connections
        /// </summary>
        public ConnectionManager Connections { get; }
        /// <summary>
        /// Manager of paired devices keys
        /// </summary>
        public DeviceKeyManager DeviceKeys { get; }
        /// <summary>
        /// Manager of saved profiles
        /// </summary>
        public ProfileManager Profiles { get; }
        /// <summary>
        /// The plugin manager class.
        /// </summary>
        public PluginManager Plugins { get; }

        /// <summary>
        /// The peer settings class.
        /// </summary>
        public SyncerPeerSettings Settings { get; }

        private readonly Lazy<FileBuilder> fileBuilder;
        private long allowPairing = 0;

        /// <summary>
        /// Creates a new peer with the specified settings.
        /// </summary>
        public SyncerPeer(SyncerPeerSettings settings)
        {
            Settings = settings;
            AllowPairing = false;

            Connections = new ConnectionManager(this);
            DeviceKeys = new DeviceKeyManager();
            Profiles = new ProfileManager();
            Plugins = new PluginManager();

            fileBuilder = new Lazy<FileBuilder>(() => new FileBuilder(this));
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

            foreach (var connection in Connections.GetConnections())
            {
                if (connection.IsSyncing()) return true;
            }

            return false;
        }

        /// <summary>
        /// Shares the specified profile with the given connection.
        /// </summary>
        /// <param name="connectionId">ID of the connection</param>
        /// <param name="profileId">ID of the profile to share</param>
        public bool ShareProfile(Guid connectionId, Guid profileId)
        {
            if (Profiles.GetProfile(profileId, out var profile))
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
            if (!Profiles.GetProfile(profileId, out var profile))
            {
                return false;
            }

            SyncProfileState profileState;

            if (profile.AllowReceive)
            {
                profileState = SyncProfileState.Create(profileId, profile);

                if (profileState == null)
                {
                    Profiles.OnProfileError(profileId, profile, SyncProfileError.FailedToGetState);
                    return false;
                }
            }
            else
            {
                profileState = new SyncProfileState(profileId, profile.Key, profile.AllowDelete, null);
            }

            return TrySend(connectionId, new SyncProfileMessage(profileState, isResponse));
        }

        internal bool TrySend(Guid connectionId, NetMessage message)
        {
            bool found = Connections.TryGetConnection(connectionId, out ISyncerConnection connection);
            if (found) (connection as SyncerConnection).Send(message);
            return found;
        }

        internal FileBuilder GetFileBuilder()
        {
            return fileBuilder.Value;
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
            Task.Run(() => FileBuilt?.Invoke(this, e));
        }

        #region IDisposable Support
        private bool disposedValue;

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

                    Connections.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override finalizer
                // Set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
