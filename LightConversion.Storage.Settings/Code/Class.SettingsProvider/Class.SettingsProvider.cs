namespace LightConversion.Storage.Settings;

public partial class SettingsProvider {
    private readonly object _dataLock = new();
    private Dictionary<string, object> _dataCache;
    public Logger Logger = LogManager.CreateNullLogger();
    public ReliableFile DataFile { get; private set; }
    public bool IsInitialized { get; private set; }
}
