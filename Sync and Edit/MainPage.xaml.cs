using Sync_and_Edit.DataBase;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Sync_and_Edit
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InsertMusicFormat();
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(1300, 700);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            myContent.Navigate(typeof(Menu.Glav_Page));
        }

        private void InsertMusicFormat()
        {
            string[] ListFormat = new string[] { "mp3", "flac", "alac", "m4a", "ape", "ogg", "wav", "wma", "aac" };
            DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
            foreach (string format in ListFormat)
            {
                Db_Helper.Insert(new Music_format(format));
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            myContent.Navigate(typeof(Menu.Glav_Page));
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            myContent.Navigate(typeof(Menu.Sync_Page));
        }

        public void EditButton_Click(object sender, RoutedEventArgs e)
        {
            myContent.Navigate(typeof(Menu.Edit_Page));
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            myContent.Navigate(typeof(Menu.Info_Page));
        }
    }
}
