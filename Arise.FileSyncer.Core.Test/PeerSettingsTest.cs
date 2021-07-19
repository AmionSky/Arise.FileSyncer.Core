using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arise.FileSyncer.Core.Test
{
    [TestClass]
    public class PeerSettingsTest
    {
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
