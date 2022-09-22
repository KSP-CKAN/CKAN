using System;
using System.Drawing;
using System.Windows.Forms;

namespace CKAN.GUI
{
    /// <summary>
    /// A TabControl that obeys system colors to look less awful in a dark theme
    /// </summary>
    public class ThemedTabControl : TabControl
    {
        public ThemedTabControl() : base()
        {
            // Tell the base class that we want to draw things ourselves
            DrawMode = TabDrawMode.OwnerDrawFixed;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // Background
            Rectangle bgRect = e.Bounds;
            bgRect.Inflate(-2, -1);
            bgRect.Offset(0, 1);
            e.Graphics.FillRectangle(new SolidBrush(BackColor), bgRect);
            // e.Index can be invalid (!!), so we need try/catch
            try
            {
                var tabPage  = TabPages[e.Index];
                var textRect = e.Bounds;

                // Image
                var imageIndex = !string.IsNullOrEmpty(tabPage.ImageKey)
                    ? ImageList.Images.IndexOfKey(tabPage.ImageKey)
                    : tabPage.ImageIndex;
                if (imageIndex > -1)
                {
                    var image = ImageList.Images[imageIndex];
                    var offsetY = (e.Bounds.Height - image.Height) / 2;
                    // Tab is wider when selected, don't move image left 1px
                    var offsetX = e.State == DrawItemState.Selected ? offsetY + 3
                                                                    : offsetY + 2;
                    // e.Graphics.DrawImage doesn't work on Mono, but this does
                    ImageList.Draw(e.Graphics, e.Bounds.Location + new Size(offsetX, offsetY), imageIndex);

                    // Don't overlap text on image
                    textRect.X     += image.Width;
                    textRect.Width -= image.Width;
                }

                // Text
                TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font,
                                      textRect, tabPage.ForeColor);
            }
            catch (ArgumentOutOfRangeException)
            {
                // No such tab page, oh well
            }
            // Alert event subscribers
            base.OnDrawItem(e);
        }
    }
}
