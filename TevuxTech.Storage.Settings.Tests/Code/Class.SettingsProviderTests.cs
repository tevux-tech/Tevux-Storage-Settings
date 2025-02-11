using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Layouts;
using TevuxTech.Storage.Settings;

namespace TevuxTech.Software.Settings.Tests;

[TestClass]
public class SettingsProviderTests {
    public double Test;


    [TestMethod]
    public void TestBasicSetGet() {
        CreateCleanTempFolder();

        var settings = new SettingsProvider();
        var settingsFile = new ReliableFile();
        settingsFile.Initialize("temp/someSettings.json");
        settings.Initialize(settingsFile);

        settings.TrySet("SomeIntegerKey", 123);
        settings.TryGet("SomeIntegerKey", out int someIntegerValue);
        Assert.AreEqual(123, someIntegerValue);

        settings.TrySet("SomeDoubleKey", 123.456);
        settings.TryGet("SomeDoubleKey", out double someDoubleValue);
        Assert.AreEqual(123.456, someDoubleValue);

        settings.TrySet("SomeBoolKey", true);
        settings.TryGet("SomeBoolKey", out bool someBoolValue);
        Assert.AreEqual(true, someBoolValue);

        settings.TrySet("SomeStringKey", "Some string");
        settings.TryGet("SomeStringKey", out string someStringValue);
        Assert.AreEqual("Some string", someStringValue);

        var someDateTimeToSet = DateTime.Now;
        settings.TrySet("SomeDateTimeKey", someDateTimeToSet);
        settings.TryGet("SomeDateTimeKey", out DateTime someDateTimeValue);
        Assert.AreEqual(someDateTimeToSet, someDateTimeValue);
    }

    [TestMethod]
    public void TestBasicFileLoading() {
        CreateCleanTempFolder();

        File.WriteAllText("temp/someSettings.json", @"{
                ""SomeIntegerKey"": 123,
                ""SomeDoubleKey"": 123.456,
                ""SomeBoolKey"": true,
                ""SomeStringKey"": ""Some string""
            }");

        var settings = new SettingsProvider();
        var settingsFile = new ReliableFile();
        settingsFile.Initialize("temp/someSettings.json");
        settings.Initialize(settingsFile);

        var isOk = settings.TryGet("SomeIntegerKey", out int someIntegerValue);
        Assert.IsTrue(isOk);
        Assert.AreEqual(123, someIntegerValue);

        isOk = settings.TryGet("SomeDoubleKey", out double someDoubleValue);
        Assert.IsTrue(isOk);
        Assert.AreEqual(123.456, someDoubleValue);

        isOk = settings.TryGet("SomeBoolKey", out bool someBoolValue);
        Assert.IsTrue(isOk);
        Assert.AreEqual(true, someBoolValue);

        isOk = settings.TryGet("SomeStringKey", out string someStringValue);
        Assert.IsTrue(isOk);
        Assert.AreEqual("Some string", someStringValue);
    }

