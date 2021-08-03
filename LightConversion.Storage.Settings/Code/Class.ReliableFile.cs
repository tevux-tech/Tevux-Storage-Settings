using System;
using System.IO;
using System.Text;
using NLog;

namespace LightConversion.Storage.Settings {
    public class ReliableFile {
        private bool _isInitialized;
        public string Path { get; }
        public ReliableFile(string filePath) {
            // Building all the file paths we'll be using in this class.
            Path = filePath;
            _rf1FilePath = filePath + ".rf1";
            _rf2FilePath = filePath + ".rf2";
        }

        private enum FileHealth {
            Intact,
            Recoverable,
            Littered,
            Unrecoverable
        }
        private readonly string _rf1FilePath;
        private readonly string _rf2FilePath;
        private readonly object _lock = new object();
        private Logger _logger;

        /// <summary>
        /// Initialize ReliableFile object. Try to recover file if last write operation failed. Start listening for file changes.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when failed to initialize object.</exception>
        public void Initialize() {
            Initialize(LogManager.CreateNullLogger());
        }

        /// <summary>
        /// Initialize ReliableFile object. Try to recover file if last write operation failed.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when failed to initialize object.</exception>
        public void Initialize(Logger logger) {
            if (_isInitialized) return;

            if (logger == null) {
                throw new InvalidOperationException($"Argument {nameof(logger)} can't be null.");
            }

            _logger = logger;

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
            }
            else if (state == FileHealth.Recoverable) {
                // Something is not right, but rf2 is present, so restoring from it.
                _logger.Warn($"Recovering from \"{_rf2FilePath}\".");
                lock (_lock) {
                    try {
                        File.Delete(Path);
                        File.Delete(_rf1FilePath);
                        File.Move(_rf2FilePath, Path);
                    }
                    catch (IOException ex) {
                        _logger.Error(ex, $"File recovery from \"{_rf1FilePath}\" failed because of IOException.");
                    }
                }
            }
            else if (state == FileHealth.Littered) {
                // Last write is probably lost, but main file is still there. Just cleaning up.
                _logger.Warn($"Recovering from \"{_rf2FilePath}\". Last write operation is probably lost.");
                try {
                    File.Delete(_rf1FilePath);
                }
                catch (IOException ex) {
                    _logger.Error(ex, $"Deleting temporary \"{_rf1FilePath}\" leftover failed because of IOException.");
                }
            }
            else if (state == FileHealth.Unrecoverable) {
                // rf2 file is missing, probably saving crashed at some point. rf1 file, if present, is probably corrupt. Can't do much here.
                _logger.Error("File is unrecoverable. Probably last saving crashed at some point.");

                try {
                    File.Delete(_rf1FilePath);
                }
                catch (Exception ex) {
                    _logger.Error(ex, $"Deleting temporary \"{_rf1FilePath}\" leftover failed because of Exception.");
                }
            }

            _isInitialized = true;
        }

        public bool Exists() {
            if (_isInitialized == false) {
                _logger.Error("Object is not initialized or failed to initialize.");
                return false;
            }

            return File.Exists(Path);
        }

        public bool TryReadAllText(out string fileContent) {
            if (_isInitialized == false) {
                _logger.Error("Object is not initialized or failed to initialize.");
                fileContent = "";
                return false;
            }

            var isOk = TryReadAllBytes(out var fileBytes);

            if (isOk) fileContent = Encoding.UTF8.GetString(fileBytes);
            else fileContent = "";

            return isOk;
        }

        public bool TryReadAllBytes(out byte[] fileContent) {
            if (_isInitialized == false) {
                _logger.Error("Object is not initialized or failed to initialize.");
                fileContent = new byte[0];
                return false;
            }

            bool returnValue;
            fileContent = new byte[0];
            lock (_lock) {
                if (File.Exists(Path)) {
                    try {
                        fileContent = File.ReadAllBytes(Path);
                        returnValue = true;
                    }
                    catch (IOException ex) {
                        _logger.Error(ex, "Reading file failed because of IOException.");
                        returnValue = false;
                    }
                }
                else {
                    _logger.Error("Reading file failed because it doesn't exist.");
                    returnValue = false;
                }
            }

            return returnValue;
        }

        public bool TryWriteAllText(string textToWrite) {
            if (_isInitialized == false) {
                _logger.Error("Object is not initialized or failed to initialize.");
                return false;
            }

            return TryWriteAllBytes(Encoding.UTF8.GetBytes(textToWrite));
        }

        public bool TryWriteAllBytes(byte[] bytesToWrite) {
            if (_isInitialized == false) {
                _logger.Error("Object is not initialized or failed to initialize.");
                return false;
            }

            bool returnValue;

            lock (_lock) {
                try {
                    File.WriteAllBytes(_rf1FilePath, bytesToWrite);
                    File.Move(_rf1FilePath, _rf2FilePath);
                    File.Delete(Path);
                    File.Move(_rf2FilePath, Path);
                    returnValue = true;
                }
                catch (IOException ex) {
                    _logger.Error(ex, "Writing to file failed because of IOException.");
                    returnValue = false;
                }
            }

            return returnValue;
        }
    }
}