namespace TevuxTech.Storage.Settings;

public partial class SettingsProvider {
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
