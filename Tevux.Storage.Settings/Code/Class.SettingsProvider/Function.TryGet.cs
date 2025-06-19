namespace Tevux.Storage.Settings;

public partial class SettingsProvider {
    public int Get(string key, int defaultValue) {
        if (TryGet(key, out int value)) {
            return value;
        } else {
            return defaultValue;
        }
    }

    public string Get(string key, string defaultValue = "") {
        if (TryGet(key, out string value)) {
            return value;
        } else {
            return defaultValue;
        }
    }

    public double Get(string key, double defaultValue) {
        if (TryGet(key, out double value)) {
            return value;
        } else {
            return defaultValue;
        }
    }

    public bool Get(string key, bool defaultValue) {
        if (TryGet(key, out bool value)) {
            return value;
        } else {
            return defaultValue;
        }
    }

    public float Get(string key, float defaultValue) {
        if (TryGet(key, out float value)) {
            return value;
        } else {
            return defaultValue;
        }
    }

    public DateTime Get(string key, DateTime defaultValue) {
        if (TryGet(key, out DateTime value)) {
            return value;
        } else {
            return defaultValue;
        }
    }

    public bool TryGet(string key, out int value) {
        value = 0;

        var isOk = TryGetInternal(key, out var valueOfUnknownType);
        if (isOk) {
            if (TryConvert(valueOfUnknownType, out value) == false) {
                _logger.LogError($"Can't get setting with key \"{key}\" because it is already set and type does not match integer. Setting value: {Encoding.UTF8.GetString(JsonSerializer.Serialize(valueOfUnknownType))}.");
                isOk = false;
            }
        }

        return isOk;
    }

    public bool TryGet(string key, out double value) {
        value = 0;

        var isOk = TryGetInternal(key, out var valueOfUnknownType);
        if (isOk) {
            if (TryConvert(valueOfUnknownType, out value) == false) {
                _logger.LogError($"Can't get setting with key \"{key}\" because it is already set and type does not match double. Setting value: {Encoding.UTF8.GetString(JsonSerializer.Serialize(valueOfUnknownType))}.");
                isOk = false;
            }
        }

        return isOk;
    }

    public bool TryGet(string key, out float value) {
        value = 0;

        var isOk = TryGetInternal(key, out var valueOfUnknownType);
        if (isOk) {
            if (TryConvert(valueOfUnknownType, out value) == false) {
                _logger.LogError($"Can't get setting with key \"{key}\" because it is already set and type does not match float. Setting value: {Encoding.UTF8.GetString(JsonSerializer.Serialize(valueOfUnknownType))}.");
                isOk = false;
            }
        }

        return isOk;
    }

    public bool TryGet(string key, out bool value) {
        value = false;

        var isOk = TryGetInternal(key, out var valueOfUnknownType);
        if (isOk) {
            if (TryConvert(valueOfUnknownType, out value) == false) {
                _logger.LogError($"Can't get setting with key \"{key}\" because it is already set and type does not match boolean. Setting value: {Encoding.UTF8.GetString(JsonSerializer.Serialize(valueOfUnknownType))}.");
                isOk = false;
            }
        }

        return isOk;
    }

    public bool TryGet(string key, out string value) {
        value = "";

        var isOk = TryGetInternal(key, out var valueOfUnknownType);
        if (isOk) {
            if (TryConvert(valueOfUnknownType, out value) == false) {
                _logger.LogError($"Can't get setting with key \"{key}\" because it is already set and type does not match string. Setting value: {Encoding.UTF8.GetString(JsonSerializer.Serialize(valueOfUnknownType))}.");
                isOk = false;
            }
        }

        return isOk;
    }

    public bool TryGet(string key, out DateTime value) {
        value = DateTime.MinValue;

        var isOk = TryGetInternal(key, out var valueOfUnknownType);
        if (isOk) {
            if (TryConvert(valueOfUnknownType, out value) == false) {
                _logger.LogError($"Can't get setting with key \"{key}\" because it is already set and type does not match DateTime. Setting value: {Encoding.UTF8.GetString(JsonSerializer.Serialize(valueOfUnknownType))}.");
                isOk = false;
            }
        }

        return isOk;
    }
}
