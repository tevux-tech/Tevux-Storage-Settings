namespace LightConversion.Storage.Settings {
    public partial class SettingsProvider {
        private bool TryGetInternal(string key, out object value) {
            bool isOk;

            lock (_dataLock) {
                isOk = _dataCache.TryGetValue(key, out value);
            }

            if (isOk == false) {
                Logger.Error($"Can't get setting with key \"{key}\" because it doesn't exist.");
            }

            return isOk;
        }
    }
}