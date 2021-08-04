using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NLog;
using Utf8Json;

namespace LightConversion.Storage.Settings {
    public partial class SettingsProvider {
        public void Initialize(ReliableFile dataFile) {
            Initialize(dataFile, LogManager.CreateNullLogger());
        }

        public void Initialize(ReliableFile dataFile, Logger logger) {
            DataFile = dataFile;

            if (logger == null) {
                throw new InvalidOperationException($"Argument {nameof(logger)} can't be null.");
            }

            _logger = logger;

            if (DataFile.Exists() == false) {
                _logger.Info($"File \"{DataFile.Path}\" doesn't exist. Creating new empty one.");
                DataFile.TryWriteAllText("{}");
            }

            if (DataFile.TryReadAllBytes(out var fileBytes)) {
                try {
                    _dataCache = JsonSerializer.Deserialize<Dictionary<string, object>>(fileBytes);
                } catch (JsonParsingException ex) {
                    _logger.Error(ex, $"Deserializing \"{dataFile.Path}\" failed. File was probably modified manually to invalid json. Creating backup of it and recreating empty setting file. File content: {Encoding.UTF8.GetString(fileBytes)}");

                    var backupFilePath = DataFile.Path + ".backup";
                    File.Copy(DataFile.Path, backupFilePath, true);

                    DataFile.TryWriteAllText("{}");
                    _dataCache = new Dictionary<string, object>();
                }
            } else {
                _logger.Error($"Reading from \"{DataFile.Path}\" failed. No settings will be loaded.");
                _dataCache = new Dictionary<string, object>();
            }

            IsInitialized = true;
        }
    }
}