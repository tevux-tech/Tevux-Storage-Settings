# LightConversion.Software.Settings Changelog

## [1.0.0] - 2021-08-05
### Changed
- Moved file watcher from ReliableFile class to separate class WatchedReliableFile (Case 7094).
- Moved FilePath argument form ReliableFile class constructor to Initialize() function (Case 7094).
- Changed ISilentReporter to NLog logging. Logging can be set using public property - "Logger" in all classes (Case 7094).

## [0.5.0] - 2021-06-25
### Added
- Added support for float settings (Case 7016).

## [0.4.0] - 2021-05-03
### Added
- Added ReliableFile.Changed event that rises when underlying file was changed (Case 6907).
- Added unit test to test ReliableFile.Changed event (Case 6907).

### Fixed
- Don't allow to use ReliableFile methods if initialization failed (Case 6907).
- Fixed SettingsProvider test, added missing ReliableFile initializations (Case 6907).