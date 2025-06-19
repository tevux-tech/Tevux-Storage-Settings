using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NullLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger;

namespace Tevux.Storage.Settings;

[TestClass]
public class SettingsProviderTests {
    [TestMethod]
    public void TestBasicFileLoading() {
        CreateCleanTempFolder();

        File.WriteAllText("temp/someSettings.json", @"{
                ""SomeIntegerKey"": 123,
                ""SomeBoolKey"": true,
                ""SomeStringKey"": ""Some string""
            }");

        var settings = new SettingsProvider(NullLoggerFactory.Instance);
        var settingsFile = new ReliableFile(NullLogger.Instance);
        settingsFile.Initialize("temp/someSettings.json");
        settings.Initialize(settingsFile);

        var isOk = settings.TryGet("SomeBoolKey", out bool someBoolValue);
        Assert.IsTrue(isOk);
        Assert.AreEqual(true, someBoolValue);

        isOk = settings.TryGet("SomeStringKey", out string someStringValue);
        Assert.IsTrue(isOk);
        Assert.AreEqual("Some string", someStringValue);
    }

    [TestMethod]
    public void TestBasicFileSaving() {
        CreateCleanTempFolder();

        var settings = new SettingsProvider(NullLoggerFactory.Instance);
        var settingsFile = new ReliableFile(NullLogger.Instance);
        settingsFile.Initialize("temp/someSettings.json");
        settings.Initialize(settingsFile);

        var isOk = settings.TrySet("SomeStringKey", "Hello world");
        Assert.IsTrue(isOk);

        var isFilePresent = File.Exists("temp/someSettings.json");
        Assert.IsTrue(isFilePresent);

        var actualFileContents = File.ReadAllText("temp/someSettings.json");
        var expectedFileContents = """
                                   {
                                     "SomeStringKey": "Hello world"
                                   }
                                   """;
        Assert.AreEqual(expectedFileContents, actualFileContents);
    }

    [TestMethod]
    public void TestBasicSetGet() {
        CreateCleanTempFolder();

        var settings = new SettingsProvider(NullLoggerFactory.Instance);
        var settingsFile = new ReliableFile(NullLogger.Instance);
        settingsFile.Initialize("temp/someSettings.json");
        settings.Initialize(settingsFile);

        settings.TrySet("SomeBoolKey", true);
        settings.TryGet("SomeBoolKey", out bool someBoolValue);
        Assert.AreEqual(true, someBoolValue);

        settings.TrySet("SomeStringKey", "Some string");
        settings.TryGet("SomeStringKey", out string someStringValue);
        Assert.AreEqual("Some string", someStringValue);
    }

    [TestMethod]
    public void TestFloatSetting() {
        CreateCleanTempFolder();

        var reliableFile = new ReliableFile(NullLogger.Instance);
        reliableFile.Initialize("temp/someSettings.json");

        var settings = new SettingsProvider(NullLoggerFactory.Instance);
        settings.Initialize(reliableFile);
        var someSetting = 10.0f / 9;
        settings.TrySet("SomeFloatNumber", someSetting);

        var isOk = settings.TryGet("SomeFloatNumber", out float loadedFloatSetting);
        Assert.IsTrue(isOk);
        Assert.AreEqual(loadedFloatSetting, someSetting);
    }

    [TestMethod]
    public void TestInitializationWithInvalidJson() {
        CreateCleanTempFolder();

        File.WriteAllText("temp/someSettings.json", "Invalid json text");

        var settings = new SettingsProvider(NullLoggerFactory.Instance);

        try {
            var settingsFile = new ReliableFile(NullLogger.Instance);
            settingsFile.Initialize("temp/someSettings.json");
            settings.Initialize(settingsFile);
        } catch (Exception ex) {
            Assert.Fail("Initialization shouldn't throw any exceptions", ex);
        }
    }

    [TestMethod]
    public void TestInvalidJsonBackup() {
        CreateCleanTempFolder();

        File.WriteAllText("temp/someSettings.json", "Invalid json text");

        var settings = new SettingsProvider(NullLoggerFactory.Instance);
        var settingsFile = new ReliableFile(NullLogger.Instance);
        settingsFile.Initialize("temp/someSettings.json");
        settings.Initialize(settingsFile);

        var isBackupCreated = File.Exists(settings.DataFile.Path + ".backup");
        Assert.IsTrue(isBackupCreated);

        var isBackupCorrect = File.ReadAllText(settings.DataFile.Path + ".backup") == "Invalid json text";
        Assert.IsTrue(isBackupCorrect);
    }

    [TestMethod]
    public void TestRemove() {
        CreateCleanTempFolder();

        var settings = new SettingsProvider(NullLoggerFactory.Instance);
        var settingsFile = new ReliableFile(NullLogger.Instance);
        settingsFile.Initialize("temp/someSettings.json");
        settings.Initialize(settingsFile);

        settings.TrySet("SomeIntegerKey1", 123);
        settings.TrySet("SomeIntegerKey2", 456);

        var isOk = settings.TryRemove("SomeIntegerKey1");
        Assert.IsTrue(isOk);

        isOk = settings.TryRemove("InvalidKey");
        Assert.IsFalse(isOk);

        Assert.IsFalse(settings.Contains("SomeIntegerKey1"));
        Assert.IsTrue(settings.Contains("SomeIntegerKey2"));

        var settings2 = new SettingsProvider(NullLoggerFactory.Instance);
        var settingsFile2 = new ReliableFile(NullLogger.Instance);
        settingsFile2.Initialize("temp/someSettings.json");
        settings2.Initialize(settingsFile2);

        Assert.IsFalse(settings2.Contains("SomeIntegerKey1"));
        Assert.IsTrue(settings2.Contains("SomeIntegerKey2"));
    }

    [TestMethod]
    public void TestTwoBackups() {
        CreateCleanTempFolder();

        File.WriteAllText("temp/someSettings.json", "Invalid json text");

        var settings = new SettingsProvider(NullLoggerFactory.Instance);
        var settingsFile = new ReliableFile(NullLogger.Instance);
        settingsFile.Initialize("temp/someSettings.json");
        settings.Initialize(settingsFile);

        File.WriteAllText("temp/someSettings.json", "Invalid json text again...");

        try {
            settings = new SettingsProvider(NullLoggerFactory.Instance);
            var settingsFile2 = new ReliableFile(NullLogger.Instance);
            settingsFile2.Initialize("temp/someSettings.json");
            settings.Initialize(settingsFile2);
        } catch (Exception ex) {
            Assert.Fail("No exception should be thrown", ex);
        }
    }

    private static void CreateCleanTempFolder() {
        if (Directory.Exists("temp")) {
            Directory.Delete("temp", true);
        }

        Directory.CreateDirectory("temp");
    }
}
