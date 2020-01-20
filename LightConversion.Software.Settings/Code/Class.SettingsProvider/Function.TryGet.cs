namespace LightConversion.Software.Settings {
    public partial class SettingsProvider {
        public bool TryGet(string key, out int value, bool setToDefaultIfDoesNotExist = false, int defaultValue = 0) {
            bool isOk;
            value = 0;

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) isOk = TryConvert(valueOfUnknownType, out value);

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
            if (isOk) isOk = TryConvert(valueOfUnknownType, out value);

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
            if (isOk) isOk = TryConvert(valueOfUnknownType, out value);

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
            if (isOk) isOk = TryConvert(valueOfUnknownType, out value);

            if ((isOk == false) && setToDefaultIfDoesNotExist) {
                isOk = TrySetInternal(key, defaultValue);
                if (isOk) value = defaultValue;
            }

            return isOk;
        }
    }
}