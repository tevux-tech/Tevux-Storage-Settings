using System.Collections.Generic;
using System.IO;
using Utf8Json;

namespace LightConversion.Software.Settings {
    public partial class SettingsProvider {
        public void Initialize(ReliableFile dataFile) {
            DataFile = dataFile;

            if (DataFile.Exists() == false) {
                HandleInfoReady($"File \"{DataFile.Path}\" doesn't exist. Creating new empty one.");
                DataFile.TryWriteAllText("{}");
            }

            if (DataFile.TryReadAllBytes(out var fileBytes)) {
                try {
                    _dataCache = JsonSerializer.Deserialize<Dictionary<string, object>>(fileBytes);
                } catch (JsonParsingException) {
                    HandleNonCriticalError($"Deserializing \"{dataFile.Path}\" failed. File was probably modified manually to invalid json. Creating backup of it and recreating empty setting file.");

                    var backupFilePath = DataFile.Path + ".backup";
                    File.Copy(DataFile.Path, backupFilePath, true);

                    DataFile.TryWriteAllText("{}");
                    _dataCache = new Dictionary<string, object>();
                }
            } else {
                HandleNonCriticalError($"Reading from \"{DataFile.Path}\" failed. No settings will be loaded.");
                _dataCache = new Dictionary<string, object>();
            }

            IsInitialized = true;
        }
    }
}