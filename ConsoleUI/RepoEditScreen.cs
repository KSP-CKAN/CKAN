using System;
using System.Collections.Generic;
using CKAN.Games;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen for editing an existing Repository entry
    /// </summary>
    public class RepoEditScreen : RepoScreen {

        /// <summary>
        /// Construct the Screen
        /// </summary>
        /// <param name="game">Game from which to get repos</param>
        /// <param name="reps">Collection of Repository objects</param>
        /// <param name="repo">The object to edit</param>
        public RepoEditScreen(IGame game, SortedDictionary<string, Repository> reps, Repository repo)
            : base(game, reps, repo.name, repo.uri.ToString())
        {
            repository = repo;
        }

        /// <summary>
        /// Check whether the fields are valid
        /// </summary>
        /// <returns>
        /// True if valid, false otherwise
        /// </returns>
        protected override bool Valid()
            => (name.Value == repository.name || nameValid())
                && (url.Value == repository.uri.ToString() || urlValid());

        /// <summary>
        /// Save changes
        /// </summary>
        protected override void Save()
        {
            if (name.Value != repository.name) {
                // They changed the name, so we have to
                // remove and re-add it.
                editList.Remove(repository.name);
                editList.Add(name.Value, new Repository(name.Value, url.Value));
            } else {
                // Only the URL changed, so we can just set it
                repository.uri = new Uri(url.Value);
            }
        }

        private readonly Repository repository;
    }

}
