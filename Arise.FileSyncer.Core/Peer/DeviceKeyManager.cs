using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Arise.FileSyncer.Core.Peer
{
    public class DeviceKeyManager
    {
        // (Remote Device Id, Verification Key)
        private readonly ConcurrentDictionary<Guid, Guid> deviceKeys;

        public DeviceKeyManager()
        {
            deviceKeys = new ConcurrentDictionary<Guid, Guid>();
        }

        public DeviceKeyManager(KeyValuePair<Guid, Guid>[] snapshot)
        {
            deviceKeys = new ConcurrentDictionary<Guid, Guid>(snapshot);
        }

        /// <summary>
        /// Gets the verification key for the specified device
        /// </summary>
        /// <param name="deviceId">Remote device ID</param>
        public bool GetVerificationKey(Guid deviceId, out Guid verificationKey)
        {
            return deviceKeys.TryGetValue(deviceId, out verificationKey);
        }

        /// <summary>
        /// Add a new or update a device verification key
        /// </summary>
        /// <param name="deviceId">Remote device ID</param>
        /// <param name="verificationKey">Verification key</param>
        public void Add(Guid deviceId, Guid verificationKey)
        {
            deviceKeys.AddOrUpdate(deviceId, verificationKey, (k, v) => verificationKey);
        }

        /// <summary>
        /// Is the specified device ID registered
        /// </summary>
        public bool ContainsId(Guid deviceId)
        {
            return deviceKeys.ContainsKey(deviceId);
        }

        /// <summary>
        /// Snapshot of the current keys and values
        /// </summary>
        public KeyValuePair<Guid,Guid>[] Snapshot()
        {
            return deviceKeys.ToArray();
        }
    }
}
