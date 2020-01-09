using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LightConversion.Software.Settings.Tests {
    [TestClass]
    public class ReliableFileTests {
        [TestMethod]
        public void TestBasicRead() {
            CreateCleanTempFolder();

            File.WriteAllText("temp/someFile.txt", "some text");

            var reliableFile = new ReliableFile("temp/someFile.txt");
            reliableFile.Initialize();

            var isOk = reliableFile.TryReadAllText(out var fileContent);
            Assert.IsTrue(isOk);
            Assert.AreEqual("some text", fileContent);
        }

        [TestMethod]
        public void TestBasicWrite() {
            CreateCleanTempFolder();

            var reliableFile = new ReliableFile("temp/someFile.txt");
            reliableFile.Initialize();

            var isOk = reliableFile.TryWriteAllText("some text");
            Assert.IsTrue(isOk);

            var actualContents = File.ReadAllText("temp/someFile.txt");
            Assert.AreEqual("some text", actualContents);
        }


        [TestMethod]
        public void TestReadingNonExistingFile() {
            CreateCleanTempFolder();

            var rf = new ReliableFile("temp/someFile.txt");
            rf.Initialize();

            var isOk = rf.TryReadAllText(out var fileContent);
            Assert.IsFalse(isOk, "File doesn't exist, must return false");
        }


        [TestMethod]
        public void TestUnrecoverableFileFromRf1() {
            CreateCleanTempFolder();

            // Simulating state when system crashed after first write to rf1.
            File.WriteAllText("temp/someFile.txt.rf1", "some text");

            var reliableFile = new ReliableFile("temp/someFile.txt");
            reliableFile.Initialize();

            var isOk = reliableFile.TryReadAllText(out var fileContent);
            Assert.IsFalse(isOk, "No recovery must be done from .rf1, must return false");
        }

        [TestMethod]
        public void TestFallbackRecoveryRf1() {
            CreateCleanTempFolder();

            // Simulating state when system crashed after second write to rf1. So rf1 contents must be lost, but original file must still be readable.
            File.WriteAllText("temp/someFile.txt", "some text");
            File.WriteAllText("temp/someFile.txt.rf1", "new text");

            var reliableFile = new ReliableFile("temp/someFile.txt");
            reliableFile.Initialize();

            var isOk = reliableFile.TryReadAllText(out var fileContent);
            Assert.IsTrue(isOk, "Original file must be read");
            Assert.AreEqual("some text", fileContent, "Original file must be read");

            var actualFileContents = File.ReadAllText("temp/someFile.txt");
            Assert.AreEqual("some text", actualFileContents, "Original file must be unchanged");
        }

        [TestMethod]
        public void TestFallbackRecoveryRf2() {
            CreateCleanTempFolder();

            // Simulating state when system crashed after second write to rf2.
            File.WriteAllText("temp/someFile.txt", "some text");
            File.WriteAllText("temp/someFile.txt.rf2", "new text");

            var reliableFile = new ReliableFile("temp/someFile.txt");
            reliableFile.Initialize();

            var isOk = reliableFile.TryReadAllText(out var fileContent);
            Assert.IsTrue(isOk, "Backup from rf2 must be made");
            Assert.AreEqual("new text", fileContent, "Backup from rf2 must be made");

            var actualFileContents = File.ReadAllText("temp/someFile.txt");
            Assert.AreEqual("new text", actualFileContents, "Backup from rf2 must be made");
        }


        [TestMethod]
        public void TestRecoverableFileFromRf2() {
            CreateCleanTempFolder();

            // Simulating state when system crashed after successfully writing to rf1 and moving it to rf2. 
            File.WriteAllText("temp/someFile.txt.rf2", "some text");

            var reliableFile = new ReliableFile("temp/someFile.txt");
            reliableFile.Initialize();

            var isOk = reliableFile.TryReadAllText(out var fileContent);
            Assert.IsTrue(isOk);
            Assert.AreEqual("some text", fileContent);
        }

        [TestMethod]
        public void TestIfRf1IsCleanedUp() {
            CreateCleanTempFolder();

            // Simulating state when system crashed after first write to rf1.
            File.WriteAllText("temp/someFile.txt.rf1", "some text");

            var reliableFile = new ReliableFile("temp/someFile.txt");
            reliableFile.Initialize();

            var isRf1StillPresent = File.Exists("temp/someFile.rf1");
            Assert.IsFalse(isRf1StillPresent);
        }

        [TestMethod]
        public void TestIfRf2IsCleanedUp() {
            CreateCleanTempFolder();

            // Simulating state when system crashed after successfully writing to rf1 and moving it to rf2. 
            File.WriteAllText("temp/someFile.txt.rf2", "some text");

            var reliableFile = new ReliableFile("temp/someFile.txt");
            reliableFile.Initialize();

            var isRf2StillPresent = File.Exists("temp/someFile.txt.rf2");
            Assert.IsFalse(isRf2StillPresent);
        }


        private void CreateCleanTempFolder() {
            if (Directory.Exists("temp")) {
                Directory.Delete("temp", true);
            }

            Directory.CreateDirectory("temp");
        }
    }
}