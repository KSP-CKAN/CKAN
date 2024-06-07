using System;
using System.IO;

using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen for adding a new game instance
    /// </summary>
    public class GameInstanceAddScreen : GameInstanceScreen {

        /// <summary>
        /// Initialize the Screen
        /// </summary>
        /// <param name="mgr">Game instance manager containing the instances</param>
        public GameInstanceAddScreen(GameInstanceManager mgr) : base(mgr)
        {
            AddObject(new ConsoleLabel(
                labelWidth, pathRow + 1, -1,
                () => string.Format(Properties.Resources.InstanceAddExample, examplePath),
                null, th => th.DimLabelFg
            ));
        }

        /// <summary>
        /// Return whether the fields are valid.
        /// The basic non-empty and unique checks are good enough for adding.
        /// </summary>
        protected override bool Valid()
            => nameValid() && pathValid();

        /// <summary>
        /// Put description in top center
        /// </summary>
        protected override string CenterHeader()
            => Properties.Resources.InstanceAddTitle;

        /// <summary>
        /// Add the instance
        /// </summary>
        protected override void Save()
        {
            manager.AddInstance(path.Value, name.Value, new NullUser());
        }

        private static readonly string examplePath = Path.Combine(
            !string.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles))
                ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Kerbal Space Program"
        );
    }

}