    [TestMethod]
    public void TestBasicFileSaving() {
        CreateCleanTempFolder();

        var settings = new SettingsProvider();
        var settingsFile = new ReliableFile();
        settingsFile.Initialize("temp/someSettings.json");
        settings.Initialize(settingsFile);

        var isOk = settings.TrySet("SomeDoubleKey", 3.1415);
        Assert.IsTrue(isOk);

        isOk = settings.TrySet("SomeStringKey", "Hello world");
        Assert.IsTrue(isOk);

        var someDateTime = new DateTime(1234, 5, 6, 7, 8, 9);
        isOk = settings.TrySet("SomeDateTimeKey", someDateTime);
        Assert.IsTrue(isOk);

        var isFilePresent = File.Exists("temp/someSettings.json");
        Assert.IsTrue(isFilePresent);

        var actualFileContents = File.ReadAllText("temp/someSettings.json");
        var expectedFileContents = @"{
  ""SomeDoubleKey"": 3.1415,
  ""SomeStringKey"": ""Hello world"",
  ""SomeDateTimeKey"": ""1234-05-06T07:08:09""
}";
        Assert.AreEqual(expectedFileContents, actualFileContents);
    }

    [TestMethod]
    public void TestNumberConversion() {
        CreateCleanTempFolder();

        File.WriteAllText("temp/someSettings.json", @"{
                ""SomeIntegerKey"": 123.456,
                ""SomeIntegerKey2"": 123.5
            }");

        var settings = new SettingsProvider();
        var settingsFile = new ReliableFile();
        settingsFile.Initialize("temp/someSettings.json");
        settings.Initialize(settingsFile);

        var isOk = settings.TryGet("SomeIntegerKey", out int someInteger);
        Assert.IsTrue(isOk);
        Assert.AreEqual(123, someInteger);

        isOk = settings.TryGet("SomeIntegerKey2", out int someInteger2);
        Assert.IsTrue(isOk);
        Assert.AreEqual(124, someInteger2, "Value should probably be rounded");
    }

    [TestMethod]
    public void TestInitializationWithInvalidJson() {
        CreateCleanTempFolder();

        File.WriteAllText("temp/someSettings.json", "Invalid json text");

        var settings = new SettingsProvider();

        try {
            var settingsFile = new ReliableFile();
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

        var settings = new SettingsProvider();
        var settingsFile = new ReliableFile();
        settingsFile.Initialize("temp/someSettings.json");
        settings.Initialize(settingsFile);

        var isBackupCreated = File.Exists(settings.DataFile.Path + ".backup");
        Assert.IsTrue(isBackupCreated);

        var isBackupCorrect = File.ReadAllText(settings.DataFile.Path + ".backup") == "Invalid json text";
        Assert.IsTrue(isBackupCorrect);
    }

    [TestMethod]
    public void TestTwoBackups() {
        CreateCleanTempFolder();

        File.WriteAllText("temp/someSettings.json", "Invalid json text");

        var settings = new SettingsProvider();
        var settingsFile = new ReliableFile();
        settingsFile.Initialize("temp/someSettings.json");
        settings.Initialize(settingsFile);

        File.WriteAllText("temp/someSettings.json", "Invalid json text again...");

        try {
            settings = new SettingsProvider();
            var settingsFile2 = new ReliableFile();
            settingsFile2.Initialize("temp/someSettings.json");
            settings.Initialize(settingsFile2);
        } catch (Exception ex) {
            Assert.Fail("No exception should be thrown", ex);
        }
    }

    [TestMethod]
    public void TestLoadingDateTime() {
        var json = @"{
              ""SomeDateTimeKey"": ""1234-05-06T07:08:09""
            }";

        var reliableFile = new ReliableFile();
        reliableFile.Initialize("temp/someSettings.json");
        reliableFile.TryWriteAllText(json);

        var settings = new SettingsProvider();
        settings.Initialize(reliableFile);

        var isOk = settings.TryGet("SomeDateTimeKey", out DateTime loadedDateTime);
        Assert.IsTrue(isOk);

        Assert.AreEqual(new DateTime(1234, 5, 6, 7, 8, 9), loadedDateTime);
    }

    [TestMethod]
    public void TestLoadingInvalidDateTime() {
        var json = @"{
              ""SomeDateTimeKey"": ""1234-05-06T99:08:09""
            }";

        var reliableFile = new ReliableFile();
        reliableFile.Initialize("temp/someSettings.json");
        reliableFile.TryWriteAllText(json);

        var settings = new SettingsProvider();
        settings.Initialize(reliableFile);

        var isOk = settings.TryGet("SomeDateTimeKey", out DateTime loadedDateTime);
        Assert.IsFalse(isOk);
    }

    [TestMethod]
    public void TestRemove() {
        CreateCleanTempFolder();

        var settings = new SettingsProvider();
        var settingsFile = new ReliableFile();
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

        var settings2 = new SettingsProvider();
        var settingsFile2 = new ReliableFile();
        settingsFile2.Initialize("temp/someSettings.json");
        settings2.Initialize(settingsFile2);

        Assert.IsFalse(settings2.Contains("SomeIntegerKey1"));
        Assert.IsTrue(settings2.Contains("SomeIntegerKey2"));
    }

    [TestMethod]
    public void TestFloatSetting() {
        CreateCleanTempFolder();

        var reliableFile = new ReliableFile();
        reliableFile.Initialize("temp/someSettings.json");

        var settings = new SettingsProvider();
        settings.Initialize(reliableFile);
        var someSetting = 10.0f / 9;
        settings.TrySet("SomeFloatNumber", someSetting);

        var isOk = settings.TryGet("SomeFloatNumber", out float loadedFloatSetting);
        Assert.IsTrue(isOk);
        Assert.AreEqual(loadedFloatSetting, someSetting);

        var someDoubleSetting = 10.0 / 9;
        settings.TrySet("SomeDoubleNumber", someDoubleSetting);
        isOk = settings.TryGet("SomeDoubleNumber", out float loadedSetting);
        Assert.IsTrue(isOk);
        Assert.AreEqual((float)someDoubleSetting, loadedSetting);
    }


    [TestMethod]
    public void TestLogging() {
        CreateCleanTempFolder();

        // Create logger that writes stacktrace to log file.
        var config = new NLog.Config.LoggingConfiguration();
        var logFilePath = "temp/nlogfile.txt";
        var logfile = new NLog.Targets.FileTarget("logfile") {
            FileName = logFilePath, Layout = Layout.FromString("${message} ${exception:format=ToString}")
        };
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
        LogManager.Configuration = config;
        var logger = LogManager.GetCurrentClassLogger();

        var settingsFilePath = "temp/someSettings.json";
        var reliableFile = new ReliableFile();
        reliableFile.Initialize(settingsFilePath);

        var settings = new SettingsProvider { Logger = logger };
        settings.Initialize(reliableFile);
        var logFileBeforeGetSetting = File.ReadAllText(logFilePath);
        var isOk = settings.TryGet("SomeNonExistingSettingName", out float _);
        Assert.IsFalse(isOk);
        var logFileAfterGetSetting = File.ReadAllText(logFilePath);

        Assert.IsTrue(logFileBeforeGetSetting.Length != logFileAfterGetSetting.Length);
    }

    private static void CreateCleanTempFolder() {
        if (Directory.Exists("temp")) {
            Directory.Delete("temp", true);
        }

        Directory.CreateDirectory("temp");
    }
}
