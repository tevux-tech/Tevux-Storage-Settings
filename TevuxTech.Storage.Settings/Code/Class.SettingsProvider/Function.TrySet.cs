namespace TevuxTech.Storage.Settings;

public partial class SettingsProvider {
    public bool TrySet(string key, int value) {
        return TrySetInternal(key, value);
    }

    public bool TrySet(string key, double value) {
        return TrySetInternal(key, value);
    }

    public bool TrySet(string key, float value) {
        return TrySetInternal(key, value);
    }

    public bool TrySet(string key, string value) {
        return TrySetInternal(key, value);
    }

    public bool TrySet(string key, bool value) {
        return TrySetInternal(key, value);
    }

    public bool TrySet(string key, DateTime value) {
        return TrySetInternal(key, value);
    }
}
