namespace Tevux.Storage.Settings;

/// <summary>
/// Class executes file read/write operations reliably. If system fails at write operation original file content will be restored.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Defending against unknown filesystem exception. They are logged, so it is fine.")]
public class ReliableFile {
    private readonly object _lock = new();
    private readonly ILogger _logger;
    private bool _isInitialized;
    private string _rf1FilePath;
    private string _rf2FilePath;

    public ReliableFile(ILogger logger) {
        _logger = logger;
    }

    public string Path { get; private set; }

    public bool Exists() {
        if (_isInitialized == false) {
            _logger.LogError("Object is not initialized or failed to initialize.");
            return false;
        }

        return File.Exists(Path);
    }

    /// <summary>
    /// Initialize ReliableFile object. Try to recover file if last write operation failed.
    /// </summary>
    public void Initialize(string filePath) {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentOutOfRangeException.ThrowIfEqual(filePath, "");

        if (_isInitialized) { return; }

        Path = filePath;
        _rf1FilePath = filePath + ".rf1";
        _rf2FilePath = filePath + ".rf2";

        bool mainFileIsPresent;
        bool rf1FileIsPresent;
        bool rf2FileIsPresent;

        // Checking what is on the disk.
        lock (_lock) {
            mainFileIsPresent = File.Exists(Path);
            rf1FileIsPresent = File.Exists(_rf1FilePath);
            rf2FileIsPresent = File.Exists(_rf2FilePath);
        }

        var mainFileIsMissing = !mainFileIsPresent;
        var rf1FileIsMissing = !rf1FileIsPresent;
        var rf2FileIsMissing = !rf2FileIsPresent;

        // Assessing situation, depending on files present.
        var state = FileHealth.Intact;
        if (mainFileIsPresent && rf1FileIsMissing && rf2FileIsMissing) { state = FileHealth.Intact; }

        if (mainFileIsPresent && rf1FileIsMissing && rf2FileIsPresent) { state = FileHealth.Recoverable; }

        if (mainFileIsPresent && rf1FileIsPresent && rf2FileIsMissing) { state = FileHealth.Littered; }

        if (mainFileIsPresent && rf1FileIsPresent && rf2FileIsPresent) { state = FileHealth.Recoverable; }

        if (mainFileIsMissing && rf1FileIsMissing && rf2FileIsMissing) { state = FileHealth.Unrecoverable; }

        if (mainFileIsMissing && rf1FileIsMissing && rf2FileIsPresent) { state = FileHealth.Recoverable; }

        if (mainFileIsMissing && rf1FileIsPresent && rf2FileIsMissing) { state = FileHealth.Unrecoverable; }

        if (mainFileIsMissing && rf1FileIsPresent && rf2FileIsPresent) { state = FileHealth.Recoverable; }

        // Taking action to fix file, if issues present.
        switch (state) {
            case FileHealth.Intact:
                // All good, file structure is intact.
                break;

            case FileHealth.Recoverable: {
                // Something is not right, but rf2 is present, so restoring from it.
                _logger.LogWarning($"Recovering from \"{_rf2FilePath}\".");
                lock (_lock) {
                    try {
                        File.Delete(Path);
                        File.Delete(_rf1FilePath);
                        File.Move(_rf2FilePath, Path);
                    } catch (IOException ex) {
                        _logger.LogError(ex, $"File recovery from \"{_rf1FilePath}\" failed because of IOException.");
                    }
                }

                break;
            }
            case FileHealth.Littered:
                // Last write is probably lost, but main file is still there. Just cleaning up.
                _logger.LogWarning($"Recovering from \"{_rf2FilePath}\". Last write operation is probably lost.");
                try {
                    File.Delete(_rf1FilePath);
                } catch (IOException ex) {
                    _logger.LogError(ex, $"Deleting temporary \"{_rf1FilePath}\" leftover failed because of IOException.");
                }

                break;

            case FileHealth.Unrecoverable:
                // rf2 file is missing, probably saving crashed at some point. rf1 file, if present, is probably corrupt. Can't do much here.
                _logger.LogError("File is unrecoverable. Probably last saving crashed at some point.");

                try {
                    File.Delete(_rf1FilePath);
                } catch (Exception ex) {
                    _logger.LogError(ex, $"Deleting temporary \"{_rf1FilePath}\" leftover failed because of Exception.");
                }

                break;
        }

        _isInitialized = true;
    }

    public bool TryReadAllBytes(out byte[] fileContent) {
        if (_isInitialized == false) {
            _logger.LogError("Object is not initialized or failed to initialize.");
            fileContent = [];
            return false;
        }

        bool returnValue;
        fileContent = [];
        lock (_lock) {
            if (File.Exists(Path)) {
                try {
                    fileContent = File.ReadAllBytes(Path);
                    returnValue = true;
                } catch (IOException ex) {
                    _logger.LogError(ex, "Reading file failed because of IOException.");
                    returnValue = false;
                } catch (Exception ex) {
                    _logger.LogError(ex, "Reading file failed because of general Exception.");
                    returnValue = false;
                }
            } else {
                _logger.LogError("Reading file failed because it doesn't exist.");
                returnValue = false;
            }
        }

        return returnValue;
    }

    public bool TryReadAllText(out string fileContent) {
        if (_isInitialized == false) {
            _logger.LogError("Object is not initialized or failed to initialize.");
            fileContent = "";
            return false;
        }

        var isOk = TryReadAllBytes(out var fileBytes);

        if (isOk) {
            fileContent = Encoding.UTF8.GetString(fileBytes);
        } else {
            fileContent = "";
        }

        return isOk;
    }

    public bool TryWriteAllBytes(byte[] bytesToWrite) {
        if (_isInitialized == false) {
            _logger.LogError("Object is not initialized or failed to initialize.");
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
            } catch (IOException ex) {
                _logger.LogError(ex, "Writing to file failed because of IOException.");
                returnValue = false;
            } catch (Exception ex) {
                _logger.LogError(ex, "Writing to file failed because of general Exception.");
                returnValue = false;
            }
        }

        return returnValue;
    }

    public bool TryWriteAllText(string textToWrite) {
        if (_isInitialized == false) {
            _logger.LogError("Object is not initialized or failed to initialize.");
            return false;
        }

        return TryWriteAllBytes(Encoding.UTF8.GetBytes(textToWrite));
    }

    private enum FileHealth {
        Intact,
        Recoverable,
        Littered,
        Unrecoverable
    }
}
