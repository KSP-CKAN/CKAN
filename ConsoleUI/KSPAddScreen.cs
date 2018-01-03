namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen for adding a new KSP instance
    /// </summary>
    public class KSPAddScreen : KSPScreen {

        /// <summary>
        /// Initialize the Screen
        /// </summary>
        /// <param name="mgr">KSP manager containing the instances</param>
        public KSPAddScreen(KSPManager mgr) : base(mgr) { }

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
        /// Add the instance
        /// </summary>
        protected override void Save()
        {
            manager.AddInstance(new KSP(path.Value, name.Value, new NullUser()));
        }
    }

}
