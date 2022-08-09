using System.Windows.Forms;

namespace CKAN.GUI
{
    public partial class Main
    {
        private void ChooseProvidedMods_OnSelectedItemsChanged(ListView.SelectedListViewItemCollection items)
        {
            ShowSelectionModInfo(items);
        }
    }
}
