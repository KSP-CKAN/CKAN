using System.Windows.Forms;

namespace CKAN
{
    public partial class Main
    {
        private void ChooseProvidedMods_OnSelectedItemsChanged(ListView.SelectedListViewItemCollection items)
        {
            ShowSelectionModInfo(items);
        }
    }
}
