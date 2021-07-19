using System;
using Arise.FileSyncer.Core.Peer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arise.FileSyncer.Core.Test
{
    [TestClass]
    public class PeerProfilesTest
    {
        private readonly Guid id1 = Guid.NewGuid();
        private readonly Guid id2 = Guid.NewGuid();
        private readonly Guid id3 = Guid.NewGuid();
        private readonly Guid id4 = Guid.NewGuid();

        [TestMethod]
        public void TestAddProfile()
        {
            ProfileManager localProfiles = new();
            localProfiles.AddProfile(id1, new SyncProfile());

            Assert.IsTrue(localProfiles.Ids.Contains(id1));
            Assert.IsFalse(localProfiles.Ids.Contains(id2));
        }

        [TestMethod]
        public void TestRemoveProfile()
        {
            ProfileManager localProfiles = new();
            localProfiles.AddProfile(id1, new SyncProfile());

            Assert.IsTrue(localProfiles.RemoveProfile(id1));
            Assert.IsFalse(localProfiles.RemoveProfile(id1));
            Assert.IsFalse(localProfiles.RemoveProfile(id2));
        }

        [TestMethod]
        public void TestGetProfileIds_Zero()
        {
            ProfileManager emtpyProfiles = new();
            Assert.AreEqual(0, emtpyProfiles.Count);
        }

        [TestMethod]
        public void TestGetProfileIds_Multiple()
        {
            var profiles = CreateProfiles();
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
            var profiles = CreateProfiles();
            Assert.IsTrue(profiles.GetProfile(id2, out var profile));
            Assert.IsNotNull(profile);
        }

        [TestMethod]
        public void TestGetProfile_NonExisting()
        {
            var profiles = CreateProfiles();
            Assert.IsFalse(profiles.GetProfile(id4, out var profile));
            Assert.IsNull(profile);
        }

        private ProfileManager CreateProfiles()
        {
            ProfileManager profiles = new();
            profiles.AddProfile(id1, new SyncProfile());
            profiles.AddProfile(id2, new SyncProfile());
            profiles.AddProfile(id3, new SyncProfile());
            return profiles;
        }
    }
}
