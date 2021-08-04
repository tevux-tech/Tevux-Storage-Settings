using System;
using System.IO;
using NLog;

namespace LightConversion.Storage.Settings {
    /// <summary>
    /// Class tracks file changes and rises <see cref="Changed"/> event. File changes can also happen outside application scope.
    /// </summary>
    public class WatchedReliableFile : ReliableFile {
        public event GeneralEventHandler Changed;
        private bool _isInitialized;
        private FileSystemWatcher _fileWatcher;
        private DateTime _lastWriteDate;
        private Logger _logger;

        public WatchedReliableFile(string filePath) : base(filePath) {
            // Nothing to do right now.
        }

        /// <summary>
        /// Initialize ReliableFile and start listening for file changes.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when failed to initialize object.</exception>
        public new void Initialize() {
            Initialize(LogManager.CreateNullLogger());
        }

        /// <summary>
        /// Initialize ReliableFile and start listening for file changes.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when failed to initialize object.</exception>
        public new void Initialize(Logger logger) {
            if (_isInitialized) return;

            if (logger == null) {
                throw new InvalidOperationException($"Argument {nameof(logger)} can't be null.");
            }

            base.Initialize(logger);
            _logger = logger;

            var fileDirectory = "";
            var fileName = "";
            try {
                fileName = System.IO.Path.GetFileName(Path);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to parse file name.");
                throw new InvalidOperationException("Failed to parse file name.", ex);
            }

            try {
                fileDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Path));
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to parse directory path.");
                throw new InvalidOperationException("Failed to parse directory path.", ex);
            }

            // Initialize file system watcher.
            try {
                _fileWatcher = new FileSystemWatcher(fileDirectory);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to create FileSystemWatcher.");
                throw new InvalidOperationException("Failed to create FileSystemWatcher.", ex);
            }

            _fileWatcher.Changed += HandleFileChangedEvent;
            _fileWatcher.Created += HandleFileCreatedEvent;
            _fileWatcher.Renamed += HandleFileCreatedEvent;
            _fileWatcher.Error += OnError;
            _fileWatcher.Filter = fileName;
            _fileWatcher.EnableRaisingEvents = true;
            _isInitialized = true;
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
            }
        }

        private void HandleFileCreatedEvent(object sender, FileSystemEventArgs e) {
            if (Exists() == false) return;

            // It's a new file to watch so last write date is current.
            _lastWriteDate = File.GetLastWriteTime(Path);
            Changed?.Invoke(this, new GeneralEventArgs($"Changed: {Path}"));
        }

        /// <summary>
        /// Handle internal <see cref="FileSystemWatcher"/> errors.
        /// </summary>
        private void OnError(object sender, ErrorEventArgs e) {
            _logger.Error("Error happened in FileSystemWatcher, message:" + e.GetException().Message);
        }
    }
}