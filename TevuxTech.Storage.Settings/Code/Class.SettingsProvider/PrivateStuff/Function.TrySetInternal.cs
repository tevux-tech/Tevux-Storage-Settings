namespace TevuxTech.Storage.Settings;

public partial class SettingsProvider {
    private bool TrySetInternal<T>(string key, T value) {
        if (value is null) {
            _logger.LogError($"Setting \"{key}\" to null is not a good idea.");
            goto error;
        }

        lock (_dataLock) {
            _dataCache[key] = value;
            var jsonBytes = JsonSerializer.Serialize(_dataCache);
            var prettyJsonBytes = JsonSerializer.PrettyPrintByteArray(jsonBytes);
            if (DataFile.TryWriteAllBytes(prettyJsonBytes) == false) {
                _logger.LogError($"Setting \"{key}\" to {value} failed because writing to file failed.");
                goto error;
            }
        }

        return true;

        error:
        return false;
    }
}
