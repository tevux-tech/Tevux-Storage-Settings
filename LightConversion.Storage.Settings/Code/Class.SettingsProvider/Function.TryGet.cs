using System;

namespace LightConversion.Storage.Settings {
    public partial class SettingsProvider {
        public bool TryGet(string key, out int value) {
            bool isOk;
            value = 0;

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) {
                if (TryConvert(valueOfUnknownType, out value) == false) {
                    HandleNonCriticalError($"Can't get setting with key \"{key}\" because it is already set and type does not match integer.", $"Function {nameof(TryGet)}(out int)");
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
                    HandleNonCriticalError($"Can't get setting with key \"{key}\" because it is already set and type does not match double.", $"Function {nameof(TryGet)}(out double)");
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
                    HandleNonCriticalError($"Can't get setting with key \"{key}\" because it is already set and type does not match float.", $"Function {nameof(TryGet)}(out float)");
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
                    HandleNonCriticalError($"Can't get setting with key \"{key}\" because it is already set and type does not match boolean.", $"Function {nameof(TryGet)}(out bool)");
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
                    HandleNonCriticalError($"Can't get setting with key \"{key}\" because it is already set and type does not match string.", $"Function {nameof(TryGet)}(out string)");
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
                    HandleNonCriticalError($"Can't get setting with key \"{key}\" because it is already set and type does not match DateTime.", $"Function {nameof(TryGet)}(out DateTime)");
                    isOk = false;
                }
            }

            return isOk;
        }
    }
}