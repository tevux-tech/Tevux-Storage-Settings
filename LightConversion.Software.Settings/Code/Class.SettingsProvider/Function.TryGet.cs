namespace LightConversion.Software.Settings {
    public partial class SettingsProvider {
        public bool TryGet(string key, out int value) {
            bool isOk;
            value = 0;

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) isOk = TryConvert(valueOfUnknownType, out value);

            return isOk;
        }

        public bool TryGet(string key, out double value) {
            bool isOk;
            value = 0;

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) isOk = TryConvert(valueOfUnknownType, out value);

            return isOk;
        }

        public bool TryGet(string key, out bool value) {
            bool isOk;
            value = false;

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) isOk = TryConvert(valueOfUnknownType, out value);

            return isOk;
        }

        public bool TryGet(string key, out string value) {
            bool isOk;
            value = "";

            isOk = TryGetInternal(key, out var valueOfUnknownType);
            if (isOk) isOk = TryConvert(valueOfUnknownType, out value);

            return isOk;
        }
    }
}