using SQLite;
using Sync_and_Edit.DataBase;
using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;


namespace Sync_and_Edit.Menu
{
    public sealed partial class Sync_Page : Page
    {
        public static Device Main_Current_Device = new Device();
        public static ListBox my_list_box;
        public Sync_Page()
        {
            this.InitializeComponent();
            my_list_box = myListBox;
            myFrame.Navigate(typeof(SyncPage.Devices));
        }

        private async void myListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (oneStep.IsSelected)
            {
                myFrame.Navigate(typeof(SyncPage.Devices));
            }
            if (twoStep.IsSelected)
            {
                using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                {
                    var exist = db.Query<Device>("Select * from Device");
                    if (exist.Count() == 0)
                    {
                        oneStep.IsSelected = true;
                        var dialog = new MessageDialog("База данных устройств пуста!");
                        await dialog.ShowAsync();
                    }
                    else
                    {
                        myFrame.Navigate(typeof(SyncPage.Edit));
                    }
                }

            }
            if (threeStep.IsSelected)
            {
                using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                {
                    var exist = db.Query<Device>("Select * from Device");
                    if (exist.Count() == 0)
                    {
                        oneStep.IsSelected = true;
                        var dialog = new MessageDialog("База данных устройств пуста!");
                        await dialog.ShowAsync();
                    }
                    else
                    {
                        myFrame.Navigate(typeof(SyncPage.Sync));
                    }
                }
            }
        }
    }
}
