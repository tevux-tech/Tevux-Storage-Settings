using System;
using System.IO;
using System.Threading.Tasks;
using LightConversion.Storage.Settings;
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

        [TestMethod]
        public void TestAlreadyOpenFile() {
            CreateCleanTempFolder();

            // Simulating opened file by other program.
            using (var openFileStream = File.Open("temp/someFile.txt", FileMode.OpenOrCreate)) {
                var reliableFile = new ReliableFile("temp/someFile.txt");
                reliableFile.Initialize();

                try {
                    var isOk = reliableFile.TryReadAllText(out var fileContent);
                    Assert.IsFalse(isOk, "Must fail because file isn't accessible.");

                    isOk = reliableFile.TryWriteAllText("Some text");
                    Assert.IsFalse(isOk, "Must fail because file isn't accessible.");
                } catch (Exception ex) {
                    Assert.Fail("No exception should be thrown", ex);
                }
            }
        }

        #region Changed event tests.

        [TestMethod]
        public async Task TestChangedEvenInternalWrite() {
            CreateCleanTempFolder();

            var reliableFile = new ReliableFile("temp/someFile.txt");
            reliableFile.Initialize();

            var changedCounter = 0;
            reliableFile.Changed += (sender, args) => {
                changedCounter += 1;
            };

            reliableFile.TryWriteAllText("Changing file content.");

            // We need to release main thread and wait for changed event.
            await Task.Delay(10);

            Assert.AreEqual(1, changedCounter);
        }

        [TestMethod]
        public async Task TestChangedEvenWithoutFolder() {
            CreateCleanTempFolder();

            var reliableFile = new ReliableFile("someFile.txt");
            reliableFile.Initialize();

            var changedCounter = 0;
            reliableFile.Changed += (sender, args) => {
                changedCounter += 1;
            };

            reliableFile.TryWriteAllText("Changing file content.");

            // We need to release main thread and wait for changed event.
            await Task.Delay(10);

            Assert.AreEqual(1, changedCounter);
        }

        [TestMethod]
        public async Task TestChangedEventLastFileContent() {
            CreateCleanTempFolder();

            var reliableFile = new ReliableFile("temp/someFile.txt");
            reliableFile.Initialize();

            var lastFileContent = "";
            reliableFile.Changed += (sender, args) => {
                reliableFile.TryReadAllText(out lastFileContent);
            };

            reliableFile.TryWriteAllText("Changing file content.");

            // We need to release main thread and wait for changed event.
            await Task.Delay(10);
            Assert.AreEqual("Changing file content.", lastFileContent);
            
            reliableFile.TryWriteAllText("Changing 2nd time.");

            await Task.Delay(10);
            Assert.AreEqual("Changing 2nd time.", lastFileContent);

            reliableFile.TryWriteAllText("Changing 3rd time.");

            await Task.Delay(10);
            Assert.AreEqual("Changing 3rd time.", lastFileContent);

            reliableFile.TryWriteAllText("Changing 4th time.");

            await Task.Delay(10);
            Assert.AreEqual("Changing 4th time.", lastFileContent);

            reliableFile.TryWriteAllText("Changing 5th time.");

            await Task.Delay(10);
            Assert.AreEqual("Changing 5th time.", lastFileContent);
        }

        [TestMethod]
        public async Task TestChangedEventExternalWrite() {
            CreateCleanTempFolder();

            var reliableFile = new ReliableFile("temp/someFile.txt");
            reliableFile.Initialize();

            var changedCounter = 0;
            reliableFile.Changed += (sender, args) => {
                changedCounter += 1;
            };

            File.AppendAllText("temp/someFile.txt", "External file edit.");

            // We need to release main thread and wait for changed event.
            await Task.Delay(10);

            Assert.AreEqual(1, changedCounter);
        }

        [TestMethod]
        public async Task TestChangedEventRecreate() {
            CreateCleanTempFolder();

            File.WriteAllText("temp/someFile.txt", "Creating file.");

            var reliableFile = new ReliableFile("temp/someFile.txt");
            reliableFile.Initialize();

            var changedCounter = 0;
            reliableFile.Changed += (sender, args) => {
                changedCounter += 1;
            };

            File.Delete("temp/someFile.txt");
            File.WriteAllText("temp/someFile.txt", "Created externally.");

            // We need to release main thread and wait for changed event.
            await Task.Delay(10);

            Assert.AreEqual(1, changedCounter);
        }

        [TestMethod]
        public async Task TestChangedEventMove() {
            CreateCleanTempFolder();

            File.WriteAllText("temp/someFile.txt", "Creating file.");
            var reliableFile = new ReliableFile("temp/someFile.txt");
            reliableFile.Initialize();

            var changedCounter = 0;
            reliableFile.Changed += (sender, args) => {
                changedCounter += 1;
            };

            // Create second temp folder to move to.
            Directory.CreateDirectory("temp2");

            // Moving file this should generate single changed event.
            File.Move("temp/someFile.txt", "temp2/someFile.txt");
            File.Move("temp2/someFile.txt", "temp/someFile.txt");

            // We need to release main thread and wait for changed event.
            await Task.Delay(10);

            Assert.AreEqual(1, changedCounter);

            // Remove temp folder so no trash is left.
            Directory.Delete("temp2", true);
        }

        [TestMethod]
        public async Task TestChangedEventExternalDelete() {
            CreateCleanTempFolder();

            var reliableFile = new ReliableFile("temp/someFile.txt");
            reliableFile.Initialize();

            var changedCounter = 0;
            reliableFile.Changed += (sender, args) => {
                changedCounter += 1;
            };

            File.Delete("temp/someFile.txt");

            // We need to release main thread and wait for changed event.
            await Task.Delay(10);

            // Delete should not rise any changed event.
            Assert.AreEqual(0, changedCounter);
        }

        #endregion


        private void CreateCleanTempFolder() {
            if (Directory.Exists("temp")) {
                Directory.Delete("temp", true);
            }

            Directory.CreateDirectory("temp");
        }
    }
}