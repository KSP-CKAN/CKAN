using System;
using System.IO;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen for adding a new KSP instance
    /// </summary>
    public class KSPAddScreen : KSPScreen {

        /// <summary>
        /// Initialize the Screen
        /// </summary>
        /// <param name="mgr">KSP manager containing the instances</param>
        public KSPAddScreen(KSPManager mgr) : base(mgr)
        {
            AddObject(new ConsoleLabel(
                labelWidth, pathRow + 1, -1,
                () => $"Example: {examplePath}",
                null, () => ConsoleTheme.Current.DimLabelFg
            ));
        }

        /// <summary>
        /// Return whether the fields are valid.
        /// The basic non-empty and unique checks are good enough for adding.
        /// </summary>
        protected override bool Valid()
        {
            if (!nameValid() || !pathValid()) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Put description in top center
        /// </summary>
        protected override string CenterHeader()
        {
            return "Add KSP Instance";
        }

        /// <summary>
        /// Add the instance
        /// </summary>
        protected override void Save()
        {
            manager.AddInstance(new KSP(path.Value, name.Value, new NullUser()));
        }

        private static readonly string examplePath = Path.Combine(
            !string.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles))
                ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Kerbal Space Program"
        );
    }

}
