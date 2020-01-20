using System.Configuration;
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

        private void CreateCleanTempFolder() {
            if (Directory.Exists("temp")) {
                Directory.Delete("temp", true);
            }

            Directory.CreateDirectory("temp");
        }
    }
}