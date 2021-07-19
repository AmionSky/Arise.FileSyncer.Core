using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Arise.FileSyncer.Core.Peer
{
    public class ConnectionManager : IDisposable
    {
        /// <summary>
        /// Called when a new connection is successfully added.
        /// </summary>
        public event EventHandler<ConnectionEventArgs> ConnectionAdded;
        /// <summary>
        /// Called when a connection is successfully removed.
        /// </summary>
        public event EventHandler<ConnectionEventArgs> ConnectionRemoved;
        /// <summary>
        /// Called when a connection got verified and we know basic information about the remote device.
        /// </summary>
        public event EventHandler<ConnectionVerifiedEventArgs> ConnectionVerified;

        private readonly ConcurrentDictionary<Guid, SyncerConnection> connections;

        public ConnectionManager()
        {
            connections = new();
        }

        /// <summary>
        /// Adds a new connection.
        /// </summary>
        public bool AddConnection(SyncerPeer peer, INetConnection connection)
        {
            if (connection == null) return false;

            SyncerConnection syncerConnection = new(peer, connection);
            bool added = connections.TryAdd(connection.Id, syncerConnection);

            if (added)
            {
                OnConnectionAdded(connection.Id);
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
        public bool RemoveConnection(Guid id)
        {
            bool removed = connections.TryRemove(id, out SyncerConnection syncerConnection);

            if (removed)
            {
                syncerConnection.Dispose();
                OnConnectionRemoved(id);
            }

            return removed;
        }

        /// <summary>
        /// Returns an array of the connection IDs.
        /// </summary>
        /// <returns>IDs of the connections</returns>
        public ICollection<Guid> GetConnectionIds()
        {
            return connections.Keys;
        }

        /// <summary>
        /// Returns an array of the connections
        /// </summary>
        internal ICollection<SyncerConnection> GetConnections()
        {
            return connections.Values;
        }

        /// <summary>
        /// Returns the number of connections.
        /// </summary>
        /// <returns>Number of connections</returns>
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
        /// Tries getting a connection.
        /// </summary>
        /// <param name="id">ID of the connection</param>
        /// <param name="connection">The connection as a public interface</param>
        public bool TryGetConnection(Guid id, out ISyncerConnection connection)
        {
            bool success = connections.TryGetValue(id, out SyncerConnection fullConnection);
            connection = fullConnection;
            return success;
        }

        private void OnConnectionAdded(Guid connectionId)
        {
            ConnectionAdded?.Invoke(this, new ConnectionEventArgs() { Id = connectionId });
        }

        private void OnConnectionRemoved(Guid connectionId)
        {
            ConnectionRemoved?.Invoke(this, new ConnectionEventArgs() { Id = connectionId });
        }

        internal void OnConnectionVerified(Guid connectionId, string displayName)
        {
            ConnectionVerified?.Invoke(this, new ConnectionVerifiedEventArgs()
            {
                Id = connectionId,
                Name = displayName,
            });
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
                    foreach (KeyValuePair<Guid, SyncerConnection> conKV in connections)
                    {
                        conKV.Value.Dispose();
                    }

                    connections.Clear();
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
