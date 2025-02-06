namespace LightConversion.Storage.Settings;

public partial class SettingsProvider {
    public bool TryRemove(string key) {
        bool isOk;
        var errorMessage = "";

        lock (_dataLock) {
            if (_dataCache.Remove(key)) {
                var jsonBytes = Utf8Json.JsonSerializer.Serialize(_dataCache);
                var prettyJsonBytes = Utf8Json.JsonSerializer.PrettyPrintByteArray(jsonBytes);
                isOk = DataFile.TryWriteAllBytes(prettyJsonBytes);

                if (isOk == false) {
                    errorMessage = $"Removing setting \"{key}\" failed because writing to file failed.";
                }
            } else {
                isOk = false;
                errorMessage = $"Can't remove \"{key}\" because it doesn't exist.";
            }
        }

        if (isOk == false) {
            _logger.LogError(errorMessage);
        }

        return isOk;
    }
}
