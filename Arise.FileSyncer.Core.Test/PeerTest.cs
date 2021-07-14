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
            using SyncerPeer peer = new(CreateSettings());
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId2)));
            Assert.IsFalse(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.IsFalse(peer.AddConnection(null));
        }

        [TestMethod]
        public void TestRemoveConnection()
        {
            using SyncerPeer peer = new(CreateSettings());
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.IsTrue(peer.RemoveConnection(dummyId1));
            Assert.IsFalse(peer.RemoveConnection(dummyId2));
        }

        [TestMethod]
        public void TestGetConnectionIds()
        {
            using SyncerPeer peer = new(CreateSettings());
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId2)));

            var ids = peer.GetConnectionIds();
            Assert.AreEqual(2, ids.Count);

            Assert.IsTrue(ids.Contains(dummyId1));
            Assert.IsTrue(ids.Contains(dummyId2));
        }

        [TestMethod]
        public void TestGetConnectionCount()
        {
            using SyncerPeer peer = new(CreateSettings());
            Assert.AreEqual(0, peer.GetConnectionCount());
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.AreEqual(1, peer.GetConnectionCount());
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId2)));
            Assert.AreEqual(2, peer.GetConnectionCount());
        }

        [TestMethod]
        public void TestDoesConnectionExist()
        {
            using SyncerPeer peer = new(CreateSettings());
            Assert.IsFalse(peer.DoesConnectionExist(dummyId1));
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.IsTrue(peer.DoesConnectionExist(dummyId1));
        }

        [TestMethod]
        public void TestTryGetConnection_Success()
        {
            using SyncerPeer peer = new(CreateSettings());
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));

            bool result = peer.TryGetConnection(dummyId1, out ISyncerConnection connection);
            Assert.IsTrue(result);
            Assert.IsNotNull(connection);
        }

        [TestMethod]
        public void TestTryGetConnection_NonExisting()
        {
            using SyncerPeer peer = new(CreateSettings());
            bool result = peer.TryGetConnection(dummyId1, out ISyncerConnection connection);
            Assert.IsFalse(result);
            Assert.IsNull(connection);
        }

        [TestMethod]
        public void TestShareProfile_Success()
        {
            using SyncerPeer peer = new(CreateSettings());
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            peer.AddProfile(dummyId1, new SyncProfile());
            Assert.IsTrue(peer.ShareProfile(dummyId1, dummyId1));
        }

        [TestMethod]
        public void TestShareProfile_NonExisting()
        {
            using SyncerPeer peer = new(CreateSettings());
            peer.AddProfile(dummyId1, new SyncProfile());
            Assert.IsFalse(peer.ShareProfile(dummyId1, dummyId1));
        }

        [TestMethod]
        public void TestShareProfile_NullProfile()
        {
            using SyncerPeer peer = new(CreateSettings());
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.IsFalse(peer.ShareProfile(dummyId1, dummyId1));
        }

        [TestMethod]
        public void TestSyncProfile_Success()
        {
            using SyncerPeer peer = new(CreateSettings());
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            peer.AddProfile(dummyId1, new SyncProfile.Creator() { AllowSend = true, RootDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar });
            Assert.IsTrue(peer.SyncProfile(dummyId1, dummyId1));
        }

        [TestMethod]
        public void TestSyncProfile_NonExisting()
        {
            using SyncerPeer peer = new(CreateSettings());
            peer.AddProfile(dummyId1, new SyncProfile.Creator() { RootDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar });
            Assert.IsFalse(peer.SyncProfile(dummyId1, dummyId1));
        }

        [TestMethod]
        public void TestSyncProfile_NullProfile()
        {
            using SyncerPeer peer = new(CreateSettings());
            Assert.IsTrue(peer.AddConnection(new DummyConnection(dummyId1)));
            Assert.IsFalse(peer.SyncProfile(dummyId1, dummyId1));
        }

        [TestMethod]
        public void TestAddProfile()
        {
            using SyncerPeer peer = new(CreateSettings());
            peer.AddProfile(dummyId1, new SyncProfile());

            Assert.IsTrue(peer.Settings.Profiles.ContainsKey(dummyId1));
            Assert.IsFalse(peer.Settings.Profiles.ContainsKey(dummyId2));
        }

        [TestMethod]
        public void TestRemoveProfile()
        {
            using SyncerPeer peer = new(CreateSettings());
            peer.AddProfile(dummyId1, new SyncProfile());

            Assert.IsTrue(peer.RemoveProfile(dummyId1));
            Assert.IsFalse(peer.RemoveProfile(dummyId1));
            Assert.IsFalse(peer.RemoveProfile(dummyId2));
        }

        private SyncerPeerSettings CreateSettings()
        {
            var settings = new SyncerPeerSettings();

            // Add dummy IDs to paired devices to don't auto-disconnect
            settings.DeviceKeys.TryAdd(dummyId1, Guid.Empty);
            settings.DeviceKeys.TryAdd(dummyId2, Guid.Empty);

            return settings;
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
