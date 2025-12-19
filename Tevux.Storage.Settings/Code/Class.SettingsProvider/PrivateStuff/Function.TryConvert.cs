namespace Tevux.Storage.Settings;

public partial class SettingsProvider {
    private static bool TryConvert(object valueToConvert, out float convertedValue) {
        var isOk = true;

        // Reading a number and getting its type is not straight forward: "1" can be an int, float, double, whatever.
        // What's crazy that this depends on the system the same exact code runs on! I don't know why.
        // So, I've got to deal with all options.
        // I think I saw somewhere in Newtonsoft code that they prefer Int64 in those cases.
        switch (valueToConvert) {
            case int valueAsInt:
                convertedValue = valueAsInt;
                break;

            case double valueAsDouble:
                // If this lib is used to store the number, then there will be no overflow.
                convertedValue = (float)valueAsDouble;
                break;

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
