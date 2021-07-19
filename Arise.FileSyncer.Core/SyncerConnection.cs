using System;
using Arise.FileSyncer.Core.Components;
using Arise.FileSyncer.Core.FileSync;
using Arise.FileSyncer.Core.Messages;

namespace Arise.FileSyncer.Core
{
    public interface ISyncerConnection
    {
        /// <summary>
        /// Is the connection verified and allows syncronization.
        /// </summary>
        bool Verified { get; }

        /// <summary>
        /// Does the remote device supports file timestamp changes.
        /// </summary>
        bool SupportTimestamp { get; }

        /// <summary>
        /// Name of the remote device.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Current progress of the syncronization process.
        /// </summary>
        ProgressCounter Progress { get; }

        /// <summary>
        /// Returns true if the connection is currently syncing.
        /// </summary>
        bool IsSyncing();
    }

    internal class SyncerConnection : ISyncerConnection, IDisposable
    {
        public SyncerPeer Owner { get; }
        public ProgressCounter Progress { get; }

        public bool Verified { get => _verified; set => _verified = value; }
        private volatile bool _verified = false;

        public bool SupportTimestamp { get => _supportTimestamp; set => _supportTimestamp = value; }
        private volatile bool _supportTimestamp = false;

        public string DisplayName { get => _displayName; set => _displayName = value; }
        private volatile string _displayName = "Unknown";

        private readonly Lazy<PairingSupport> pairingSupport;
        private readonly Lazy<FileSender> fileSender;
        private readonly NetMessageHandler messageHandler;
        private readonly ProgressChecker progressChecker;
        private readonly ConnectionChecker connectionChecker;

        public SyncerConnection(SyncerPeer owner, INetConnection connection)
        {
            Owner = owner;
            Progress = new ProgressCounter();

            pairingSupport = new Lazy<PairingSupport>();
            fileSender = new Lazy<FileSender>(() => new FileSender(this));
            messageHandler = new NetMessageHandler(connection, MessageReceived, Disconnect);
            progressChecker = new ProgressChecker(Progress, ProgressTimeout, Owner.Settings.ProgressTimeout);
            connectionChecker = new ConnectionChecker(messageHandler.Send, Owner.Settings.PingInterval);
        }

        public void Start()
        {
            VerificationDataMessage.Send(this);

            if (!disposedValue)
            {
                messageHandler.Start();
            }
        }

        public Guid GetRemoteDeviceId()
        {
            return messageHandler.Connection.Id;
        }

        public void Disconnect()
        {
            Owner.Connections.RemoveConnection(GetRemoteDeviceId());
        }

        public bool IsSyncing()
        {
            if (Progress.Indeterminate) return true;
            if (Progress.Current != Progress.Maximum) return true;
            if (fileSender.IsValueCreated && !fileSender.Value.IsSendQueueEmpty()) return true;

            return false;
        }

        public void Pair()
        {
            if (!Verified)
            {
                Log.Verbose("Sending pairing request");
                pairingSupport.Value.Accept = true;
                Send(new PairingRequestMessage(Owner.Settings.DisplayName));
            }
        }

        public void Send(NetMessage message)
        {
            messageHandler.Send(message);
        }

        public void SendAndDisconnect(NetMessage message)
        {
            messageHandler.SendAndDisconnect(message);
        }

        public Lazy<PairingSupport> GetPairingSupport()
        {
            return pairingSupport;
        }

        internal void OnChunkRequest()
        {
            if (fileSender.IsValueCreated) fileSender.Value.ChunkRequest();
            else Log.Warning($"{DisplayName}: FileChunkRequest before fileSender Init");
        }

        internal void OnPairingRequestCallback(bool accepted)
        {
            if (accepted)
            {
                DateTime now = DateTime.Now;
                pairingSupport.Value.GenTime = now;
                Guid rawKey = Guid.NewGuid();
                AddDeviceKey(GetRemoteDeviceId(), rawKey);
                Send(PairingResponseMessage.Accept(rawKey, now));
                VerificationDataMessage.Send(this);
            }
            else
            {
                Send(PairingResponseMessage.Refuse());
            }
        }

