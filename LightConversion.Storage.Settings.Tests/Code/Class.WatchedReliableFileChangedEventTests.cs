using System;
using System.IO;
using System.Threading.Tasks;
using LightConversion.Storage.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Layouts;

namespace LightConversion.Software.Settings.Tests {
    [TestClass]
    public class WatchedReliableFileChangedEventTests {
        [TestMethod]
        public void TestFailedInitialize() {
            CreateCleanTempFolder();

            var reliableFile = new WatchedReliableFile("QuestionMark?IsNotAllowedInFileName.txt");

            try {
                reliableFile.Initialize(LogManager.CreateNullLogger());
                Assert.Fail("Initialize() with invalid name should throw exception so this line should never execute.");
            }
            catch (Exception) {
                // All good.
            }
        }

        [TestMethod]
        public async Task TestInternalWrite() {
            CreateCleanTempFolder();

            var reliableFile = new WatchedReliableFile("temp/someFile.txt");
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
        public async Task TestInitializeWithoutFolder() {
            CreateCleanTempFolder();

            var reliableFile = new WatchedReliableFile("someFile.txt");
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
        public async Task TestLastFileContent() {
            CreateCleanTempFolder();

            var reliableFile = new WatchedReliableFile("temp/someFile.txt");
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
        public async Task TestExternalWrite() {
            CreateCleanTempFolder();

            var reliableFile = new WatchedReliableFile("temp/someFile.txt");
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
        public async Task TestRecreateFile() {
            CreateCleanTempFolder();

            File.WriteAllText("temp/someFile.txt", "Creating file.");

            var reliableFile = new WatchedReliableFile("temp/someFile.txt");
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
        public async Task TestMoveFile() {
            CreateCleanTempFolder();

            File.WriteAllText("temp/someFile.txt", "Creating file.");
            var reliableFile = new WatchedReliableFile("temp/someFile.txt");
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
        public async Task TestExternalDelete() {
            CreateCleanTempFolder();

            var reliableFile = new WatchedReliableFile("temp/someFile.txt");
            reliableFile.Initialize();

            var changedCounter = 0;
            reliableFile.Changed += (sender, args) => {
                changedCounter += 1;
            };

            File.Delete("temp/someFile.txt");

            // We need to release main thread and wait for changed event.
            await Task.Delay(10);

            Assert.AreEqual(0, changedCounter, "Delete should not rise any changed events.");
        }

        [TestMethod]
        public void TestLogging() {
            CreateCleanTempFolder();

            // Create logger that writes stacktrace to log file.
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "temp/nlogfile.txt", Layout = Layout.FromString("${message} ${exception:format=ToString}") };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
            var logger = LogManager.GetCurrentClassLogger();

            var testFilePath = "temp/someFile.txt";
            var reliableFile = new WatchedReliableFile(testFilePath);
            reliableFile.Initialize(logger);

            reliableFile.TryWriteAllText("Reliable write to file.");

            // Setting file to read-only.
            File.SetAttributes(testFilePath, File.GetAttributes(testFilePath) | FileAttributes.ReadOnly);

            var logFileBeforeWrite = File.ReadAllText("temp/nlogfile.txt");
            reliableFile.TryWriteAllText("This should never be written cuz of read-only file attribute.");

            // Remove read-only attribute from file.
            File.SetAttributes(testFilePath, File.GetAttributes(testFilePath) & ~FileAttributes.ReadOnly);

            var logFileAfterWrite = File.ReadAllText("temp/nlogfile.txt");
            Assert.IsTrue(logFileBeforeWrite.Length != logFileAfterWrite.Length);
        }

        private void CreateCleanTempFolder() {
            if (Directory.Exists("temp")) {
                if (File.Exists("temp/someFile.txt")) {
                    File.SetAttributes("temp/someFile.txt", File.GetAttributes("temp/someFile.txt") & ~FileAttributes.ReadOnly);
                }

                Directory.Delete("temp", true);
            }

            Directory.CreateDirectory("temp");
        }
    }
}