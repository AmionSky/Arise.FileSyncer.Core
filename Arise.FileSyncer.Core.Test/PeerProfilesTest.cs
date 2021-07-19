using System;
using Arise.FileSyncer.Core.Peer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arise.FileSyncer.Core.Test
{
    [TestClass]
    public class PeerProfilesTest
    {
        private readonly PeerProfiles profiles;

        private readonly Guid id1 = Guid.NewGuid();
        private readonly Guid id2 = Guid.NewGuid();
        private readonly Guid id3 = Guid.NewGuid();
        private readonly Guid id4 = Guid.NewGuid();

        public PeerProfilesTest()
        {
            profiles = new();
            profiles.TryAdd(id1, new SyncProfile());
            profiles.TryAdd(id2, new SyncProfile());
            profiles.TryAdd(id3, new SyncProfile());
        }

        [TestMethod]
        public void TestGetProfileIds_Zero()
        {
            PeerProfiles emtpyProfiles = new();
            Assert.AreEqual(0, emtpyProfiles.Count);
        }

        [TestMethod]
        public void TestGetProfileIds_Multiple()
        {
            Assert.AreEqual(3, profiles.Count);

            var ids = profiles.Ids;
            Assert.AreEqual(3, ids.Count);

            Assert.IsTrue(ids.Contains(id1));
            Assert.IsTrue(ids.Contains(id2));
            Assert.IsTrue(ids.Contains(id3));
        }

        [TestMethod]
        public void TestGetProfile_Existing()
        {
            Assert.IsTrue(profiles.TryGetProfile(id2, out var profile));
            Assert.IsNotNull(profile);
        }

        [TestMethod]
        public void TestGetProfile_NonExisting()
        {
            Assert.IsFalse(profiles.TryGetProfile(id4, out var profile));
            Assert.IsNull(profile);
        }
    }
}
