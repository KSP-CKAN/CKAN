# CKAN Console UI

A cross-platform text UI inspired by Borland's Turbo Vision and IBM's Common User Access circa 1990.

## Technical summary

The core framework classes are in the `ConsoleUI\Toolkit` folder, while the application classes are in the `ConsoleUI` folder.

A UI in this framework consists of a `ScreenContainer` hosting one or more `ScreenObjects`. The container handles the overall logic of the display, while the objects implement specific UI elements. These are both abstract classes, so you have to use child classes like `ConsoleScreen` and `ConsoleField`.

### Class hierarchy

Main UI classes:

```
ScreenObject
 |-- ConsoleButton
 |-- ConsoleField
 |-- ConsoleFrame
 |-- ConsoleLabel
 |-- ConsoleListBox<T>
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
ConsolePopupMenu
ConsoleUI
ConsoleCKAN
SplashScreen
```

Resource classes:

```
Symbols
Keys
ConsoleTheme
```

Helper classes:

```
Formatting
ConsoleMenuOption
ScreenTip
ChangePlan
Dependency
```

Enums:

```
InstallStatus
TextAlign
```

### `IUser`

The `IUser` interface is the main way that algorithms in `Core` interact with the user generically. They accept an `IUser` object as a parameter, and call methods on it to raise alerts or ask questions. In `CmdLine`, these calls are handled via simple `Console.WriteLine` statements, and in `GUI` various WinForms objects are created and displayed.

In `ConsoleUI`, `ConsoleScreen` is in charge of managing a whole screen, so it implements `IUser` and raises popup dialogs.

`ProgressScreen` handles the parts of `IUser` related to task completion, since these assume a visible progress bar.

## Acknowledgements

Special thanks to @HebaruSan's rl waifu for user testing.
