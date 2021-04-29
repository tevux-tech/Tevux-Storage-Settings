using System;
using System.IO;
using System.Text;

namespace LightConversion.Storage.Settings {
    public class ReliableFile : ISilentReporter {
        private enum FileHealth {
            Intact,
            Recoverable,
            Littered,
            Unrecoverable
        }

        private readonly string _rf1FilePath;
        private readonly string _rf2FilePath;
        private readonly object _lock = new object();
        private bool _isInitialized;
        private DateTime _lastWriteDate;
        public string Path { get; }
        public bool AreExceptionsSilent { get; set; }
        private FileSystemWatcher _fileWatcher;
        public event GeneralEventHandler ErrorOccurred;
        public event GeneralEventHandler InfoReady;
        public event GeneralEventHandler Changed;

        public ReliableFile(string filePath) {
            // Building all the file paths we'll be using in this class.
            Path = filePath;
            _rf1FilePath = filePath + ".rf1";
            _rf2FilePath = filePath + ".rf2";
        }

        public bool Initialize() {
            if (_isInitialized) return true;

            var isOk = false;
            var mainFileExists = false;
            var rf1FileExists = false;
            var rf2FileExists = false;

            // Checking what is on the disk.
            lock (_lock) {
                mainFileExists = File.Exists(Path);
                rf1FileExists = File.Exists(_rf1FilePath);
                rf2FileExists = File.Exists(_rf2FilePath);
            }

            // Assessing situation, depending on files present.
            var state = FileHealth.Intact;
            if ((mainFileExists == true) && (rf1FileExists == false) && (rf2FileExists == false)) state = FileHealth.Intact;
            if ((mainFileExists == true) && (rf1FileExists == false) && (rf2FileExists == true)) state = FileHealth.Recoverable;
            if ((mainFileExists == true) && (rf1FileExists == true) && (rf2FileExists == false)) state = FileHealth.Littered;
            if ((mainFileExists == true) && (rf1FileExists == true) && (rf2FileExists == true)) state = FileHealth.Recoverable;
            if ((mainFileExists == false) && (rf1FileExists == false) && (rf2FileExists == false)) state = FileHealth.Unrecoverable;
            if ((mainFileExists == false) && (rf1FileExists == false) && (rf2FileExists == true)) state = FileHealth.Recoverable;
            if ((mainFileExists == false) && (rf1FileExists == true) && (rf2FileExists == false)) state = FileHealth.Unrecoverable;
            if ((mainFileExists == false) && (rf1FileExists == true) && (rf2FileExists == true)) state = FileHealth.Recoverable;

            // Taking action to fix file, if issues present.
            if (state == FileHealth.Intact) {
                // All good, file structure is intact.
                isOk = true;
            } else if (state == FileHealth.Recoverable) {
                // Something is not right, but rf2 is present, so restoring from it.
                HandleInfoReady($"Warning. Recovering from \"{_rf2FilePath}\"", "Function Initialize()");

                lock (_lock) {
                    try {
                        File.Delete(Path);
                        File.Delete(_rf1FilePath);
                        File.Move(_rf2FilePath, Path);
                        isOk = true;
                    } catch (IOException ex) {
                        HandleNonCriticalError($"File recovery from \"{_rf1FilePath}\" failed because of IOException.", "Function Initialize()", ex);
                        isOk = false;
                    }
                }
            } else if (state == FileHealth.Littered) {
                // Last write is probably lost, but main file is still there. Just cleaning up.
                HandleInfoReady($"Warning. Recovering from \"{_rf2FilePath}\". Last write operation is probably lost.", "Function Initialize()");

                try {
                    File.Delete(_rf1FilePath);
                    isOk = true;
                } catch (IOException ex) {
                    HandleNonCriticalError($"Deleting temporary \"{_rf1FilePath}\" leftover failed because of IOException.", "Function Initialize()", ex);
                    isOk = false;
                }
            } else if (state == FileHealth.Unrecoverable) {
                // rf2 file is missing, probably saving crashed at some point. rf1 file, if present, is probably corrupt. Can't do much here.
                HandleNonCriticalError("File is unrecoverable. Probably last saving crashed at some point.", "Function Initialize()");

                try {
                    File.Delete(_rf1FilePath);
                    isOk = true;
                } catch (IOException ex) {
                    HandleNonCriticalError($"Deleting temporary \"{_rf1FilePath}\" leftover failed because of IOException.", "Function Initialize()", ex);
                    isOk = false;
                } catch (Exception ex) {
                    HandleNonCriticalError($"Deleting temporary \"{_rf1FilePath}\" leftover failed because of Exception.", "Function Initialize()", ex);
                    isOk = false;
                }
            }

            // Initialize file system watcher.
            try {
                var fileDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Path));
                if (string.IsNullOrEmpty(fileDirectory) == false) {
                    var fileName = System.IO.Path.GetFileName(Path);
                    _fileWatcher = new FileSystemWatcher(fileDirectory);
                    _fileWatcher.Changed += HandleFileChangedEvent;
                    _fileWatcher.Created += HandleFileCreatedEvent;
                    _fileWatcher.Renamed += HandleFileCreatedEvent;
                    _fileWatcher.Error += OnError;
                    _fileWatcher.Filter = fileName;
                    _fileWatcher.EnableRaisingEvents = true;
                } else {
                    HandleNonCriticalError("Failed to create FileSystemWatcher because can't get directory name from path: " + Path);
                }
            } catch (Exception ex) {
                HandleNonCriticalError("Error while initializing file system watcher, error message:" + ex.Message);
            }

