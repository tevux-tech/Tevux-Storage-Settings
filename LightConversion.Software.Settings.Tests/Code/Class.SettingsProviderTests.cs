using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LightConversion.Software.Settings.Tests {
    [TestClass]
    public class SettingsProviderTests {
        public double Test;


        [TestMethod]
        public void TestBasicSetGet() {
            CreateCleanTempFolder();

            var settings = new SettingsProvider();
            settings.Initialize("temp/someSettings.json");

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
            settings.Initialize("temp/someSettings.json");

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
            settings.Initialize("temp/someSettings.json");

            var isOk = settings.TrySet("SomeDoubleKey", 3.1415);
            Assert.IsTrue(isOk);

            isOk = settings.TrySet("SomeStringKey", "Hello world");
            Assert.IsTrue(isOk);

            var isFilePresent = File.Exists("temp/someSettings.json");
            Assert.IsTrue(isFilePresent);

            var actualFileContents = File.ReadAllText("temp/someSettings.json");
            var expectedFileContents = @"{
  ""SomeDoubleKey"": 3.1415,
  ""SomeStringKey"": ""Hello world""
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
            settings.Initialize("temp/someSettings.json");

            var isOk = settings.TryGet("SomeIntegerKey", out int someInteger);
            Assert.IsTrue(isOk);
            Assert.AreEqual(123, someInteger);

            isOk = settings.TryGet("SomeIntegerKey2", out int someInteger2);
            Assert.IsTrue(isOk);
            Assert.AreEqual(124, someInteger2, "Value should probably be rounded");
        }

        [TestMethod]
        public void TestDefaultValues() {
            CreateCleanTempFolder();

            var settings = new SettingsProvider();
            settings.Initialize("temp/someSettings.json");

            var isOk = settings.TryGet("NonExistingKey", out int value, true, 123);
            Assert.IsTrue(isOk);
            Assert.AreEqual(123, value);

            // Value should now be set to default after last operation.
            isOk = settings.TryGet("NonExistingKey", out value);
            Assert.IsTrue(isOk);
            Assert.AreEqual(123, value);
        }

        [TestMethod]
        public void TestInitializationWithInvalidJson() {
            CreateCleanTempFolder();

            File.WriteAllText("temp/someSettings.json", "Invalid json text");

            var settings = new SettingsProvider();

            try {
                settings.Initialize("temp/someSettings.json");
            } catch (Exception ex) {
                Assert.Fail("Initialization shouldn't throw any exceptions", ex);
            }
        }

        private void CreateCleanTempFolder() {
            if (Directory.Exists("temp")) {
                Directory.Delete("temp", true);
            }

            Directory.CreateDirectory("temp");
        }
    }
}