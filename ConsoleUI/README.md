# CKAN Console UI

A cross-platform text UI inspired by Borland's Turbo Vision and IBM's Common User Access circa 1990.

## Caution

Not tested on MacOS X.

## Technical summary

The core framework classes are in the `ConsoleUI\Toolkit` folder, while the application classes are in the `ConsoleUI` folder.

A UI in this framework consists of a `ScreenContainer` hosting one or more `ScreenObjects`. The container handles the overall logic of the display, while the objects implement specific UI elements. These are both abstract classes, so you have to use child classes like `ConsoleScreen` and `ConsoleField`.

## Class hierarchy

```
ScreenObject
 |-- ConsoleButton
 |-- ConsoleField
 |-- ConsoleFrame
 |-- ConsoleLabel
 |-- ConsoleListBox
 |-- ConsoleProgressBar
 \-- ConsoleTextBox
ScreenContainer
 |-- ConsoleScreen
 |    |-- KSPListScreen
 |    |-- KSPScreen
 |    |    |-- KSPAddScreen
 |    |    \-- KSPEditScreen
 |    |-- ProgressScreen
 |    |    \-- InstallScreen
 |    |-- DependencyScreen
 |    |-- ModListScreen
 |    \-- ModInfoScreen
 \-- ConsoleDialog
      |-- ConsoleMessageDialog
      |-- ConsoleChoiceDialog<T>
      \-- ModListHelpDialog
Program
ConsoleCKAN
SplashScreen
```

## Acknowledgements

Special thanks to @HebaruSan's rl waifu for user testing.
