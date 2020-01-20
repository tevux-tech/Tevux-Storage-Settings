using System;
using System.Collections.Generic;

namespace LightConversion.Software.Settings {
    public class SettingsProvider : IDisposable {
        private Dictionary<string, object> _dataCache;
        private readonly object _dataLock = new object();
        private ReliableFile _dataFile;
        public bool IsInitialized { get; private set; }

        public void Initialize(string storageFilePath) {
            _dataFile = new ReliableFile(storageFilePath);

            if (_dataFile.TryReadAllText(out var fileContents)) {
                // TODO: what if invalid json is saved?
                _dataCache = Utf8Json.JsonSerializer.Deserialize<Dictionary<string, object>>(fileContents);
            } else {
                // Probably file doesn't exist, initializing to empty stuff. 
                _dataFile.TryWriteAllText("{}");
                _dataCache = new Dictionary<string, object>();
            }

            IsInitialized = true;
        }

        public void Dispose() {}

        public bool Exists(string key) {
            return true;
        }

        public bool TrySet(string key, int value) {
            return TrySetInternal<int>(key, value);
        }

        public bool TrySet(string key, double value) {
            return TrySetInternal<double>(key, value);
        }

        public bool TrySet(string key, string value) {
            return TrySetInternal<string>(key, value);
        }

        public bool TrySet(string key, bool value) {
            return TrySetInternal<bool>(key, value);
        }

        private bool TrySetInternal<T>(string key, T value) {
            bool isOk;

            lock (_dataLock) {
                _dataCache[key] = value;
                var jsonBytes = Utf8Json.JsonSerializer.Serialize(_dataCache);
                var prettyJson = Utf8Json.JsonSerializer.PrettyPrint(jsonBytes);
                isOk = _dataFile.TryWriteAllText(prettyJson);
            }

            return isOk;
        }

        public bool TryGet(string key, out int value, bool setToDefaultIfDoesNotExist = false, int defaultValue = 0) {
            bool isOk;
            value = 0;

            object valueOfUnknownType;

            lock (_dataLock) {
                isOk = _dataCache.TryGetValue(key, out valueOfUnknownType);
            }

            if (isOk) {
                isOk = TryConvertToInt(valueOfUnknownType, out value);
            }

            return isOk;
        }

        public bool TryGet(string key, out double value, bool setToDefaultIfDoesNotExist = false, double defaultValue = 0) {
            bool isOk;
            value = 0;

            object valueOfUnknownType;

            lock (_dataLock) {
                isOk = _dataCache.TryGetValue(key, out valueOfUnknownType);
            }

            if (isOk) {
                isOk = TryConvertToDouble(valueOfUnknownType, out value);
            }

            return isOk;
        }

        public bool TryGet(string key, out string value, bool setToDefaultIfDoesNotExist = false, string defaultValue = null) {
            bool isOk;
            value = "";

            object valueOfUnknownType;

            lock (_dataLock) {
                isOk = _dataCache.TryGetValue(key, out valueOfUnknownType);
            }

            if (isOk) {
                isOk = TryConvertToString(valueOfUnknownType, out value);
            }

            return isOk;
        }

        public bool TryGet(string key, out bool value, bool setToDefaultIfDoesNotExist = false, bool defaultValue = false) {
            bool isOk;
            value = false;

            object valueOfUnknownType;

            lock (_dataLock) {
                isOk = _dataCache.TryGetValue(key, out valueOfUnknownType);
            }

            if (isOk) {
                isOk = TryConvertToBool(valueOfUnknownType, out value);
            }

            return isOk;
        }

        private bool TryConvertToInt(object valueToConvert, out int convertedValue) {
            var isOk = true;

            if (valueToConvert is int valueAsInt) {
                convertedValue = valueAsInt;
            } else if (valueToConvert is long valueAsLong) {
                convertedValue = (int)valueAsLong;
            } else if (valueToConvert is double valueAsDouble) {
                convertedValue = Convert.ToInt32(valueAsDouble);
            } else {
                convertedValue = 0;
                isOk = false;
            }

            return isOk;
        }

        private bool TryConvertToDouble(object valueToConvert, out double convertedValue) {
            var isOk = true;

            if (valueToConvert is int valueAsInt) {
                convertedValue = valueAsInt;
            } else if (valueToConvert is long valueAsLong) {
                convertedValue = valueAsLong;
            } else if (valueToConvert is double valueAsDouble) {
                convertedValue = valueAsDouble;
            } else {
                convertedValue = 0;
                isOk = false;
            }

            return isOk;
        }

        private bool TryConvertToBool(object valueToConvert, out bool convertedValue) {
            bool isOk;

            if (valueToConvert is bool valueAsBool) {
                isOk = true;
                convertedValue = valueAsBool;
            } else {
                isOk = false;
                convertedValue = false;
            }

            return isOk;
        }

        private bool TryConvertToString(object valueToConvert, out string convertedValue) {
            if (valueToConvert is string valueAsString) {
                convertedValue = valueAsString;
                return true;
            } else {
                convertedValue = "";
                return false;
            }
        }
    }
}