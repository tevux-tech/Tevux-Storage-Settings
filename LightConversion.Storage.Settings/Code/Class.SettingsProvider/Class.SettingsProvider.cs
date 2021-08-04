using System.Collections.Generic;
using NLog;

namespace LightConversion.Storage.Settings {
    public partial class SettingsProvider {
        private Dictionary<string, object> _dataCache;
        private readonly object _dataLock = new object();
        private Logger _logger;
        
        public ReliableFile DataFile { get; private set; }

        public bool IsInitialized { get; private set; }
    }
}