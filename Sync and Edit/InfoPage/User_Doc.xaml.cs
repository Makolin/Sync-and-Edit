using Sync_and_Edit.Menu;
using Windows.UI.Xaml.Controls;


namespace Sync_and_Edit.InfoPage
{
    public sealed partial class User_Doc : Page
    {
        public User_Doc()
        {
            this.InitializeComponent();
            Info_Page.my_list_box.SelectedIndex = 1;
        }
    }
}
