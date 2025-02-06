using System.Diagnostics.CodeAnalysis;

namespace LightConversion.Storage.Settings;

[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Defending against unknown filesystem exception. They are logged, so it is fine.")]
public partial class SettingsProvider {
    private readonly object _dataLock = new();
    private readonly ILogger _logger;
    private Dictionary<string, object> _dataCache;

    public SettingsProvider(ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<SettingsProvider>();
    }

    public ReliableFile DataFile { get; private set; }
    public bool IsInitialized { get; private set; }
}
