namespace LightConversion.Software.Settings {
    public partial class SettingsProvider {
        private bool TryGetInternal(string key, out object value) {
            lock (_dataLock) {
                return _dataCache.TryGetValue(key, out value);
            }
        }
    }
}