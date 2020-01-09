using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LightConversion.Software.Settings {
    public class GlobalStuff {
        /// <summary>
        /// Made this for <see cref="SettingsManager"/> to compile for now.
        /// </summary>
        public static string SettingsFilePath = "bybis.txt";
    }

    /// <summary>
    /// 
    /// </summary>
    public class SettingsManager : IDisposable {
        //public Logger Log { get; } = LogManager.GetLogger("PhoebeLog.SettingsManager");

        private readonly object _dataStorageLock = new object();
        private Dictionary<string, Dictionary<string, object>> _dataStorage = new Dictionary<string, Dictionary<string, object>>();

        private bool _hasUnsavedSettings;
        private readonly CancellationTokenSource _disposingCancellationTokenSource = new CancellationTokenSource();
        private bool _isInitialized;

        public void Initialize() {
            if (_isInitialized) return;
            LoadSettingsFromFile();

            // Task that checks every 10s. for changed settings and saves them to file.
            Task.Run(async () => {
                while (_disposingCancellationTokenSource.Token.IsCancellationRequested == false) {
                    await Task.Delay(10000);

                    if (_hasUnsavedSettings) {
                        _hasUnsavedSettings = false;

                        SaveSettingsToFile();
                    }
                }
            });

            _isInitialized = true;
        }

        public void LoadSettingsFromFile() {
            try {
                lock (_dataStorageLock) {
                    // Check if settings file exits and it is not empty for some reason.
                    if (File.Exists(GlobalStuff.SettingsFilePath) && (new FileInfo(GlobalStuff.SettingsFilePath).Length != 0)) {
                        _dataStorage = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(GlobalStuff.SettingsFilePath));
                    } else {
                        // Application run for the first time?
                        // Saving some default settings.
                        _dataStorage = new Dictionary<string, Dictionary<string, object>>();
                    }

                    if (_isInitialized) {
                        //MessageBus.Default.Send(this, MessageBusTokens.SettingsChanged);
                    }
                }
            } catch (Exception ex) {
                Debug.Print(ex.ToString());
            }
        }

        /// <summary>
        /// Call method to save changed settings to file.
        /// </summary>
        public void EnableHasUnsavedSettingsFlag() {
            _hasUnsavedSettings = true;
        }

        private void SaveSettingsToFile() {
            string json;
            lock (_dataStorageLock) {
                json = JsonConvert.SerializeObject(_dataStorage, Formatting.Indented);
            }

            try {
                File.WriteAllText(GlobalStuff.SettingsFilePath, json);
            } catch (Exception) {
                // Swallowing.
            }
        }

        /// <summary>
        /// Sets any type of setting to internal dictionary. Use <see cref="Get{T}"/> to receive it latter.
        /// </summary>
        /// <param name="section">Logical section of the setting. You will need this for <see cref="Get{T}"/> operation.</param>
        /// <param name="key">Unique key in the specified section.</param>
        /// <param name="setting">Actual setting value.</param>
        public void Set<T>(string section, string key, T setting) {
            lock (_dataStorageLock) {
                if (_dataStorage.ContainsKey(section) == false) _dataStorage[section] = new Dictionary<string, object>();
                _dataStorage[section][key] = setting;

                _hasUnsavedSettings = true;
            }
        }

        /// <summary>
        /// Tries to get previously stored setting. If setting is not found or it`s type doesn't mach - default value will be stored there.
        /// </summary>
        /// <param name="section">Logical section of the setting. </param>
        /// <param name="key">Unique key in the specified section.</param>
        /// <param name="defaultValue">This value will be stored if setting is not found. Next time you try to Get - this will be returned.</param>
        /// <returns>Setting at specified section and key. If not found, default value is returned.</returns>
        public T GetAndSetIfDoesNotExist<T>(string section, string key, T defaultValue) {
            lock (_dataStorageLock) {
                if (_dataStorage.ContainsKey(section) == false) _dataStorage[section] = new Dictionary<string, object>();
                if (_dataStorage[section].ContainsKey(key) == false) {
                    _dataStorage[section][key] = defaultValue;
                    _hasUnsavedSettings = true;
                    return defaultValue;
                }

                var valueObject = _dataStorage[section][key];
                if (valueObject is T settingValue) return settingValue;

                T actualValue;
                try {
                    actualValue = JsonConvert.DeserializeObject<T>(valueObject.ToString());
                } catch (Exception) {
                    _dataStorage[section][key] = defaultValue;
                    return defaultValue;
                }

                return actualValue;
            }
        }

        /// <summary>
        /// Tries to get previously stored setting. If setting is not found or it`s type doesn't mach - default value will be returned.
        /// </summary>
        /// <param name="section">Logical section of the setting. </param>
        /// <param name="key">Unique key in the specified section.</param>
        /// <param name="defaultValue">If setting is not found at specified section/key, default value is returned.</param>
        /// <returns>Setting at specified section/key or default value if not exist.</returns>
        public T Get<T>(string section, string key, T defaultValue) {
            lock (_dataStorageLock) {
                if (_dataStorage.ContainsKey(section) == false) return defaultValue;
                if (_dataStorage[section].ContainsKey(key) == false) return defaultValue;
                var valueObject = _dataStorage[section][key];

                if (valueObject is T settingValue) return settingValue;
                try {
                    // Getting JSON value in type we expected. JSON value should be JSON-convertable to our type.
                    var actualValue = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(valueObject));
                    return actualValue;
                } catch (Exception) {
                    return defaultValue;
                }
            }
        }

        public void Dispose() {
            _disposingCancellationTokenSource.Cancel();
        }
    }
}