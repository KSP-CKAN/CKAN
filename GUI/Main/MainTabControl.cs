using System;
using System.Windows.Forms;

namespace CKAN
{
    public class MainTabControl : ThemedTabControl
    {
        public MainTabControl() : base() { }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            switch (this.SelectedTab?.Name)
            {
                case "ManageModsTabPage":
                    // Focus the grid
                    Main.Instance.ModList.Focus();
                    break;
            }
        }
    }
}
