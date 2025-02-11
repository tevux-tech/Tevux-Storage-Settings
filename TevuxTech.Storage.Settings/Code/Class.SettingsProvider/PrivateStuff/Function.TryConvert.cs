namespace TevuxTech.Storage.Settings;

public partial class SettingsProvider {
    private static bool TryConvert(object valueToConvert, out int convertedValue) {
        var isOk = true;

        switch (valueToConvert) {
            case int valueAsInt:
                convertedValue = valueAsInt;
                break;

            case long valueAsLong:
                convertedValue = (int)valueAsLong;
                break;

            case double valueAsDouble:
                convertedValue = Convert.ToInt32(valueAsDouble);
                break;

            case float valueAsFloat:
                convertedValue = Convert.ToInt32(valueAsFloat);
                break;

            default:
                convertedValue = 0;
                isOk = false;
                break;
        }

        return isOk;
    }

    private static bool TryConvert(object valueToConvert, out double convertedValue) {
        var isOk = true;

        switch (valueToConvert) {
            case double valueAsDouble:
                convertedValue = valueAsDouble;
                break;

            case float valueAsFloat:
                convertedValue = valueAsFloat;
                break;

            case int valueAsInt:
                convertedValue = valueAsInt;
                break;

            case long valueAsLong:
                convertedValue = valueAsLong;
                break;

            default:
                convertedValue = 0;
                isOk = false;
                break;
        }

        return isOk;
    }

    private static bool TryConvert(object valueToConvert, out float convertedValue) {
        var isOk = true;

        switch (valueToConvert) {
            case float valueAsFloat:
                convertedValue = valueAsFloat;
                break;

            case double valueAsDouble:
                convertedValue = (float)valueAsDouble;
                break;

            case int valueAsInt:
                convertedValue = valueAsInt;
                break;

            case long valueAsLong:
                convertedValue = valueAsLong;
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

    private static bool TryConvert(object valueToConvert, out DateTime convertedValue) {
        var isOk = true;

        switch (valueToConvert) {
            case DateTime valueAsDateTime:
                convertedValue = valueAsDateTime;
                break;

            case string valueAsString:
                try {
                    convertedValue = JsonSerializer.Deserialize<DateTime>($"\"{valueAsString}\"");
                } catch (Exception) {
                    convertedValue = DateTime.MinValue;
                    isOk = false;
                }

                break;

            default:
                convertedValue = DateTime.MinValue;
                isOk = false;
                break;
        }

        return isOk;
    }
}
