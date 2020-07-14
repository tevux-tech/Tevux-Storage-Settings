using System;

namespace LightConversion.Storage.Settings {
    public partial class SettingsProvider {
        private void HandleNonCriticalError(string message, string source = "", Exception innerException = null) {
            var fullMessage = message;
            if (innerException != null) {
                fullMessage += "\r\n\r\n Original exception:\r\n";
                var tempException = innerException;
                while (tempException != null) {
                    fullMessage += tempException.Message;
                    tempException = tempException.InnerException;
                }
            }

            ErrorOccurred?.Invoke(this, new GeneralEventArgs(fullMessage, source, innerException));
        }
    }
}