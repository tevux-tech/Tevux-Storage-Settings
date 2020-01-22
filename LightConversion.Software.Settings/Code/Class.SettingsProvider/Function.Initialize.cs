using System.Collections.Generic;
using System.IO;
using Utf8Json;

namespace LightConversion.Software.Settings {
    public partial class SettingsProvider {
        public void Initialize(ReliableFile dataFile) {
            DataFile = dataFile;

            if (DataFile.TryReadAllBytes(out var fileBytes)) {
                try {
                    _dataCache = JsonSerializer.Deserialize<Dictionary<string, object>>(fileBytes);
                } catch (JsonParsingException) {
                    // File was probably modified manually to invalid json. Creating backup of it and recreating empty setting file.
                    var backupFilePath = DataFile.Path + ".backup";
                    File.Copy(DataFile.Path, backupFilePath, true);

                    DataFile.TryWriteAllText("{}");
                    _dataCache = new Dictionary<string, object>();
                }
            } else {
                // Probably file doesn't exist, initializing to empty stuff. 
                DataFile.TryWriteAllText("{}");
                _dataCache = new Dictionary<string, object>();
            }

            IsInitialized = true;
        }
    }
}