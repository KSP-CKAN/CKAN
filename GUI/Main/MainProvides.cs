using System.Windows.Forms;

namespace CKAN
{
    public partial class Main
    {
        private void ChooseProvidedMods_OnSelectedItemsChanged(ListView.SelectedListViewItemCollection items)
        {
            ShowSelectionModInfo(items);
        }

        private CkanModule TooManyModsProvideCore(TooManyModsProvideKraken kraken)
        {
            tabController.ShowTab("ChooseProvidedModsTabPage", 3);
            ChooseProvidedMods.LoadProviders(kraken.requested, kraken.modules, Manager.Cache);
            tabController.SetTabLock(true);
            return ChooseProvidedMods.Wait();
        }
    }
}
