using System.Collections.Generic;
using System.IO;
using Utf8Json;

namespace LightConversion.Software.Settings {
    public partial class SettingsProvider {
        public void Initialize(string storageFilePath) {
            _dataFile = new ReliableFile(storageFilePath);

            if (_dataFile.TryReadAllText(out var fileContents)) {
                try {
                    _dataCache = JsonSerializer.Deserialize<Dictionary<string, object>>(fileContents);
                } catch (JsonParsingException) {
                    // File was probably modified manually to invalid json. Creating backup of it and recreating empty setting file.
                    var backupFilePath = storageFilePath + ".backup";
                    File.Copy(_dataFile.Path, backupFilePath, true);

                    _dataFile.TryWriteAllText("{}");
                    _dataCache = new Dictionary<string, object>();
                }
            } else {
                // Probably file doesn't exist, initializing to empty stuff. 
                _dataFile.TryWriteAllText("{}");
                _dataCache = new Dictionary<string, object>();
            }

            IsInitialized = true;
        }
    }
}