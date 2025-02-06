namespace LightConversion.Storage.Settings;

public partial class SettingsProvider {
    private readonly object _dataLock = new();
    private Dictionary<string, object> _dataCache;
    private readonly ILogger _logger;
    public ReliableFile DataFile { get; private set; }
    public bool IsInitialized { get; private set; }

    public SettingsProvider(ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<SettingsProvider>();
    }
}
