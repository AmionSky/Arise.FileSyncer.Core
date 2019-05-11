using System;
using System.Collections.Concurrent;

namespace Arise.FileSyncer.Core
{
    public class SyncerPeerSettings
    {
        /// <summary>
        /// Id of the local device.
        /// </summary>
        public Guid DeviceId { get; }

        /// <summary>
        /// Readable name of the local device.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Paired devices. (Remote Device Id, Verification Key)
        /// </summary>
        public ConcurrentDictionary<Guid, Guid> DeviceKeys { get; }

        /// <summary>
        /// Saved sync profiles. (Profile ID, Profile Data)
        /// </summary>
        public ConcurrentDictionary<Guid, SyncProfile> Profiles { get; }

        /// <summary>
        /// Size of the file send buffer.
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// The maximum number of file chunks in the builder queue.
        /// </summary>
        public int ChunkRequestCount { get; set; }

        /// <summary>
        /// The time in milliseconds the connection should check if progress was made.
        /// </summary>
        public int ProgressTimeout { get; set; }

        /// <summary>
        /// The time in milliseconds the connection should ping the remote if it's alive.
        /// </summary>
        public int PingInterval { get; set; }

        /// <summary>
        /// Creates a new instance with generic default values.
        /// </summary>
        public SyncerPeerSettings() : this(Guid.Empty, string.Empty) { }

        public SyncerPeerSettings(Guid deviceId, string displayName)
        {
            DeviceId = deviceId;
            DisplayName = displayName;
            DeviceKeys = new ConcurrentDictionary<Guid, Guid>(1, 0);
            Profiles = new ConcurrentDictionary<Guid, SyncProfile>(2, 0);
            BufferSize = 4096;
            ChunkRequestCount = 16;
            ProgressTimeout = 61000;
            PingInterval = 61000;
        }
    }
}
