using System;
using System.IO;
using System.Threading.Tasks;

namespace LightConversion.Software.Settings {
    public class Test {
        public void Main() {
            Console.WriteLine("Hello World!");
            var bybis = new ReliableFile("bybis.txt");
            bybis.Initialize();
            Task.Run(async () => {
                while (true) {
                    var isAllGood = bybis.TryWriteAllText("Bybis");
                    Console.WriteLine($"Writing bybis, allgood=={isAllGood}");
                    await Task.Delay(TimeSpan.FromMilliseconds(10));
                }
            });
            Task.Run(async () => {
                while (true) {
                    var isAllGood = bybis.TryWriteAllText("Papai");
                    Console.WriteLine($"Writing papai, allgood=={isAllGood}");
                    await Task.Delay(TimeSpan.FromMilliseconds(15));
                }
            });
            Task.Run(async () => {
                while (true) {
                    var isAllGood = bybis.TryReadAllText(out var content);
                    Console.WriteLine($"Reading {content}, allgood=={isAllGood}");
                    if ((content != "Bybis") && (content != "Papai")) {
                        var pzdc = 1;
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(7));
                }
            });
            Console.ReadLine();
        }
    }

    public class ReliableFile {
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
            var state = "intact";
            if ((mainFileExists == true) && (rf1FileExists == false) && (rf2FileExists == false)) state = "intact";
            if ((mainFileExists == true) && (rf1FileExists == false) && (rf2FileExists == true)) state = "recoverable";
            if ((mainFileExists == true) && (rf1FileExists == true) && (rf2FileExists == false)) state = "littered";
            if ((mainFileExists == true) && (rf1FileExists == true) && (rf2FileExists == true)) state = "recoverable";
            if ((mainFileExists == false) && (rf1FileExists == false) && (rf2FileExists == false)) state = "unrecoverable";
            if ((mainFileExists == false) && (rf1FileExists == false) && (rf2FileExists == true)) state = "recoverable";
            if ((mainFileExists == false) && (rf1FileExists == true) && (rf2FileExists == false)) state = "unrecoverable";
            if ((mainFileExists == false) && (rf1FileExists == true) && (rf2FileExists == true)) state = "recoverable";
            // Taking action to fix file, if issues present.
            if (state == "intact") {
                // All good, file structure is intact.
                returnValue = true;
            }

            if (state == "recoverable") {
                // Something is not right, but rf2 is present, so restoring from it.
                lock (_lock) {
                    File.Delete(Path);
                    File.Delete(_rf1FilePath);
                    File.Move(_rf2FilePath, Path);
                }

                returnValue = true;
            }

            if (state == "littered") {
                // Last write is probably lost, but main file is still there. Just cleaning up.
                File.Delete(_rf1FilePath);
                returnValue = true;
            }

            if (state == "unrecoverable") {
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
    }
}