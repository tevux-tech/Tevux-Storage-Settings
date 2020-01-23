using System.Collections.Generic;

namespace LightConversion.Software.Settings {
    public partial class SettingsProvider : ISilentReporter {
        private Dictionary<string, object> _dataCache;
        private readonly object _dataLock = new object();
        public ReliableFile DataFile { get; private set; }

        public bool IsInitialized { get; private set; }

        public bool AreExceptionsSilent { get; set; }

        public event GeneralEventHandler ErrorOccurred;
        public event GeneralEventHandler InfoReady;
    }
}