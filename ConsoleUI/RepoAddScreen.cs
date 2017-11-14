using System.Collections.Generic;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen for creating a new Repository entry
    /// </summary>
    public class RepoAddScreen : RepoScreen {

        /// <summary>
        /// Construct the screen
        /// </summary>
        /// <param name="reps">Collection of Repository objects</param>
        public RepoAddScreen(SortedDictionary<string, Repository> reps)
            : base(reps, "", "") { }

        /// <summary>
        /// Check whether the fields are valid
        /// </summary>
        /// <returns>
        /// True if name and URL are valid, false otherwise.
        /// </returns>
        protected override bool Valid()
        {
            if (!nameValid() || !urlValid()) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Save the new Repository
        /// </summary>
        protected override void Save()
        {
            // Set priority to end of list
            int prio = 0;
            foreach (var kvp in editList) {
                Repository r = kvp.Value;
                if (r.priority >= prio) {
                    prio = r.priority + 1;
                }
            }
            editList.Add(name.Value, new Repository(name.Value, url.Value, prio));
        }

    }

}
