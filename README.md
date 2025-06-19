# Tevux-Software-Settings

For many years, we've been using `File.WriteAllText` and `File.ReadAllText` methods to store program settings in file.
However, every once in a while, when debugging client's broken systems, we find settings file completely empty. It is a
complete mystery of why this happens, but it happens nevertheless and thus we decided to build a reliable multi-stage
write process that at least guarantees us a valid settings file. Sure, if the power is cut during the write, newest
changes will be lost, but then at least original file will be restored automatically.

## ReliableFile

Let's take `settings.txt` file as an example. To read its content, use either of these methods:

```
public bool TryReadAllText(out string fileContent) 
public bool TryReadAllBytes(out byte[] fileContent) 
```

Since read operations are inherently quite safe, not much is happening behind the scenes. It is just a silent wrapper
around `File.ReadAllBytes` which does not throw any exceptions.

The story is different for write counterparts:

```
public bool TryWriteAllBytes(byte[] bytesToWrite)
public bool TryWriteAllText(string textToWrite)
```

When calling those it goes like this:

1. New content is written to `settings.txt.rf1`
2. `settings.txt.rf1` is renamed to `settings.txt.rf2`
3. `settings.txt` is deleted
4. `settings.txt.rf2` is renamed to `settings.txt`

Each step, starting from #2, validates the previous one; so, if the application crashed during file write, there are
quite a few good oppurtinities to restore the file - at least to the original content. This is done automatically when
calling `Initialize` method.

Here's a basic usage example:

```
var reliableFile = new ReliableFile();
reliableFile.Initialize("temp/someFile.txt");
if (reliableFile.TryReadAllText(out var fileContent)){
    Debug.Write(fileContent);
};
```

## SettingsProvider

This class provides dictionary-like settings storage and utilizes `ReliableFile`. It is simplistic by design. It only
allows storing very simple data types: `float`, `string` and `bool`. Also, there are no sections or categories. 
It is meant to store a handful of settings, and do that reliably.

Content is saved as JSON.

Here's how to use it:

```
// Preparing an underlying ReliableFile.
var settingsFile = new ReliableFile();
settings.Initialize(settingsFile);

// Loading that file into SettingsProvider.
var settings = new SettingsProvider();
settingsFile.Initialize("temp/someSettings.json");

// Setting and getting a value.
settings.TrySet("SomeNumberKey", 123);
settings.TryGet("SomeNumberKey", out float someIntegerValue);

// When getting a non-existent setting, you may also specify a default value.
var setting = settings.Get("SomeNonExistentKey", "Default value");
```