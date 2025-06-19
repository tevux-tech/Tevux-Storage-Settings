namespace Tevux.Storage.Settings;

public partial class SettingsProvider {
    private static bool TryConvert(object valueToConvert, out float convertedValue) {
        var isOk = true;

        switch (valueToConvert) {
            case float valueAsFloat:
                convertedValue = valueAsFloat;
                break;

            default:
                convertedValue = 0;
                isOk = false;
                break;
        }

        return isOk;
    }

    private static bool TryConvert(object valueToConvert, out bool convertedValue) {
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

    private static bool TryConvert(object valueToConvert, out string convertedValue) {
        if (valueToConvert is string valueAsString) {
            convertedValue = valueAsString;
            return true;
        } else {
            convertedValue = "";
            return false;
        }
    }
}
