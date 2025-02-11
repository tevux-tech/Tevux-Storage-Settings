# TevuxTech.Software.Settings Changelog

## [2.0.1] - 2025-02-11
### Changed
- No functional changes, just optimizations.


## [2.0.0] - 2022-09-29
### Changed
- Moving to NET6, and applying some modern code style.

### Removed
- Deleting WatchedReliableFile (and related unit tests), because it serves no purpose and is not reliable at all.


## [1.0.1] - 2022-06-10
### Changed
- No actual changes. Added MIT licence and created github action workflow.

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