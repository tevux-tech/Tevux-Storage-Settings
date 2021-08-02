using System;
using System.IO;
using NLog;

namespace LightConversion.Storage.Settings {
    public class WatchedReliableFile : ReliableFile {
        public bool IsInitialized { get; private set; }
        
        private FileSystemWatcher _fileWatcher;
        private DateTime _lastWriteDate;
        public event GeneralEventHandler Changed;
        private Logger _logger;

        public WatchedReliableFile(string filePath) : base(filePath) {

        }

        /// <summary>
        /// Initialize ReliableFile and start listening for file changes.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when failed to initialize object.</exception>
        public new void Initialize(Logger logger) {
            if (IsInitialized) return;

            if (logger == null) {
                throw new InvalidOperationException($"Argument {nameof(logger)} can't be null.");
            }

            base.Initialize(_logger);
            if (base.IsInitialized == false) {
                // TODO: log here.
                return;
            }

            _logger = logger;

            string fileDirectory;
            string fileName;
            try {
                fileName = System.IO.Path.GetFileName(Path);
            }
            catch (Exception ex) {
                throw new InvalidOperationException("Failed to parse file name.", ex);
            }

            try {
                fileDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Path));
            }
            catch (Exception ex) {
                throw new InvalidOperationException("Failed to parse directory path.", ex);
            }

            // Initialize file system watcher.
            try {
                _fileWatcher = new FileSystemWatcher(fileDirectory);
            }
            catch (Exception ex) {
                throw new InvalidOperationException("Failed to create FileSystemWatcher.", ex);
            }

            _fileWatcher.Changed += HandleFileChangedEvent;
            _fileWatcher.Created += HandleFileCreatedEvent;
            _fileWatcher.Renamed += HandleFileCreatedEvent;
            _fileWatcher.Error += OnError;
            _fileWatcher.Filter = fileName;
            _fileWatcher.EnableRaisingEvents = true;
            IsInitialized = true;
        }
        
        
        public new bool TryWriteAllBytes(byte[] bytesToWrite) {
            if (IsInitialized == false) {
                // TODO: log.error ("Object is not initialized or failed to initialize.", $"Function {nameof(TryWriteAllBytes)}()");
                return false;
            }

            // Let's not rise multiple Changed event with single write operation. Disabling watcher for now.
            if (_fileWatcher != null) _fileWatcher.EnableRaisingEvents = false;

            var writeOperationResult = base.TryWriteAllBytes(bytesToWrite);

            // Make sure to reenable watcher before leaving method.
            if ((_fileWatcher != null) && (_fileWatcher.EnableRaisingEvents == false)) {
                _fileWatcher.EnableRaisingEvents = true;
            }

            return writeOperationResult;
        }

        private void HandleFileChangedEvent(object sender, FileSystemEventArgs e) {
            if (Exists() == false) return;

            // When file is edited it rises multiple Changed events. Let's only rise Changed event when file content changes. 
            var isChanged = true;
            if (e.ChangeType == WatcherChangeTypes.Changed) {
                var newLastWriteDate = File.GetLastWriteTime(Path);
                if (_lastWriteDate == newLastWriteDate) isChanged = false;
                else _lastWriteDate = newLastWriteDate;
            }

            if (isChanged) {
                Changed?.Invoke(this, new GeneralEventArgs($"Changed: {Path}"));
                // TODO: log.info(this, new GeneralEventArgs($"Changed: {Path}"));
            }
        }

        private void HandleFileCreatedEvent(object sender, FileSystemEventArgs e) {
            if (Exists() == false) return;

            // It's a new file to watch so last write date is current.
            _lastWriteDate = File.GetLastWriteTime(Path);
            // TODO: log.info(this, new GeneralEventArgs($"Changed: {Path}"));
            Changed?.Invoke(this, new GeneralEventArgs($"Changed: {Path}"));
        }

        private void OnError(object sender, ErrorEventArgs e) {
            // TODO: log.error ("Error happened in FileSystemWatcher, message:" + e.GetException().Message, $"Function {nameof(OnError)}()");
        }
    }
}