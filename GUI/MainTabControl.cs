using System;
using System.Windows.Forms;

namespace CKAN
{
    public class MainTabControl : TabControl
    {
        public MainTabControl() : base()
        {
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            if (this.SelectedTab != null && this.SelectedTab.Name.Equals("ManageModsTabPage"))
                ((Main)this.Parent).ModList.Focus();
        }
    }
}