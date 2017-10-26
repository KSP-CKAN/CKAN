using System;
using System.IO;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen for editing an existing game instance
    /// </summary>
    public class KSPEditScreen : KSPScreen {

        /// <summary>
        /// Initialize the Screen
        /// </summary>
        /// <param name="mgr">KSP manager containing the instances</param>
        /// <param name="k">Instance to edit</param>
        public KSPEditScreen(KSPManager mgr, KSP k)
            : base(mgr, KSPListScreen.InstallName(mgr, k), k.GameDir())
        {
            ksp = k;
        }

        /// <summary>
        /// Return whether the fields are valid.
        /// Similar to adding, except leaving the fields unchanged is allowed.
        /// </summary>
        protected override bool Valid()
        {
            if (name.Value != KSPListScreen.InstallName(manager, ksp)
                    && !nameValid()) {
                return false;
            }
            if (path.Value != ksp.GameDir()
                    && !pathValid()) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Save the changes.
        /// Similar to adding, except we have to remove the old instance,
        /// and there's an API specifically for renaming.
        /// </summary>
        protected override void Save()
        {
            string oldName = KSPListScreen.InstallName(manager, ksp);
            if (path.Value != ksp.GameDir()) {
                // If the path is changed, then we have to remove the old instance
                // and replace it with a new one, whether or not the name is changed.
                manager.RemoveInstance(oldName);
                manager.AddInstance(name.Value, new KSP(path.Value, new NullUser()));
            } else if (name.Value != oldName) {
                // If only the name changed, there's an API for that.
                manager.RenameInstance(KSPListScreen.InstallName(manager, ksp), name.Value);
            }
        }

        private KSP ksp;
    }

}
