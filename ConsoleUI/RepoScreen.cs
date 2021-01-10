using System;
using System.Collections.Generic;
using CKAN.ConsoleUI.Toolkit;
using CKAN.Games;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Base class for screens that create or edit Repository entries
    /// </summary>
    public abstract class RepoScreen : ConsoleScreen {

        /// <summary>
        /// Construct the screens
        /// </summary>
        /// <param name="game">Game from which to get repos</param>
        /// <param name="reps">Collection of Repository objects</param>
        /// <param name="initName">Initial value of the Name field</param>
        /// <param name="initUrl">Iniital value of the URL field</param>
        protected RepoScreen(IGame game, SortedDictionary<string, Repository> reps, string initName, string initUrl) : base()
        {
            editList = reps;
            defaultRepos = RepositoryList.DefaultRepositories(game);

            name = new ConsoleField(labelWidth, nameRow, -1, initName) {
                GhostText = () => "<Enter the name to use for this repository>"
            };
            url  = new ConsoleField(labelWidth, urlRow,  -1, initUrl) {
                GhostText = () => "<Enter the URL of this repository>"
            };

            AddObject(new ConsoleLabel(1, nameRow, labelWidth, () => "Name:"));
            AddObject(name);
            AddObject(new ConsoleLabel(1, urlRow,  labelWidth, () => "URL:"));
            AddObject(url);

            AddTip("F2", "Accept");
            AddBinding(Keys.F2, (object sender) => {
                if (Valid()) {
                    Save();
                    return false;
                } else {
                    return true;
                }
            });

            AddTip("Esc", "Cancel");
            AddBinding(Keys.Escape, (object sender) => {
                return false;
            });

            // mainMenu = list of default options
            if (defaultRepos.repositories != null && defaultRepos.repositories.Length > 0) {
                List<ConsoleMenuOption> opts = new List<ConsoleMenuOption>();
                foreach (Repository r in defaultRepos.repositories) {
                    // This variable will be remembered correctly in our lambdas later
                    Repository repo = r;
                    opts.Add(new ConsoleMenuOption(
                        repo.name, "", $"Import values from default mod list source {repo.name}",
                        true, () => {
                            name.Value    = repo.name;
                            name.Position = name.Value.Length;
                            url.Value     = repo.uri.ToString();
                            url.Position  = url.Value.Length;
                            return true;
                        }
                    ));
                }
                mainMenu = new ConsolePopupMenu(opts);
            }
        }

        /// <summary>
        /// Put CKAN 1.25.5 in top left corner
        /// </summary>
        protected override string LeftHeader()
        {
            return $"CKAN {Meta.GetVersion()}";
        }

        /// <summary>
        /// Put description in top center
        /// </summary>
        protected override string CenterHeader()
        {
            return "Edit Mod List Source";
        }

        /// <summary>
        /// Report whether the fields are Valid
        /// </summary>
        /// <returns>
        /// True if valid, false otherwise
        /// </returns>
        protected abstract bool Valid();

        /// <summary>
        /// Save changes
        /// </summary>
        protected abstract void Save();

        /// <summary>
        /// Check whether the name field is valid
        /// </summary>
        /// <returns>
        /// True if non-empty and unique, false otherwise
        /// </returns>
        protected bool nameValid()
        {
            if (string.IsNullOrEmpty(name.Value)) {
                RaiseError("Name cannot be empty!");
                SetFocus(name);
                return false;
            } else if (editList.ContainsKey(name.Value)) {
                RaiseError($"{name.Value} already exists!");
                SetFocus(name);
                return false;
            } else {
                return true;
            }
        }

        /// <summary>
        /// Check whether the URL field is valid.
        /// Would be nice to access the URL and validate, but they can be large,
        /// for example master.tar.gz is ~2MB.
        /// </summary>
        /// <returns>
        /// True if non-empty, false otherwise
        /// </returns>
        protected bool urlValid()
        {
            if (string.IsNullOrEmpty(url.Value)) {
                RaiseError("URL cannot be empty!");
                SetFocus(url);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Name field
        /// </summary>
        protected ConsoleField name;
        /// <summary>
        /// URL field
        /// </summary>
        protected ConsoleField url;

        /// <summary>
        /// Temporary list of Repository objects
        /// </summary>
        protected SortedDictionary<string, Repository> editList;

        private RepositoryList defaultRepos;

        private const int labelWidth = 8;
        private const int nameRow    = 3;
        private const int urlRow     = 5;
    }

}