        internal void AddDeviceKey(Guid deviceId, Guid verificationKey)
        {
            Owner.DeviceKeys.Add(deviceId, verificationKey);
            Owner.OnNewPairAdded(new NewPairAddedEventArgs(deviceId));
        }

        internal void StartProfileSync(SyncProfileState remoteProfile)
        {
            bool exists = Owner.Profiles.GetProfile(remoteProfile.Id, out var localProfile);

            if (!exists || !localProfile.AllowSend || localProfile.Key != remoteProfile.Key)
            {
                Log.Info("Tried to sync invalid profile: " + remoteProfile.Id);
                return;
            }

            if (!localProfile.GenerateState(out FileSystemItem[] state))
            {
                Log.Warning($"Failed to get profile state: PID:{remoteProfile.Id}");
                return;
            }

            Log.Info("Processing Sync Profile: " + remoteProfile.Id);
            /*
            string[] directories = null;
            IList<string> filesRelative = null;
            IList<string> filesAbsolute = null;

            
            // Plugin
            bool usedPlugin = false;
            if (!string.IsNullOrEmpty(localProfile.Plugin))
            {
                if (Owner.Plugins.TryGet(localProfile.Plugin, out Plugin plugin))
                {
                    if (plugin.Features.HasFlag(PluginFeatures.ModifySendData))
                    {
                        usedPlugin = true;

                        Plugin.MSD_IN dataIn = new Plugin.MSD_IN()
                        {
                            Connection = this,
                            Profile = localProfile,
                            LocalState = state,
                            RemoteState = remoteProfile.State,
                        };

                        Plugin.MSD_OUT values = await plugin.ModifySendDataAsync(dataIn);

                        directories = values.Directories;
                        filesRelative = values.Files;
                        filesAbsolute = values.Redirects;
                    }
                }
                else
                {
                    Log.Warning($"Plugin '{localProfile.Plugin}' for profile '{remoteProfile.Id}' is unavailable. Skipping profile...");
                    Owner.OnProfileError(new ProfileErrorEventArgs(remoteProfile.Id, localProfile, SyncProfileError.PluginUnavailable));
                    return;
                }
            }

            // If no plugin was used, execute default logic
            if (!usedPlugin)
            {
                DirectoryTreeDifference delta = new DirectoryTreeDifference(state, remoteProfile.State, SupportTimestamp);
                directories = delta.RemoteMissingDirectories.ToArray();
                filesRelative = delta.RemoteMissingFiles;
            }
            */

            var delta = new DirectoryTreeDifference(state, remoteProfile.State, SupportTimestamp);

            if (remoteProfile.AllowDelete)
            {
                // Delete
                Send(new DeleteFilesMessage(remoteProfile.Id, localProfile, delta.LocalMissingFiles));
                Send(new DeleteDirectoriesMessage(remoteProfile.Id, localProfile, delta.LocalMissingDirectories));
            }

            // Send
            Send(new CreateDirectoriesMessage(remoteProfile.Id, localProfile, delta.RemoteMissingDirectories));
            fileSender.Value.AddFiles(FileSendInfo.Create(remoteProfile.Id, localProfile, delta.RemoteMissingFiles, null));

            // Update last sync date
            localProfile.UpdateLastSyncDate(Owner.Profiles, remoteProfile.Id);
        }

        private void ProgressTimeout()
        {
            Log.Info($"{DisplayName}: Sync progress timeout. Disconnecting...");
            Disconnect();
        }

        private void MessageReceived(NetMessage message)
        {
            if (CheckMessage(message)) message.Process(this);
            else Log.Warning($"{DisplayName}: Message rejected!");
        }

        private bool CheckMessage(NetMessage message)
        {
            return message != null
                && (Verified
                    || message.MessageType == NetMessageType.VerificationData
                    || message.MessageType == NetMessageType.IsAlive
                    || message.MessageType == NetMessageType.PairingRequest
                    || message.MessageType == NetMessageType.PairingResponse
                );
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    messageHandler.Dispose();
                    progressChecker.Dispose();
                    connectionChecker.Dispose();

                    if (fileSender.IsValueCreated)
                    {
                        fileSender.Value.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