            _isInitialized = isOk;
            return isOk;
        }

        public bool Exists() {
            if (_isInitialized == false) throw new InvalidOperationException("Object is not initialized. Call Initialize(...) method first.");
            return File.Exists(Path);
        }

        public bool TryReadAllText(out string fileContent) {
            if (_isInitialized == false) throw new InvalidOperationException("Object is not initialized. Call Initialize(...) method first.");
            var isOk = TryReadAllBytes(out var fileBytes);

            if (isOk) fileContent = Encoding.UTF8.GetString(fileBytes);
            else fileContent = "";

            return isOk;
        }

        public bool TryReadAllBytes(out byte[] fileContent) {
            if (_isInitialized == false) throw new InvalidOperationException("Object is not initialized. Call Initialize(...) method first.");
            bool returnValue;

            fileContent = new byte[0];
            lock (_lock) {
                if (File.Exists(Path)) {
                    try {
                        fileContent = File.ReadAllBytes(Path);
                        returnValue = true;
                    } catch (IOException ex) {
                        HandleNonCriticalError("Reading file failed because of IOException.", "Function TryReadAllBytes()", ex);
                        returnValue = false;
                    }
                } else {
                    HandleNonCriticalError("Reading file failed because it doesn't exist.", "Function TryReadAllBytes()");
                    returnValue = false;
                }
            }

            return returnValue;
        }

        public bool TryWriteAllText(string textToWrite) {
            if (_isInitialized == false) throw new InvalidOperationException("Object is not initialized. Call Initialize(...) method first.");
            return TryWriteAllBytes(Encoding.UTF8.GetBytes(textToWrite));
        }

        public bool TryWriteAllBytes(byte[] bytesToWrite) {
            if (_isInitialized == false) throw new InvalidOperationException("Object is not initialized. Call Initialize(...) method first.");
            bool returnValue;

            lock (_lock) {
                try {
                    // Let's not rise multiple Changed event with single write operation. Disabling watcher for now.
                    if (_fileWatcher != null) _fileWatcher.EnableRaisingEvents = false;

                    File.WriteAllBytes(_rf1FilePath, bytesToWrite);
                    File.Move(_rf1FilePath, _rf2FilePath);
                    File.Delete(Path);

                    // Enabling watcher before last file operation so we get only one Changed event.
                    if (_fileWatcher != null) _fileWatcher.EnableRaisingEvents = true;
                    File.Move(_rf2FilePath, Path);
                    returnValue = true;
                } catch (IOException ex) {
                    HandleNonCriticalError("Writing to file failed because of IOException.", "Function TryWriteAllBytes()", ex);
                    returnValue = false;
                } finally {
                    // Make sure to reenable watcher before leaving method.
                    if ((_fileWatcher != null) && (_fileWatcher.EnableRaisingEvents == false)) {
                        _fileWatcher.EnableRaisingEvents = true;
                    }
                }
            }

            return returnValue;
        }

        private void HandleInfoReady(string message, string source = "", object additionalInfo = null) {
            InfoReady?.Invoke(this, new GeneralEventArgs(message, source, additionalInfo));
        }

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
                HandleInfoReady($"{e.ChangeType}: {Path}");
                Changed?.Invoke(this, new GeneralEventArgs($"Changed: {Path}"));
            }
        }

        private void HandleFileCreatedEvent(object sender, FileSystemEventArgs e) {
            if (Exists() == false) return;

            // It's a new file to watch so last write date is current.
            _lastWriteDate = File.GetLastWriteTime(Path);
            HandleInfoReady($"{e.ChangeType}: {Path}");
            Changed?.Invoke(this, new GeneralEventArgs($"Changed: {Path}"));
        }

        private void OnError(object sender, ErrorEventArgs e) {
            HandleNonCriticalError("Error happened in FileSystemWatcher, message:" + e.GetException().Message);
        }
    }
}