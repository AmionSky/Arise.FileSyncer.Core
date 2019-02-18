using System.IO;
using Arise.FileSyncer.Helpers;
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

        [DataTestMethod]
        [DataRow("/Just/Some/Path/Test.cst", "/Just/Some", "Path/Test.cst", false)]
        [DataRow("/Just/Some/Path/Test.cst", "/Just/Some/", "Path/Test.cst", false)]
        [DataRow("Just/Some/Path/Test.cst", "Just/Some", "Path/Test.cst", false)]
        [DataRow("Just/Some/Path/Test.cst", "Just/Some/", "Path/Test.cst", false)]
        [DataRow("/Just/Some/Path/", "/Just/Some", "Path/", true)]
        [DataRow("/Just/Some/Path", "/Just/Some", "Path/", true)]
        [DataRow("/Just/Some/Path/", "/Just/Some/", "Path/", true)]
        [DataRow("/Just/Some/Path", "/Just/Some/", "Path/", true)]
        [DataRow("/Just/Some/Path", "/sJust/Some/", "", true)]
        [DataRow("/sJust/Some/Path", "/Just/Some/", "", true)]
        [DataRow("/sJust/Some/Path", "", "", true)]
        [DataRow("", "/Just/Some/", "", true)]
        [DataRow("", "", "", true)]
        public void GetRelativeTest(string fullPath, string rootPath, string expectedResult, bool fullPathIsDir)
        {
            //Convert to OS specific
            expectedResult = expectedResult.Replace(PathHelper.OtherSeparatorChar, Path.DirectorySeparatorChar);

            string result = PathHelper.GetRelative(fullPath, rootPath, fullPathIsDir);

            Assert.AreEqual(expectedResult, result);
        }
    }
}
