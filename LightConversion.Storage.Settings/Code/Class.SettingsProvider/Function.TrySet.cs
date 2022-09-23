namespace LightConversion.Storage.Settings;

public partial class SettingsProvider {
    public bool TrySet(string key, int value) {
        return TrySetInternal<int>(key, value);
    }

    public bool TrySet(string key, double value) {
        return TrySetInternal<double>(key, value);
    }

    public bool TrySet(string key, float value) {
        return TrySetInternal<float>(key, value);
    }

    public bool TrySet(string key, string value) {
        return TrySetInternal<string>(key, value);
    }

    public bool TrySet(string key, bool value) {
        return TrySetInternal<bool>(key, value);
    }

    public bool TrySet(string key, DateTime value) {
        return TrySetInternal<DateTime>(key, value);
    }
}
