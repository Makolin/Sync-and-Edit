using Windows.UI.Xaml.Controls;

namespace Sync_and_Edit.Menu
{
    public sealed partial class Info_Page : Page
    {
        public static ListBox my_list_box;
        public Info_Page()
        {
            this.InitializeComponent();
            my_list_box = myListBox;
            myFrame.Navigate(typeof(InfoPage.Options));
        }
        private void myListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (oneStep.IsSelected)
            {
                myFrame.Navigate(typeof(InfoPage.Options));
            }
            if (twoStep.IsSelected)
            {
                myFrame.Navigate(typeof(InfoPage.Statistics));
            }
            if (threeStep.IsSelected)
            {
                myFrame.Navigate(typeof(InfoPage.About));
            }
        }
    }
}
