using System;
using System.Collections.Generic;

namespace LightConversion.Software.Settings {
    public partial class SettingsProvider {
        private Dictionary<string, object> _dataCache;
        private readonly object _dataLock = new object();
        private ReliableFile _dataFile;
        public bool IsInitialized { get; private set; }
    }
}