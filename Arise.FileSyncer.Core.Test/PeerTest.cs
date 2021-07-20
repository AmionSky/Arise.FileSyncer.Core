using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arise.FileSyncer.Core.Test
{
    [TestClass]
    public class PeerTest
    {
        private readonly Guid dummyId1 = Guid.NewGuid();
        private readonly Guid dummyId2 = Guid.NewGuid();

        [TestMethod]
        public void TestAddConnection()
        {
            using SyncerPeer peer = CreatePeer();
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId2)));
            Assert.IsFalse(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.IsFalse(peer.AddConnection(null));
        }

        [TestMethod]
        public void TestRemoveConnection()
        {
            using SyncerPeer peer = CreatePeer();
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.IsTrue(peer.Connections.RemoveConnection(dummyId1));
            Assert.IsFalse(peer.Connections.RemoveConnection(dummyId2));
        }

        [TestMethod]
        public void TestGetConnectionIds()
        {
            using SyncerPeer peer = CreatePeer();
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId2)));

            var ids = peer.Connections.GetConnectionIds();
            Assert.AreEqual(2, ids.Count);

            Assert.IsTrue(ids.Contains(dummyId1));
            Assert.IsTrue(ids.Contains(dummyId2));
        }

        [TestMethod]
        public void TestGetConnectionCount()
        {
            using SyncerPeer peer = CreatePeer();
            Assert.AreEqual(0, peer.Connections.GetConnectionCount());
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.AreEqual(1, peer.Connections.GetConnectionCount());
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId2)));
            Assert.AreEqual(2, peer.Connections.GetConnectionCount());
        }

        [TestMethod]
        public void TestDoesConnectionExist()
        {
            using SyncerPeer peer = CreatePeer();
            Assert.IsFalse(peer.Connections.DoesConnectionExist(dummyId1));
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.IsTrue(peer.Connections.DoesConnectionExist(dummyId1));
        }

        [TestMethod]
        public void TestTryGetConnection_Success()
        {
            using SyncerPeer peer = CreatePeer();
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));

            bool result = peer.Connections.TryGetConnection(dummyId1, out ISyncerConnection connection);
            Assert.IsTrue(result);
            Assert.IsNotNull(connection);
        }

        [TestMethod]
        public void TestTryGetConnection_NonExisting()
        {
            using SyncerPeer peer = CreatePeer();
            bool result = peer.Connections.TryGetConnection(dummyId1, out ISyncerConnection connection);
            Assert.IsFalse(result);
            Assert.IsNull(connection);
        }

        [TestMethod]
        public void TestShareProfile_Success()
        {
            using SyncerPeer peer = CreatePeer();
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            peer.Profiles.AddProfile(dummyId1, new SyncProfile());
            Assert.IsTrue(peer.ShareProfile(dummyId1, dummyId1));
        }

        [TestMethod]
        public void TestShareProfile_NonExisting()
        {
            using SyncerPeer peer = CreatePeer();
            peer.Profiles.AddProfile(dummyId1, new SyncProfile());
            Assert.IsFalse(peer.ShareProfile(dummyId1, dummyId1));
        }

        [TestMethod]
        public void TestShareProfile_NullProfile()
        {
            using SyncerPeer peer = CreatePeer();
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.IsFalse(peer.ShareProfile(dummyId1, dummyId1));
        }

        [TestMethod]
        public void TestSyncProfile_Success()
        {
            using SyncerPeer peer = CreatePeer();
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            peer.Profiles.AddProfile(dummyId1, new SyncProfile() { AllowSend = true, RootDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar });
            Assert.IsTrue(peer.SyncProfile(dummyId1, dummyId1));
        }

        [TestMethod]
        public void TestSyncProfile_NonExisting()
        {
            using SyncerPeer peer = CreatePeer();
            peer.Profiles.AddProfile(dummyId1, new SyncProfile() { RootDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar });
            Assert.IsFalse(peer.SyncProfile(dummyId1, dummyId1));
        }

        [TestMethod]
        public void TestSyncProfile_NullProfile()
        {
            using SyncerPeer peer = CreatePeer();
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.IsFalse(peer.SyncProfile(dummyId1, dummyId1));
        }

        private SyncerPeer CreatePeer()
        {
            var peer = new SyncerPeer(null, null, null);

            // Add dummy IDs to paired devices to don't auto-disconnect
            peer.DeviceKeys.Add(dummyId1, Guid.Empty);
            peer.DeviceKeys.Add(dummyId2, Guid.Empty);

            return peer;
        }

        private class DummyConnection : INetConnection
        {
            public Guid Id { get; }

            public Stream SenderStream => new DummyStream();
            public Stream ReceiverStream => new DummyStream();

            public DummyConnection(Guid id)
            {
                Id = id;
            }

            public void Dispose() { }
        }

        private class DummyStream : Stream
        {
            public override bool CanRead => true;
            public override bool CanSeek => true;
            public override bool CanWrite => true;
            public override long Length => 0;
            public override long Position { get; set; }

            private readonly ManualResetEvent mre;
            private bool disposed = false;

            public DummyStream()
            {
                Position = 0;
                mre = new ManualResetEvent(false);
            }

            public override void Flush() { }

            public override int Read(byte[] buffer, int offset, int count)
            {
                mre.WaitOne();
                return 0;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                mre.WaitOne();
                return 0;
            }

            public override void SetLength(long value) { }

            public override void Write(byte[] buffer, int offset, int count) { }

            protected override void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    mre.Set();
                    mre.Dispose();

                    disposed = true;
                }
            }
        }
    }
}
