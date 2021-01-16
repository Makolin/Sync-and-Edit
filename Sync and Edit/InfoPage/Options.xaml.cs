using SQLite;
using Sync_and_Edit.DataBase;
using Sync_and_Edit.Json;
using Sync_and_Edit.Menu;
using System;
using System.IO;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Sync_and_Edit.InfoPage
{
    public sealed partial class Options : Page
    {
        Json_options Json = new Json_options();

        public Options()
        {
            Read_Json();
            this.InitializeComponent();
            Info_Page.my_list_box.SelectedIndex = 0;

        }

        private int Index_combobox(string codec)
        {
            switch (codec)
            {
                case "High":
                    return 0;
                case "Low":
                    return 1;
                case "Medium":
                    return 2;
                default:
                    return 0;
            }
        }

        private string Index_comboboxBack(int codec)
        {
            switch (codec)
            {
                case 0:
                    return "High";
                case 1:
                    return "Low";
                case 2:
                    return "Medium";
                default:
                    return "High";
            }
        }

        private async void Read_Json()
        {
            Json = await Json.Read_Json();
            mp3.SelectedIndex = Index_combobox(Json.Json_mp3);
            alac.SelectedIndex = Index_combobox(Json.Json_alac);
            flac.SelectedIndex = Index_combobox(Json.Json_flac);
            aac.SelectedIndex = Index_combobox(Json.Json_aac);
        }

        private async void Accept_Click(object sender, RoutedEventArgs e)
        {
            Json.Write_Json(Json.Json_source_1, Json.Json_source_2, Json.Json_source_3,
               Json.Json_audioteca, Index_comboboxBack(mp3.SelectedIndex),
               Index_comboboxBack(alac.SelectedIndex), Index_comboboxBack(flac.SelectedIndex),
               Index_comboboxBack(aac.SelectedIndex));

            var dialog = new MessageDialog("Настройки успешно сохранены");
            dialog.Commands.Add(new UICommand { Label = "Okay", Id = 0 });
            await dialog.ShowAsync();
        }

        private async void ClearDB_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                try
                {
                    DatabaseHelperClass.ClearDatabase();
                    var dialog = new MessageDialog("База очищена ");
                    dialog.Commands.Add(new UICommand { Label = "Okay", Id = 0 });
                    await dialog.ShowAsync();
                }

                catch { }
            }
        }

        private async void User_doc_Click(object sender, RoutedEventArgs e)
        {
            string pdfFile = @"ms-appx:///Resources/guide.pdf";
            var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(pdfFile);
            if (file != null)
            {
                var success = await Windows.System.Launcher.LaunchFileAsync(file);
                if (success)
                {
                    // File launched
                }
                else
                {
                    // File launch failed
                }
            }
            else
            {
                // Could not find file
            }
        }
    }
}
