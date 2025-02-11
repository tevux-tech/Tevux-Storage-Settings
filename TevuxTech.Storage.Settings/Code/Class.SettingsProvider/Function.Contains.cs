namespace TevuxTech.Storage.Settings;

public partial class SettingsProvider {
    public bool Contains(string key) {
        lock (_dataLock) {
            return _dataCache.ContainsKey(key);
        }
    }
}
