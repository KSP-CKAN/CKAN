using System.IO;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Base class for screens adding/editing KSP instances
    /// </summary>
    public abstract class KSPScreen : ConsoleScreen {

        /// <summary>
        /// Initialize the screen
        /// </summary>
        /// <param name="mgr">KSP manager containing the instances, needed for saving changes</param>
        /// <param name="initName">Initial value of name field</param>
        /// <param name="initPath">Initial value of path field</param>
        protected KSPScreen(KSPManager mgr, string initName = "", string initPath = "") : base()
        {
            manager = mgr;

            AddTip("F2", "Accept");
            AddBinding(Keys.F2, (object sender) => {
                if (Valid()) {
                    Save();
                    // Close screen
                    return false;
                } else {
                    // Keep running the screen
                    return true;
                }
            });

            AddTip("Esc", "Cancel");
            AddBinding(Keys.Escape, (object sender) => {
                // Discard changes
                return false;
            });

            name = new ConsoleField(labelWidth, nameRow, -1, initName);
            path = new ConsoleField(labelWidth, pathRow, -1, initPath);

            AddObject(new ConsoleLabel(1, nameRow, labelWidth, () => "Name:"));
            AddObject(name);
            AddObject(new ConsoleLabel(1, pathRow, labelWidth, () => "Path to KSP:"));
            AddObject(path);

            LeftHeader   = () => $"CKAN {Meta.GetVersion()}";
            CenterHeader = () => "Edit KSP Instance";
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
                RaiseError("Name cannot be empty!");
                SetFocus(name);
                return false;
            } else if (manager.HasInstance(name.Value)) {
                // Complain about duplicate name
                RaiseError($"{name.Value} already exists!");
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
            if (!IsKspDir(path.Value)) {
                // Complain about non-KSP path
                RaiseError("Path does not correspond to a KSP folder!");
                SetFocus(path);
                return false;
            } else {
                return true;
            }
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
        /// KSP manager object that contains the instances
        /// </summary>
        protected KSPManager manager;

        // Copied from KSP class because it's inaccessible.
        // (Calling a constructor just for validation is gross.)
        private static bool IsKspDir(string directory)
        {
            return Directory.Exists(Path.Combine(directory, "GameData"));
        }

        private   const int labelWidth = 16;
        private   const int nameRow    = 2;
        /// <summary>
        /// Y coordinate of path field
        /// </summary>
        protected const int pathRow    = 4;
    }

}
