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

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) isOk = TryConvertToInt(valueOfUnknownType, out value);

            if ((isOk == false) && setToDefaultIfDoesNotExist) {
                isOk = TrySetInternal(key, defaultValue);
                if (isOk) value = defaultValue;
            }

            return isOk;
        }

        public bool TryGet(string key, out double value, bool setToDefaultIfDoesNotExist = false, double defaultValue = 0) {
            bool isOk;
            value = 0;

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) isOk = TryConvertToDouble(valueOfUnknownType, out value);

            if ((isOk == false) && setToDefaultIfDoesNotExist) {
                isOk = TrySetInternal(key, defaultValue);
                if (isOk) value = defaultValue;
            }

            return isOk;
        }

        public bool TryGet(string key, out bool value, bool setToDefaultIfDoesNotExist = false, bool defaultValue = false) {
            bool isOk;
            value = false;

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) isOk = TryConvertToBool(valueOfUnknownType, out value);

            if ((isOk == false) && setToDefaultIfDoesNotExist) {
                isOk = TrySetInternal(key, defaultValue);
                if (isOk) value = defaultValue;
            }

            return isOk;
        }


        public bool TryGet(string key, out string value, bool setToDefaultIfDoesNotExist = false, string defaultValue = null) {
            bool isOk;
            value = "";

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) isOk = TryConvertToString(valueOfUnknownType, out value);

            if ((isOk == false) && setToDefaultIfDoesNotExist) {
                isOk = TrySetInternal(key, defaultValue);
                if (isOk) value = defaultValue;
            }


            return isOk;
        }

        private bool TryGetInternal(string key, out object value) {
            lock (_dataLock) {
                return _dataCache.TryGetValue(key, out value);
            }
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