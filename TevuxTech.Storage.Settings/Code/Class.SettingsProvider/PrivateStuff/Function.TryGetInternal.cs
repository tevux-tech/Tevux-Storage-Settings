namespace TevuxTech.Storage.Settings;

public partial class SettingsProvider {
    private bool TryGetInternal(string key, out object value) {
        lock (_dataLock) {
            if (_dataCache.TryGetValue(key, out var candidateValue)) {
                value = candidateValue;
            } else {
                _logger.LogError($"Can't get setting with key \"{key}\" because it doesn't exist.");
                goto error;
            }
        }

        return true;

        error:
        value = new object();
        return false;
    }
}
