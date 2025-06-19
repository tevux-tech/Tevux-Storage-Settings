namespace Tevux.Storage.Settings;

public partial class SettingsProvider {
    public bool TrySet(string key, float value) {
        return TrySetInternal(key, value);
    }

    public bool TrySet(string key, string value) {
        return TrySetInternal(key, value);
    }

    public bool TrySet(string key, bool value) {
        return TrySetInternal(key, value);
    }
}
