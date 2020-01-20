using System.Collections.Generic;

namespace LightConversion.Software.Settings {
    public partial class SettingsProvider {
        public void Initialize(string storageFilePath) {
            _dataFile = new ReliableFile(storageFilePath);

            if (_dataFile.TryReadAllText(out var fileContents)) {
                // TODO: what if invalid json is saved?
                _dataCache = Utf8Json.JsonSerializer.Deserialize<Dictionary<string, object>>(fileContents);
            } else {
                // Probably file doesn't exist, initializing to empty stuff. 
                _dataFile.TryWriteAllText("{}");
                _dataCache = new Dictionary<string, object>();
            }

            IsInitialized = true;
        }
    }
}