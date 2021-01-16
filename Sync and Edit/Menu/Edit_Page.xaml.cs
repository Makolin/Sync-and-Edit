using SQLite;
using Sync_and_Edit.DataBase;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Sync_and_Edit.Menu
{
    public sealed partial class Edit_Page : Page
    {
        public static ListBox my_list_box;
        public Edit_Page()
        {
            this.InitializeComponent();
            my_list_box = myListBox;
            myFrame.Navigate(typeof(EditPage.Source));
        }
        public void MyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (source.IsSelected)
            {
                myFrame.Navigate(typeof(EditPage.Source));
            }
            if (edit_tags.IsSelected)
            {
                using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                {
                    var exist = db.Query<Song>("Select * from Song");
                    if (exist.Count() == 0)
                    {
                        source.IsSelected = true;
                    }
                    else
                    {
                        myFrame.Navigate(typeof(EditPage.Edit_Tags));
                    }
                }

            }
            if (edit_files.IsSelected)
            {
                using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                {
                    var exist = db.Query<Song>("Select * from Song");
                    if (exist.Count() == 0)
                    {
                        source.IsSelected = true;
                    }
                    else
                    {
                        myFrame.Navigate(typeof(EditPage.Edit_Files));
                    }
                }
            }
        }
    }
}
