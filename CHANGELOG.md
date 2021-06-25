# LightConversion.Software.Settings Changelog

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