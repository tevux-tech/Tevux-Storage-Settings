namespace LightConversion.Storage.Settings {
    public partial class SettingsProvider {
        private bool TrySetInternal<T>(string key, T value) {
            bool isOk;

            lock (_dataLock) {
                _dataCache[key] = value;
                var jsonBytes = Utf8Json.JsonSerializer.Serialize(_dataCache);
                var prettyJsonBytes = Utf8Json.JsonSerializer.PrettyPrintByteArray(jsonBytes);
                isOk = DataFile.TryWriteAllBytes(prettyJsonBytes);
            }

            if (isOk == false) {
                HandleNonCriticalError($"Setting setting \"{key}\" to {value} failed because writing to file failed.", $"Function {nameof(TrySetInternal)}()");
            }

            return isOk;
        }
    }
}