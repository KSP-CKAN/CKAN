using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CKAN.GUI
{
    /// <summary>
    /// A container control for the individual EditModSearch controls
    /// </summary>
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

            editors.Add(ctl);
            Controls.Add(ctl);
            // Docked controls are back at top, front at bottom, so send to bottom
            ctl.BringToFront();
            // Still need to be able to see the add button, without this it's covered up
            AddSearchButton.BringToFront();

            Height = editors.Sum(ems => ems.Height);

            ResumeLayout(false);
            PerformLayout();

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

        private List<EditModSearch> editors = new List<EditModSearch>();
    }

}
