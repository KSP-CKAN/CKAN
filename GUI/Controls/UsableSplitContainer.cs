using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class UsableSplitContainer : SplitContainer
    {
        public new int SplitterDistance
        {
            get => base.SplitterDistance;

            // SplitContainer throws exceptions if the PanelMinSize limits are exceeded
            // rather than simply obeying them.  Here we fix that.
            set
            {
                try
                {
                    base.SplitterDistance =
                        // Ensure the value respects Panel1MinSize
                        value < Panel1MinSize
                            ? Panel1MinSize
                            // Ensure the value respects Panel2MinSize
                            : Orientation switch
                            {
                                Orientation.Horizontal =>
                                    value > Height - SplitterWidth - Panel2MinSize
                                        ? Height - SplitterWidth - Panel2MinSize
                                        : value,
                                Orientation.Vertical or _ =>
                                    value > Width - SplitterWidth - Panel2MinSize
                                        ? Width - SplitterWidth - Panel2MinSize
                                        : value,
                            };
                            // If both can't be respected, just let it throw and discard
                }
                catch
                {
                    // Suppress any exceptions
                }
            }
        }
    }
}
