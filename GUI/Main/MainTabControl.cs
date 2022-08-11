using System;

namespace CKAN.GUI
{
    public class MainTabControl : ThemedTabControl
    {
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            switch (this.SelectedTab?.Name)
            {
                case "ManageModsTabPage":
                    // Focus the grid
                    Main.Instance.ManageMods.ModGrid.Focus();
                    break;
            }
        }
    }
}
