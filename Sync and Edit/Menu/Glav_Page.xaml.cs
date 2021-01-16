using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Sync_and_Edit.Menu
{
    public sealed partial class Glav_Page : Page
    {
        public Glav_Page()
        {
            this.InitializeComponent();
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Edit_Page));
        }

        private void Sync_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Sync_Page));
        }
    }
}
