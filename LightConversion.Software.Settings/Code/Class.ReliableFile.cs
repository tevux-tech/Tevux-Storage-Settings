using System.IO;

namespace LightConversion.Software.Settings {
    public class ReliableFile {
        private enum FileHealth {
            Intact,
            Recoverable,
            Littered,
            Unrecoverable
        }

        private readonly string _rf1FilePath;
        private readonly string _rf2FilePath;
        private readonly object _lock = new object();
        public string Path { get; }

        public ReliableFile(string filePath) {
            // Building all the file paths we'll be using in this class.
            Path = filePath;
            _rf1FilePath = filePath + ".rf1";
            _rf2FilePath = filePath + ".rf2";
        }

        public bool Initialize() {
            var returnValue = false;
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
                returnValue = true;
            }

            if (state == FileHealth.Recoverable) {
                // Something is not right, but rf2 is present, so restoring from it.
                lock (_lock) {
                    File.Delete(Path);
                    File.Delete(_rf1FilePath);
                    File.Move(_rf2FilePath, Path);
                }

                returnValue = true;
            }

            if (state == FileHealth.Littered) {
                // Last write is probably lost, but main file is still there. Just cleaning up.
                File.Delete(_rf1FilePath);
                returnValue = true;
            }

            if (state == FileHealth.Unrecoverable) {
                // rf2 file is missing, probably saving crashed at some point. rf1 file, if present, is probably corrupt. Can't do much here.
                File.Delete(_rf1FilePath);
                returnValue = true;
            }

            return returnValue;
        }

        public bool TryReadAllText(out string fileContent) {
            bool returnValue;

            fileContent = "";
            lock (_lock) {
                if (File.Exists(Path)) {
                    fileContent = File.ReadAllText(Path);
                    returnValue = true;
                } else {
                    returnValue = false;
                }
            }

            return returnValue;
        }

        public bool TryWriteAllText(string textToWrite) {
            var returnValue = false;
            lock (_lock) {
                File.WriteAllText(_rf1FilePath, textToWrite);
                File.Move(_rf1FilePath, _rf2FilePath);
                File.Delete(Path);
                File.Move(_rf2FilePath, Path);
                returnValue = true;
            }

            return returnValue;
        }

        public bool TryWriteAllBytes(byte[] bytesToWrite) {
            var returnValue = false;
            lock (_lock) {
                File.WriteAllBytes(_rf1FilePath, bytesToWrite);
                File.Move(_rf1FilePath, _rf2FilePath);
                File.Delete(Path);
                File.Move(_rf2FilePath, Path);
                returnValue = true;
            }

            return returnValue;
        }
    }
}