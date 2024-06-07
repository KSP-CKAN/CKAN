using System;
using System.IO;

using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Base class for screens adding/editing game instances
    /// </summary>
    public abstract class GameInstanceScreen : ConsoleScreen {

        /// <summary>
        /// Initialize the screen
        /// </summary>
        /// <param name="mgr">Game instance manager containing the instances, needed for saving changes</param>
        /// <param name="initName">Initial value of name field</param>
        /// <param name="initPath">Initial value of path field</param>
        protected GameInstanceScreen(GameInstanceManager mgr, string initName = "", string initPath = "") : base()
        {
            manager = mgr;

            AddTip("F2", Properties.Resources.Accept);
            AddBinding(Keys.F2, (object sender, ConsoleTheme theme) => {
                if (Valid()) {
                    Save();
                    // Close screen
                    return false;
                } else {
                    // Keep running the screen
                    return true;
                }
            });

            AddTip(Properties.Resources.Esc, Properties.Resources.Cancel);
            AddBinding(Keys.Escape, (object sender, ConsoleTheme theme) => {
                // Discard changes
                return false;
            });

            name = new ConsoleField(labelWidth, nameRow, -1, initName) {
                GhostText = () => Properties.Resources.InstanceNameGhostText
            };
            path = new ConsoleField(labelWidth, pathRow, -1, initPath) {
                GhostText = () => Properties.Resources.InstancePathGhostText
            };

            AddObject(new ConsoleLabel(1, nameRow, labelWidth, () => Properties.Resources.InstanceNameLabel));
            AddObject(name);
            AddObject(new ConsoleLabel(1, pathRow, labelWidth, () => Properties.Resources.InstancePathLabel));
            AddObject(path);
        }

        /// <summary>
        /// Put CKAN 1.25.5 in top left corner
        /// </summary>
        protected override string LeftHeader()
        {
            return $"{Meta.GetProductName()} {Meta.GetVersion()}";
        }

        /// <summary>
        /// Return whether the fields currently are valid.
        /// Abstract because the details depend on whether we're adding or editing.
        /// </summary>
        protected abstract bool Valid();

        /// <summary>
        /// Save the fields.
        /// Abstract because the details depend on whether we're adding or editing.
        /// </summary>
        protected abstract void Save();

        /// <summary>
        /// Return whether the name field is non-empty and unique
        /// </summary>
        protected bool nameValid()
        {
            if (string.IsNullOrEmpty(name.Value)) {
                // Complain about empty name
                RaiseError(Properties.Resources.InstanceNameEmptyError);
                SetFocus(name);
                return false;
            } else if (manager.HasInstance(name.Value)) {
                // Complain about duplicate name
                RaiseError(Properties.Resources.InstanceNameDuplicateError, name.Value);
                SetFocus(name);
                return false;
            } else {
                return true;
            }
        }

        /// <summary>
        /// Return whether the path is valid
        /// </summary>
        protected bool pathValid()
        {
            if (Platform.IsMac) {
                // Handle default path dragged-and-dropped onto Mac's Terminal
                path.Value = path.Value.Replace("Kerbal\\ Space\\ Program", "Kerbal Space Program");
            }
            try
            {
                // DirectoryInfo's constructor throws exceptions for invalid characters, empty strings, etc.
                if (GameInstanceManager.IsGameInstanceDir(new DirectoryInfo(path.Value))) {
                    return true;
                }
            } catch {
                // Pretend DirectoryInfo constructed an instance that made IsGameInstanceDir return false
            }
            // Complain about non-KSP path
            RaiseError(Properties.Resources.InstancePathNotGameFolderError);
            SetFocus(path);
            return false;
        }

        /// <summary>
        /// ScreenObject that edits the name
        /// </summary>
        protected ConsoleField name;
        /// <summary>
        /// ScreenObject that edits the path
        /// </summary>
        protected ConsoleField path;

        /// <summary>
        /// Game instance manager object that contains the instances
        /// </summary>
        protected GameInstanceManager manager;

        /// <summary>
        /// Number of columns reserved at left of screen for labels
        /// </summary>
        protected int labelWidth => Math.Max(24, Math.Max(
            Properties.Resources.InstanceNameLabel.Length,
            Properties.Resources.InstancePathLabel.Length
        ));
        private   const int nameRow    = 2;
        /// <summary>
        /// Y coordinate of path field
        /// </summary>
        protected const int pathRow    = 4;
    }

}
