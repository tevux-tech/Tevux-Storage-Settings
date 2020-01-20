namespace LightConversion.Software.Settings {
    public partial class SettingsProvider {
        private bool TrySetInternal<T>(string key, T value) {
            bool isOk;

            lock (_dataLock) {
                _dataCache[key] = value;
                var jsonBytes = Utf8Json.JsonSerializer.Serialize(_dataCache);
                var prettyJson = Utf8Json.JsonSerializer.PrettyPrint(jsonBytes);
                isOk = _dataFile.TryWriteAllText(prettyJson);
            }

            return isOk;
        }
    }
}