using SQLite;
using Sync_and_Edit.DataBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


namespace Sync_and_Edit.SyncPage
{
    public sealed partial class New_Device : Page
    {
        string[] ListFormat = new string[] { "mp3", "flac", "alac", "m4a", "ape", "ogg", "wav", "wma", "aac" };
        DatabaseHelperClass Db_Helper = new DatabaseHelperClass();

        public New_Device()
        {
            this.InitializeComponent();
        }

        private void Enter_Device_music(int formatId)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var deviceID = db.Query<Music_format>("select Id from Device").Last();
                Db_Helper.Insert_Device_Format(new Device_format(deviceID.Id, formatId));
            }
        }

        public static List<string> Find_Song(string path)
        {
            try
            {
                var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).ToList();
                return files;
            }
            catch
            {

            }
            return null;
        }

        private async void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (Name_device.Text == "" || Model.Text == "" || Size.Text == "" || Source.Text == "")
            {
                var dialog = new MessageDialog("Введены не все данные");
                await dialog.ShowAsync();
            }
            else
            {
                if ((Format_0.IsChecked == true || Format_1.IsChecked == true || Format_2.IsChecked == true ||
                    Format_3.IsChecked == true || Format_4.IsChecked == true || Format_5.IsChecked == true ||
                    Format_6.IsChecked == true || Format_7.IsChecked == true || Format_8.IsChecked == true) == false)
                {
                    var dialog = new MessageDialog("Не выбран ни один из форматов музыкальных файлов!");
                    await dialog.ShowAsync();
                }
                else
                {
                    FinishDialog();
                }
            }
        }

        private async System.Threading.Tasks.Task Insert_to_DB()
        {
            int number = 1;
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                try
                {
                    var device_last = db.Query<Device>("Select * from Device").Last();
                    if (device_last != null)
                    {
                        number = device_last.Id + 1;
                    }
                }
                catch
                {

                }
            }

            Db_Helper.Insert_Device(new Device(number, Name_device.Text, Model.Text, Convert.ToDouble(Size.Text)));
            if (Format_0.IsChecked == true)
            {
                Enter_Device_music(1);
            }
            if (Format_1.IsChecked == true)
            {
                Enter_Device_music(2);
            }
            if (Format_2.IsChecked == true)
            {
                Enter_Device_music(3);
            }
            if (Format_3.IsChecked == true)
            {
                Enter_Device_music(4);
            }
            if (Format_4.IsChecked == true)
            {
                Enter_Device_music(5);
            }
            if (Format_5.IsChecked == true)
            {
                Enter_Device_music(6);
            }
            if (Format_6.IsChecked == true)
            {
                Enter_Device_music(7);
            }
            if (Format_7.IsChecked == true)
            {
                Enter_Device_music(8);
            }
            if (Format_8.IsChecked == true)
            {
                Enter_Device_music(9);
            }
            Insert_sync(); //добавление файлов для синхронизации
            Save_file();

            var dialog = new MessageDialog("Устройство успешно добавлено");
            dialog.Commands.Add(new UICommand { Label = "Okay", Id = 0 });
            await dialog.ShowAsync();
            Frame.Navigate(typeof(Devices));
        }

        private async void Save_file()
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var device = db.Query<Device>("Select * from Device").Last();
                var name = "Device" + device.Id;
                var Main_folder = await Windows.Storage.AccessCache.StorageApplicationPermissions
                    .FutureAccessList.GetFolderAsync(name);
                StorageFile id = await Main_folder.CreateFileAsync(name + ".ini", CreationCollisionOption.ReplaceExisting);
            }
        }

        private void Insert_sync()
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var temp_id = db.Query<Device>("select * from Device").Last();
                var counts_song = db.Query<Song>("Select * from Song");
                foreach (Song song in counts_song)
                {
                    Db_Helper.Insert_Sync_Device(new Sync_Device(temp_id.Id, song.SongID));
                }
            }
        }

        private void Size_PreviewKeyDown(object sender, KeyRoutedEventArgs e) //Вводить только числа
        {
            if (e.Key.ToString().Equals("Back"))
            {
                e.Handled = false;
                return;
            }
            for (int i = 0; i < 10; i++)
            {
                if (e.Key.ToString() == string.Format("Number{0}", i))
                {
                    e.Handled = false;
                    return;
                }
            }
            e.Handled = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Devices));
        }


        private async void Source_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                {
                    //храним пути до папок
                    try
                    {
                        var device = db.Query<Device>("Select * from Device").Last();
                        var name = "Device" + (device.Id + 1).ToString();
                        Windows.Storage.AccessCache.StorageApplicationPermissions.
                            FutureAccessList.AddOrReplace(name, folder);
                        Source.Text = folder.Path;
                    }
                    catch
                    {
                        var name = "Device" + 1;
                        Windows.Storage.AccessCache.StorageApplicationPermissions.
                            FutureAccessList.AddOrReplace(name, folder);
                        Source.Text = folder.Path;
                    }
                }
            }
            else
            {

            }
        }
        private async void FinishDialog()
        {
            ContentDialog noWifiDialog = new ContentDialog
            {
                Title = "Очищение папки",
                Content = "Указанная папка будет очищена, Вы уверены?",
                PrimaryButtonText = "Да",
                SecondaryButtonText = "Отмена"

            };
            ContentDialogResult result = await noWifiDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var files = Find_Song(Source.Text);
                foreach (var file in files)
                {
                    await Task.Run(() =>
                    {
                        File.Delete(file);
                    });
                }
                await Insert_to_DB();
            }
            if (result == ContentDialogResult.Secondary)
            {
                var dialog = new MessageDialog("Выберите другую папку для аудиотеки");
                dialog.Commands.Add(new UICommand { Label = "Okay", Id = 0 });
                await dialog.ShowAsync();
            }
        }
    }
}
