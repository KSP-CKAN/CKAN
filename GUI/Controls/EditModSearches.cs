using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    /// <summary>
    /// A container control for the individual EditModSearch controls
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class EditModSearches : UserControl
    {
        public EditModSearches()
        {
            InitializeComponent();
            ToolTip.SetToolTip(AddSearchButton, Properties.Resources.EditModSearchesTooltipAddSearchButton);
            ActiveControl = AddSearch();
        }

        public event Action                  SurrenderFocus;
        public event Action<List<ModSearch>> ApplySearches;
        public event Action<string>          ShowError;

        public void Clear()
        {
            for (int i = editors.Count - 1; i >= 0; --i)
            {
                RemoveSearch(editors[i]);
            }
            editors[0].Clear();
        }

        public void ExpandCollapse()
        {
            (ActiveControl as EditModSearch)?.ExpandCollapse();
        }

        public void CloseSearch(Point screenCoords)
        {
            foreach (var editor in editors)
            {
                editor.CloseSearch(screenCoords);
            }
        }

        public void ParentMoved()
        {
            foreach (var editor in editors)
            {
                editor.ParentMoved();
            }
        }

        public void SetSearches(List<ModSearch> searches)
        {
            while (editors.Count > searches.Count && editors.Count > 1)
            {
                RemoveSearch(editors[editors.Count - 1]);
            }
            if (searches.Count < 1)
            {
                editors[0].Clear();
            }
            else
            {
                for (int i = 0; i < searches.Count; ++i)
                {
                    var editor = i >= editors.Count ? AddSearch() : editors[i];
                    editor.Search = searches[i];
                }
            }
            Apply();
        }

        /// <summary>
        /// Merge the given searches with the currently active ones
        /// </summary>
        /// <param name="searches">New searches to add</param>
        public void MergeSearches(List<ModSearch> searches)
        {
            // Merge inputs once for all editors
            var merged = searches.Aggregate((search, newSearch) => search.MergedWith(newSearch));
            foreach (var editor in editors)
            {
                // Combine all new with each existing (old AND new)
                editor.Search = editor.Search?.MergedWith(merged) ?? merged;
            }
            Apply();
        }

        private void AddSearchButton_Click(object sender, EventArgs e)
        {
            AddSearch().Focus();
            AddSearchButton.Enabled = false;
        }

        private EditModSearch AddSearch()
        {
            SuspendLayout();

            var ctl = new EditModSearch()
            {
                // Dock handles the layout for us
                Dock      = DockStyle.Top,
                ShowLabel = editors.Count < 1,
                TabIndex  = editors.Count * 2
            };
            ctl.ApplySearch    += EditModSearch_ApplySearch;
            ctl.SurrenderFocus += EditModSearch_SurrenderFocus;
            ctl.ShowError      += error => ShowError?.Invoke(error);

            editors.Add(ctl);
            Controls.Add(ctl);
            // Docked controls are back at top, front at bottom, so send to bottom
            ctl.BringToFront();
            // Still need to be able to see the add button, without this it's covered up
            AddSearchButton.BringToFront();

            Height = editors.Sum(ems => ems.Height);

            ResumeLayout(false);
            PerformLayout();

            AddSearchButton.Top = editors[editors.Count - 1].Top;

            return ctl;
        }

        private void RemoveSearch(EditModSearch which)
        {
            if (editors.Count >= 2)
            {
                if (which == ActiveControl)
                {
                    // Move focus to next control, or previous if last in list
                    var activeIndex = editors.IndexOf(which);
                    ActiveControl   = activeIndex < editors.Count - 1
                        ? editors[activeIndex + 1]
                        : editors[activeIndex - 1];
                }

                editors.Remove(which);
                Controls.Remove(which);
                // Make sure the top label is always visible
                editors[0].ShowLabel = true;

                AddSearchButton.Top = editors[editors.Count - 1].Top;

                Height = editors.Sum(ems => ems.Height);
            }
        }

        private void EditModSearch_ApplySearch(EditModSearch source, ModSearch what)
        {
            if (what == null && editors.Count > 1)
            {
                RemoveSearch(source);
            }
            Apply();
        }

        private void Apply()
        {
            var searches = editors.Select(ems => ems.Search)
                                  .Where(s => s != null)
                                  .ToList();
            ApplySearches?.Invoke(searches.Count == 0 ? null : searches);
            AddSearchButton.Enabled = editors.Count == searches.Count;
        }

        private void EditModSearch_SurrenderFocus()
        {
            SurrenderFocus?.Invoke();
        }

        private readonly List<EditModSearch> editors = new List<EditModSearch>();
    }

}
