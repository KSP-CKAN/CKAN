using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.Extensions;

namespace CKAN.GUI
{
    /// <summary>
    /// Show the user's play time in each instance and allow editing
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class PlayTime : UserControl
    {
        /// <summary>
        /// Initialize the control
        /// </summary>
        public PlayTime()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Load the grid of play times
        /// </summary>
        /// <param name="manager">Game instance manager containing the instances to be loaded</param>
        public void loadAllPlayTime(GameInstanceManager manager)
        {
            rows = manager.Instances
                .Where(kvp => kvp.Value.playTime != null)
                .Select(kvp => new PlayTimeRow(kvp.Key, kvp.Value))
                .ToList();
            PlayTimeGrid.DataSource = new BindingList<PlayTimeRow>(rows);
            ShowTotal();
        }

        /// <summary>
        /// Invoked when the user clicks OK
        /// </summary>
        public event Action Done;

        /// <summary>
        /// Open the user guide when the user presses F1
        /// </summary>
        protected override void OnHelpRequested(HelpEventArgs evt)
        {
            evt.Handled = Util.TryOpenWebPage(HelpURLs.PlayTime);
        }

        private void ShowTotal()
        {
            if (rows != null)
            {
                var totalPlayed = rows
                    .Select(row => row.PlayTime.Time)
                    .Sum();
                TotalLabel.Text = string.Format(
                    Properties.Resources.TotalPlayTime,
                    totalPlayed.TotalHours.ToString("N1"));
            }
        }

        private void PlayTimeGrid_CellValueChanged(object sender, DataGridViewCellEventArgs evt)
        {
            ShowTotal();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            Done?.Invoke();
        }

        private List<PlayTimeRow> rows;
    }

    /// <summary>
    /// Represents one row of the play time grid in the DataSource,
    /// with properties mapped to columns by DataPropertyName
    /// </summary>
    public class PlayTimeRow
    {
        /// <summary>
        /// Initialize the row
        /// </summary>
        public PlayTimeRow(string name, GameInstance instance)
        {
            Name     = name;
            PlayTime = instance.playTime;
            path     = TimeLog.GetPath(instance.CkanDir());
        }

        /// <summary>
        /// The name of the instance to display
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The TimeLog object for this row
        /// </summary>
        public readonly TimeLog PlayTime;

        /// <summary>
        /// The time value to display in hours.
        /// If the user edits the cell and changes it to a valid
        /// double, we will convert it from hours to a TimeSpan
        /// and save it to disk for this instance.
        /// </summary>
        public string Time
        {
            get => PlayTime.ToString();
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    PlayTime.Time = TimeSpan.Zero;
                    PlayTime.Save(path);
                }
                else if (double.TryParse(value, out double hours))
                {
                    PlayTime.Time = TimeSpan.FromHours(hours);
                    PlayTime.Save(path);
                }
            }
        }

        private readonly string path;
    }
}
