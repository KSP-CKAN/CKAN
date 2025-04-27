using System.Linq;
using System.Windows.Forms;

namespace CKAN.GUI
{
    public static class WinFormsExtensions
    {
        /// <summary>
        /// GetControlFromPosition returns null if Control.Visible == false.
        /// This doesn't.
        /// </summary>
        /// <param name="panel">The table to inspect</param>
        /// <param name="column">The column of the control to retrieve</param>
        /// <param name="row">the row of the control to retrieve</param>
        /// <returns>The control at the given cell of the table</returns>
        public static Control? GetControlFromPositionEvenIfInvisible(this TableLayoutPanel panel,
                                                                     int                   column,
                                                                     int                   row)
            => panel.Controls.OfType<Control>()
                             .FirstOrDefault(ctl => panel.GetPositionFromControl(ctl)
                                                    is TableLayoutPanelCellPosition pos
                                                    && pos.Column == column
                                                    && pos.Row    == row);
    }
}
