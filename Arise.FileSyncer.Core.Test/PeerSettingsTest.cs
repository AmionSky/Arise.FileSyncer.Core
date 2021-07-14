using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arise.FileSyncer.Core.Test
{
    [TestClass]
    public class PeerSettingsTest
    {
        private readonly SyncerPeerSettings settings;

        private readonly Guid id1 = Guid.NewGuid();
        private readonly Guid id2 = Guid.NewGuid();
        private readonly Guid id3 = Guid.NewGuid();
        private readonly Guid id4 = Guid.NewGuid();

        public PeerSettingsTest()
        {
            settings = new SyncerPeerSettings();
            settings.Profiles.TryAdd(id1, new SyncProfile());
            settings.Profiles.TryAdd(id2, new SyncProfile());
            settings.Profiles.TryAdd(id3, new SyncProfile());
        }

        [TestMethod]
        public void TestGetProfileIds_Zero()
        {
            SyncerPeerSettings emptySettings = new();
            Assert.AreEqual(0, emptySettings.Profiles.Count);
        }

        [TestMethod]
        public void TestGetProfileIds_Multiple()
        {
            var ids = settings.Profiles.Keys;
            Assert.AreEqual(3, ids.Count);

            Assert.IsTrue(ids.Contains(id1));
            Assert.IsTrue(ids.Contains(id2));
            Assert.IsTrue(ids.Contains(id3));
        }

        [TestMethod]
        public void TestGetProfile_Existing()
        {
            Assert.IsTrue(settings.Profiles.TryGetValue(id2, out var profile));
            Assert.IsNotNull(profile);
        }

        [TestMethod]
        public void TestGetProfile_NonExisting()
        {
            Assert.IsFalse(settings.Profiles.TryGetValue(id4, out var profile));
            Assert.IsNull(profile);
        }

        [TestMethod]
        public void TestVerify_Ok()
        {
            var settings = new SyncerPeerSettings(Guid.NewGuid(), "Test");
            Assert.IsTrue(settings.Verify());
        }

        [TestMethod]
        public void TestVerify_Fail()
        {
            var settings = new SyncerPeerSettings(Guid.Empty, "Test");
            Assert.IsFalse(settings.Verify());
        }
    }
}
