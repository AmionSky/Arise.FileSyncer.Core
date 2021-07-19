using System;
using System.IO;
using System.Threading;
using Arise.FileSyncer.Core.FileSync;
using Arise.FileSyncer.Core.Peer;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core
{
    public class SyncProfile : IBinarySerializable
    {
        /// <summary>
        /// Profile verification key
        /// </summary>
        public Guid Key { get => key; init => key = value; }
        /// <summary>
        /// User given name of the profile
        /// </summary>
        public string Name { get => name; init => name = value; }
        /// <summary>
        /// Is syncing for this profile enabled
        /// </summary>
        public bool Activated { get => activated; init => activated = value; }
        /// <summary>
        /// Allows sending files from this device
        /// </summary>
        public bool AllowSend { get => allowSend; init => allowSend = value; }
        /// <summary>
        /// Allows receiving files from remote devices
        /// </summary>
        public bool AllowReceive { get => allowReceive; init => allowReceive = value; }
        /// <summary>
        /// Allows deletion requests from remote devices
        /// </summary>
        public bool AllowDelete { get => allowDelete; init => allowDelete = value; }
        /// <summary>
        /// Last time the profile had a successful syncing operation
        /// </summary>
        public DateTime LastSyncDate
        {
            get => new(Interlocked.Read(ref lastSyncDate));
            private set => Interlocked.Exchange(ref lastSyncDate, value.Ticks);
        }
        /// <summary>
        /// The date and time the profile was created
        /// </summary>
        public DateTime CreationDate { get => creationDate; init => creationDate = value; }
        /// <summary>
        /// Should skip syncing hidden files
        /// </summary>
        public bool SkipHidden { get => skipHidden; init => skipHidden = value; }
        /// <summary>
        /// The base directory of the files to sync
        /// </summary>
        public string RootDirectory { get => rootDirectory; init => rootDirectory = value; }
        /// <summary>
        /// (Unused) Sync plugin to use
        /// </summary>
        public string Plugin { get => plugin; init => plugin = value; }

        private Guid key;
        private string name;
        private bool activated;
        private bool allowSend;
        private bool allowReceive;
        private bool allowDelete;
        private long lastSyncDate = 0;
        private DateTime creationDate;
        private bool skipHidden;
        private string rootDirectory;
        private string plugin;

        public SyncProfile()
        {
            Key = Guid.Empty;
            Name = string.Empty;
            Activated = true;
            AllowSend = false;
            AllowReceive = false;
            AllowDelete = false;
            LastSyncDate = DateTime.Now;
            CreationDate = DateTime.Now;
            SkipHidden = true;
            RootDirectory = string.Empty;
            Plugin = string.Empty;
        }

        public SyncProfile(SyncProfile profile)
        {
            Key = profile.Key;
            Name = profile.Name;
            Activated = profile.Activated;
            AllowSend = profile.AllowSend;
            AllowReceive = profile.AllowReceive;
            AllowDelete = profile.AllowDelete;
            LastSyncDate = profile.LastSyncDate;
            CreationDate = profile.CreationDate;
            SkipHidden = profile.SkipHidden;
            RootDirectory = profile.RootDirectory;
            Plugin = profile.Plugin;
        }

        /// <summary>
        /// Update the LastSyncDate to 'Now' and call event
        /// </summary>
        internal void UpdateLastSyncDate(PeerProfiles peerProfiles, Guid id)
        {
            LastSyncDate = DateTime.Now;
            peerProfiles.OnProfileChanged(id, this);
        }

        /// <summary>
        /// Generate file-system state for sync
        /// </summary>
        internal bool GenerateState(out FileSystemItem[] state)
        {
            return DirectoryTreeQuery.GenerateTree(out state, RootDirectory, SkipHidden, FileBuilder.TemporaryFileExtension);
        }

        public void Deserialize(Stream stream)
        {
            key = stream.ReadGuid();
            name = stream.ReadString();
            activated = stream.ReadBoolean();
            allowSend = stream.ReadBoolean();
            allowReceive = stream.ReadBoolean();
            allowDelete = stream.ReadBoolean();
            lastSyncDate = stream.ReadDateTime().Ticks;
            creationDate = stream.ReadDateTime();
            skipHidden = stream.ReadBoolean();
            rootDirectory = stream.ReadString();
            plugin = stream.ReadString();
        }

        public void Serialize(Stream stream)
        {
            stream.WriteAFS(Key);
            stream.WriteAFS(Name);
            stream.WriteAFS(Activated);
            stream.WriteAFS(AllowSend);
            stream.WriteAFS(AllowReceive);
            stream.WriteAFS(AllowDelete);
            stream.WriteAFS(LastSyncDate);
            stream.WriteAFS(CreationDate);
            stream.WriteAFS(SkipHidden);
            stream.WriteAFS(RootDirectory);
            stream.WriteAFS(Plugin);
        }
    }
}
