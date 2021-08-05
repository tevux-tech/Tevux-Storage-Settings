using System;
using System.Text;
using Utf8Json;

namespace LightConversion.Storage.Settings {
    public partial class SettingsProvider {
        public bool TryGet(string key, out int value) {
            bool isOk;
            value = 0;

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) {
                if (TryConvert(valueOfUnknownType, out value) == false) {
                    Logger.Error($"Can't get setting with key \"{key}\" because it is already set and type does not match integer. Setting value: {Encoding.UTF8.GetString(JsonSerializer.Serialize(valueOfUnknownType))}.");
                    isOk = false;
                }
            }

            return isOk;
        }

        public bool TryGet(string key, out double value) {
            bool isOk;
            value = 0;

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) {
                if (TryConvert(valueOfUnknownType, out value) == false) {
                    Logger.Error($"Can't get setting with key \"{key}\" because it is already set and type does not match double. Setting value: {Encoding.UTF8.GetString(JsonSerializer.Serialize(valueOfUnknownType))}.");
                    isOk = false;
                }
            }

            return isOk;
        }

        public bool TryGet(string key, out float value) {
            bool isOk;
            value = 0;

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) {
                if (TryConvert(valueOfUnknownType, out value) == false) {
                    Logger.Error($"Can't get setting with key \"{key}\" because it is already set and type does not match float. Setting value: {Encoding.UTF8.GetString(JsonSerializer.Serialize(valueOfUnknownType))}.");
                    isOk = false;
                }
            }

            return isOk;
        }

        public bool TryGet(string key, out bool value) {
            bool isOk;
            value = false;

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) {
                if (TryConvert(valueOfUnknownType, out value) == false) {
                    Logger.Error($"Can't get setting with key \"{key}\" because it is already set and type does not match boolean. Setting value: {Encoding.UTF8.GetString(JsonSerializer.Serialize(valueOfUnknownType))}.");
                    isOk = false;
                }
            }

            return isOk;
        }

        public bool TryGet(string key, out string value) {
            bool isOk;
            value = "";

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) {
                if (TryConvert(valueOfUnknownType, out value) == false) {
                    Logger.Error($"Can't get setting with key \"{key}\" because it is already set and type does not match string. Setting value: {Encoding.UTF8.GetString(JsonSerializer.Serialize(valueOfUnknownType))}.");
                    isOk = false;
                }
            }

            return isOk;
        }

        public bool TryGet(string key, out DateTime value) {
            bool isOk;
            value = DateTime.MinValue;

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) {
                if (TryConvert(valueOfUnknownType, out value) == false) {
                    Logger.Error($"Can't get setting with key \"{key}\" because it is already set and type does not match DateTime. Setting value: {Encoding.UTF8.GetString(JsonSerializer.Serialize(valueOfUnknownType))}.");
                    isOk = false;
                }
            }

            return isOk;
        }
    }
}