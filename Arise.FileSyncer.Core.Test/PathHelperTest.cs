using System;
using System.IO;
using Arise.FileSyncer.Core.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arise.FileSyncer.Core.Test
{
    [TestClass]
    public class PathHelperTest
    {
        [DataTestMethod]
        [DataRow("C:\\Some\\Path", "C:\\Some\\Path", false)]
        [DataRow("Some/Path", "Some\\Path", false)]
        [DataRow("Some\\Path", "Some/Path", false)]
        [DataRow("Some/Path", "Some\\Path\\", true)]
        [DataRow("Some\\Path", "Some/Path/", true)]
        [DataRow("Some\\Path", "Some\\Path\\", true)]
        [DataRow("Some\\Path", "Some\\Path", false)]
        [DataRow("Some/Path", "Some/Path/", true)]
        [DataRow("Some/Path", "Some/Path", false)]
        [DataRow("/Some/Path", "/Some/Path", false)]
        [DataRow("/Some/Path", "/Some/Path/", true)]
        public void GetCorrectTest(string path, string expectedResult, bool isDirectory)
        {
            //Convert to OS specific
            expectedResult = expectedResult.Replace(PathHelper.OtherSeparatorChar, Path.DirectorySeparatorChar);

            string result = PathHelper.GetCorrect(path, isDirectory);

            Assert.AreEqual(expectedResult, result);
        }
    }
}
