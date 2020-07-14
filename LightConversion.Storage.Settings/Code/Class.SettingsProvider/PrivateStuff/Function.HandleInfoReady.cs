namespace LightConversion.Storage.Settings {
    public partial class SettingsProvider {
        private void HandleInfoReady(string message, string source = "") {
            InfoReady?.Invoke(this, new GeneralEventArgs(message, source));
        }
    }
}