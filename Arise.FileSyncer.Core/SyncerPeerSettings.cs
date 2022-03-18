using System;
using System.IO;
using Arise.FileSyncer.Serializer;

namespace Arise.FileSyncer.Core
{
    public class SyncerPeerSettings : IBinarySerializable
    {
        private Guid deviceId;
        private bool supportTimestamp;

        /// <summary>
        /// Id of the local device.
        /// </summary>
        public Guid DeviceId { get => deviceId; init => deviceId = value; }

        /// <summary>
        /// Readable name of the local device.
        /// </summary>
        public string DisplayName { get; set; }

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
        /// Is the local device supports file timestamp get and set.
        /// </summary>
        public bool SupportTimestamp { get => supportTimestamp; init => supportTimestamp = value; }

        /// <summary>
        /// Creates a new instance with generic default values.
        /// </summary>
        public SyncerPeerSettings() : this(Guid.Empty, string.Empty, true) { }

        public SyncerPeerSettings(Guid deviceId, string displayName, bool supportTimestamp)
        {
            DeviceId = deviceId;
            DisplayName = displayName;
            BufferSize = 4096;
            ChunkRequestCount = 16;
            ProgressTimeout = 61000;
            PingInterval = 61000;
            SupportTimestamp = supportTimestamp;
        }

        /// <summary>
        /// Verifies the values validity
        /// </summary>
        public bool Verify()
        {
            return !(DeviceId == Guid.Empty
                || string.IsNullOrWhiteSpace(DisplayName)
                || BufferSize <= 0
                || ChunkRequestCount <= 0
                || ProgressTimeout <= 0
                || PingInterval <= 0);
        }

        /// <summary>
        /// Fix broken values with a reference settings class
        /// </summary>
        public void Fix(SyncerPeerSettings settings)
        {
            if (DeviceId == Guid.Empty)
                deviceId = settings.DeviceId;

            if (string.IsNullOrWhiteSpace(DisplayName))
                DisplayName = settings.DisplayName;

            if (BufferSize <= 0)
                BufferSize = settings.BufferSize;

            if (ChunkRequestCount <= 0)
                ChunkRequestCount = settings.ChunkRequestCount;

            if (ProgressTimeout <= 0)
                ProgressTimeout = settings.ProgressTimeout;

            if (PingInterval <= 0)
                PingInterval = settings.PingInterval;
        }

        public void Deserialize(Stream stream)
        {
            deviceId = stream.ReadGuid();
            DisplayName = stream.ReadString();
            BufferSize = stream.ReadInt32();
            ChunkRequestCount = stream.ReadInt32();
            ProgressTimeout = stream.ReadInt32();
            PingInterval = stream.ReadInt32();
            supportTimestamp = stream.ReadBoolean();
        }

        public void Serialize(Stream stream)
        {
            stream.WriteAFS(DeviceId);
            stream.WriteAFS(DisplayName);
            stream.WriteAFS(BufferSize);
            stream.WriteAFS(ChunkRequestCount);
            stream.WriteAFS(ProgressTimeout);
            stream.WriteAFS(PingInterval);
            stream.WriteAFS(SupportTimestamp);
        }
    }
}
