namespace LightConversion.Storage.Settings;

public partial class SettingsProvider {
    private bool TryConvert(object valueToConvert, out int convertedValue) {
        var isOk = true;

        if (valueToConvert is int valueAsInt) {
            convertedValue = valueAsInt;
        }
        else if (valueToConvert is long valueAsLong) {
            convertedValue = (int)valueAsLong;
        }
        else if (valueToConvert is double valueAsDouble) {
            convertedValue = Convert.ToInt32(valueAsDouble);
        }
        else if (valueToConvert is float valueAsFloat) {
            convertedValue = Convert.ToInt32(valueAsFloat);
        }
        else {
            convertedValue = 0;
            isOk = false;
        }

        return isOk;
    }

    private bool TryConvert(object valueToConvert, out double convertedValue) {
        var isOk = true;

        if (valueToConvert is double valueAsDouble) {
            convertedValue = valueAsDouble;
        }
        else if (valueToConvert is float valueAsFloat) {
            convertedValue = valueAsFloat;
        }
        else if (valueToConvert is int valueAsInt) {
            convertedValue = valueAsInt;
        }
        else if (valueToConvert is long valueAsLong) {
            convertedValue = valueAsLong;
        }
        else {
            convertedValue = 0;
            isOk = false;
        }

        return isOk;
    }

    private bool TryConvert(object valueToConvert, out float convertedValue) {
        var isOk = true;

        if (valueToConvert is float valueAsFloat) {
            convertedValue = valueAsFloat;
        }
        else if (valueToConvert is double valueAsDouble) {
            convertedValue = (float)valueAsDouble;
        }
        else if (valueToConvert is int valueAsInt) {
            convertedValue = valueAsInt;
        }
        else if (valueToConvert is long valueAsLong) {
            convertedValue = valueAsLong;
        }
        else {
            convertedValue = 0;
            isOk = false;
        }

        return isOk;
    }

    private bool TryConvert(object valueToConvert, out bool convertedValue) {
        bool isOk;

        if (valueToConvert is bool valueAsBool) {
            isOk = true;
            convertedValue = valueAsBool;
        }
        else {
            isOk = false;
            convertedValue = false;
        }

        return isOk;
    }

    private bool TryConvert(object valueToConvert, out string convertedValue) {
        if (valueToConvert is string valueAsString) {
            convertedValue = valueAsString;
            return true;
        }
        else {
            convertedValue = "";
            return false;
        }
    }

    private bool TryConvert(object valueToConvert, out DateTime convertedValue) {
        var isOk = true;

        if (valueToConvert is DateTime valueAsDateTime) {
            convertedValue = valueAsDateTime;
        }
        else if (valueToConvert is string valueAsString) {
            try {
                convertedValue = JsonSerializer.Deserialize<DateTime>($"\"{valueAsString}\"");
            }
            catch (Exception) {
                convertedValue = DateTime.MinValue;
                isOk = false;
            }
        }
        else {
            convertedValue = DateTime.MinValue;
            isOk = false;
        }

        return isOk;
    }
}
