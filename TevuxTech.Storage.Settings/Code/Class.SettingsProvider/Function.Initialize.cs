namespace TevuxTech.Storage.Settings;

public partial class SettingsProvider {
    public void Initialize(ReliableFile dataFile) {
        ArgumentNullException.ThrowIfNull(dataFile);

        DataFile = dataFile;

        if (DataFile.Exists() == false) {
            _logger.LogInformation($"File \"{DataFile.Path}\" doesn't exist. Creating new empty one.");
            DataFile.TryWriteAllText("{}");
        }

        if (DataFile.TryReadAllBytes(out var fileBytes)) {
            try {
                _dataCache = JsonSerializer.Deserialize<Dictionary<string, object>>(fileBytes);
            } catch (JsonParsingException ex) {
                _logger.LogError(ex, $"Deserializing \"{dataFile.Path}\" failed. File was probably modified manually to invalid json. Creating backup of it and recreating empty setting file. File content: {Encoding.UTF8.GetString(fileBytes)}");

                var backupFilePath = DataFile.Path + ".backup";
                File.Copy(DataFile.Path, backupFilePath, true);

                DataFile.TryWriteAllText("{}");
                _dataCache = [];
            }
        } else {
            _logger.LogError($"Reading from \"{DataFile.Path}\" failed. No settings will be loaded.");
            _dataCache = new Dictionary<string, object>();
        }

        IsInitialized = true;
    }
}
