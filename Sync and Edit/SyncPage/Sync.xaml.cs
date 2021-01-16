using SQLite;
using Sync_and_Edit.DataBase;
using Sync_and_Edit.Menu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Sync_and_Edit.SyncPage
{
    public sealed partial class Sync : Page
    {
        DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
        Device Current_device;
        private static int delete_comp = 0, delete_device = 0, copy_device = 0, step = 0,
            count_sync_song = 0;
        static List<Sync_Device> first; //для проверки о первом запуске синхронизации для данного устройства
        public Sync()
        {
            this.InitializeComponent();
            Sync_Page.my_list_box.SelectedIndex = 2;
            Text();
        }


        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Sync));
        }

        public static List<string> Find_Song(string path)
        {
            //*делать проверку на доступ
            try
            {
                var files = Directory
                .EnumerateFiles(path, "*.*", SearchOption.AllDirectories) //Перенос всех песен из данного каталога и подкатологов
                .Where(file => file.ToLower().EndsWith("mp3") ||
                file.ToLower().EndsWith("m4a") || file.ToLower().EndsWith("flac") ||
                file.ToLower().EndsWith("alac") || file.ToLower().EndsWith("ogg") ||
                file.ToLower().EndsWith("wav") || file.ToLower().EndsWith("wma") ||
                file.ToLower().EndsWith("ape") || file.ToLower().EndsWith("aac"))
                .ToList();
                return files;
            }
            catch
            {

            }
            return null;
        }

        private async Task<StorageFolder> Find_device(Device item)
        {
            try
            {
                var name = "Device" + item.Id;
                var Main_folder = await Windows.Storage.AccessCache.StorageApplicationPermissions
                    .FutureAccessList.GetFolderAsync(name);
                return Main_folder;
            }

            catch
            {
                return null;
            }
        }

        private async void Delete_computer_Checked(object sender, RoutedEventArgs e)
        {
            if (Current_device != null)
            {
                using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                {
                    var temp = await Find_device(Current_device);
                    var count_on_device = Find_Song(temp.Path);
                    var count_on_copm = db.Query<Sync_Device>("Select * from Sync_Device where Date_sync != 0 and DeviceID = " + Current_device.Id).ToList();
                    if (count_on_device.Count() != count_on_copm.Count())
                    {
                        delete_comp = count_on_copm.Count() - count_on_device.Count();
                    }
                }
                DeleteComputer.Text = "\t - " + delete_comp + " файл(ов) будет удалено с компьютера";
            }
        }

        private void Delete_computer_Unchecked(object sender, RoutedEventArgs e)
        {
            delete_comp = 0;
            DeleteComputer.Text = "\t - " + delete_comp + " файл(ов) будет удалено с компьютера";
        }

        private void Delete_device_Checked(object sender, RoutedEventArgs e)
        {
            if (Current_device != null)
            {
                using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                {
                    var temp_song = db.Query<Song>("Select * from Song where Deleted = true");
                    if (temp_song.Count() != 0)
                    {
                        delete_device = temp_song.Count();
                    }
                }
                DeleteDevice.Text = "\t - " + delete_device + " файл(ов) будет удалено с устройства";
            }

        }

        private void Delete_device_Unchecked(object sender, RoutedEventArgs e)
        {
            delete_device = 0;
            DeleteDevice.Text = "\t - " + delete_device + " файл(ов) будет удалено с устройства";
        }

        private async void Text()
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var all_device = db.Query<Device>("Select * from Device");
                int not_unic = 0; //для изменения количества, если песни одинаковые
                StorageFolder temp = null;
                foreach (var item in all_device)
                {
                    temp = await Find_device(item);
                    if (temp != null)
                    {
                        Current_device = item;
                        var song = db.Query<Sync_Device>("Select * from Sync_Device where Date_sync = 0 and " +
                            "Synchronization = true and DeviceID = " + Current_device.Id).ToList();
                        copy_device = song.Count();

                        var count_on_device = Find_Song(temp.Path);
                        var count_on_copm = db.Query<Sync_Device>("Select * from Sync_Device where Date_sync != 0 and " +
                            " DeviceID = " + Current_device.Id).ToList();

                        var temp_song = db.Query<Song>("Select * from Song where Deleted = true");
                        if (temp_song.Count() != 0)
                        {
                            foreach (var item_song in temp_song)
                            {
                                var format = db.Find<Music_format>(c => c.Id == item_song.Format_Id);
                                var text_song = temp.Path + "\\" + item_song.Name_song + "." + format.Name_format;
                                if (!count_on_device.Contains(text_song))
                                {
                                    Db_Helper.DeleteSong(item_song.SongID);
                                    Db_Helper.DeleteTag(item_song.TagId);
                                    Db_Helper.Delete_Song_in_Device_sync(item_song.SongID); //удаляем из таблицы синхронизации
                                    not_unic++;
                                }
                            }
                        }
                        delete_device = temp_song.Count() - not_unic;
                        delete_comp = count_on_copm.Count() - count_on_device.Count - not_unic;


                        if ((delete_comp + delete_device + copy_device) == 0)
                        {
                            Load.Text = "Все файлы синхронизированы";
                        }
                        else
                        {
                            Insert_Text(item);
                        }
                    }
                }
            }
        }

        private void Insert_Text(Device current_device)
        {
            try
            {
                Reload.Visibility = Visibility.Collapsed;
                Step_plus();
                using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                {
                    first = db.Query<Sync_Device>("select * from Sync_Device where Date_sync != 0 and " +
                        "Synchronization = true and DeviceID = " + current_device.Id);
                    //если это первая синхронизация для данного устройства

                    Load.Visibility = Visibility.Collapsed;
                    About_Sync.Text = "Текущее активное устройство - " + current_device.Name + " " + '"' + current_device.Model + '"';
                    CopySong.Text = "\t - " + copy_device + " файл(ов) будет перенесено на устройство";
                    ProgressBar.Visibility = Visibility.Visible;
                    Accept_button.Visibility = Visibility.Visible;
                }
                if (first.Count != 0)
                {
                    DeleteComputer.Text = "\t - " + delete_comp + " файл(ов) будет удалено с компьютера";
                    DeleteDevice.Text = "\t - " + delete_device + " файл(ов) будет удалено с устройства";
                    Check.Visibility = Visibility.Visible;
                }
            }
            catch
            {

            }
        }

        private void Step_plus()
        {
            step = 100 / (delete_comp + delete_device + copy_device);
            status.Maximum = step * (delete_comp + delete_device + copy_device);
        }

        private async void Accept_Click(object sender, RoutedEventArgs e)
        {

            var name = "Device" + Current_device.Id;
            var Device_folder = await Windows.Storage.AccessCache.StorageApplicationPermissions
                .FutureAccessList.GetFolderAsync(name);
            var Audio = await Windows.Storage.AccessCache.StorageApplicationPermissions
                .FutureAccessList.GetFolderAsync("Audio");

            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                if (copy_device != 0)
                {
                    var songs = db.Query<Sync_Device>("Select * from Sync_Device where Date_sync = 0 and " +
                        "Synchronization = true and DeviceID = " + Current_device.Id).ToList();
                    foreach (var song in songs)
                    {
                        //править проверку 
                        var temp_song = db.Find<Song>(c => c.SongID == song.SongID && c.Deleted == false);
                        var format = db.Find<Music_format>(c => c.Id == temp_song.Format_Id);
                        var old_file = Audio.Path + "\\" + temp_song.Path + temp_song.Name_song + "." + format.Name_format;
                        string fileName = temp_song.Name_song + "." + format.Name_format;
                        string destFile = Path.Combine(Device_folder.Path, fileName);
                        await Task.Run(() =>
                        {
                            File.Copy(old_file, destFile, false); //Перемещаем новый файл
                        });

                        song.Date_sync = DateTime.Now.AddHours(5);
                        Db_Helper.Update_Sync_Device(song);
                        status.Value += step;
                        count_sync_song++;
                        Count_sync.Text = "Обработано " + count_sync_song + " из " + (delete_comp + delete_device + copy_device)
                            + " песен";
                    }
                }

                if (delete_comp != 0)
                {
                    var temp = await Find_device(Current_device);
                    var files = Find_Song(temp.Path);
                    var song_comp = db.Query<Sync_Device>("Select * from Sync_Device where " +
                        " Date_sync = 0 and DeviceID = " + Current_device.Id);
                    foreach (var item in song_comp)
                    {
                        var song = db.Find<Song>(c => c.SongID == item.SongID);
                        var format = db.Find<Music_format>(c => c.Id == song.Format_Id);
                        var text_song = temp.Path + "\\" + song.Name_song + "." + format.Name_format;
                        if (!files.Contains(text_song))
                        {
                            var temp_path = Audio.Path + "\\" + song.Path + song.Name_song + "." + format.Name_format;
                            await Task.Run(() =>
                            {
                                File.Delete(temp_path);
                            });
                            Db_Helper.Delete_Song_in_Device_sync(item.SongID); //удаляем из таблицы синхронизации
                            status.Value += step;
                            count_sync_song++;
                            Count_sync.Text = "Обработано " + count_sync_song + " из " + (delete_comp + delete_device + copy_device) + " песен";

                            var exist = db.Query<Sync_Device>("Select * from Sync_Device where SongID = " + item.SongID +
                                " and Date_sync = 0");
                            if (exist.Count() == 0)
                            {
                                Db_Helper.DeleteSong(song.SongID);
                                Db_Helper.DeleteTag(song.TagId);
                            }
                        }
                    }
                }

                if (delete_device != 0)
                {
                    var songs_comp = db.Query<Song>("Select * from Song where Deleted = true");
                    var temp = await Find_device(Current_device);
                    var files = Find_Song(temp.Path);
                    foreach (var item in songs_comp)
                    {
                        foreach (string file in files)
                        {
                            var format = db.Find<Music_format>(c => c.Id == item.Format_Id);
                            var text_song = temp.Path + "\\" + item.Name_song + "." + format.Name_format;
                            if (file == text_song)
                            {
                                await Task.Run(() =>
                                {
                                    File.Delete(file);
                                });
                                Db_Helper.Delete_Song_in_Device_sync(item.SongID); //удаляем из таблицы синхронизации
                                status.Value += step;
                                count_sync_song++;
                                Count_sync.Text = "Обработано " + count_sync_song + " из " + (delete_comp + delete_device + copy_device) + " песен";
                            }

                        }
                        var exist = db.Query<Sync_Device>("Select * from Sync_Device where SongID = " + item.SongID +
                            " and Date_sync = 0 and Synchronization = true");
                        if (exist.Count() == 0)
                        {
                            Db_Helper.DeleteSong(item.SongID);
                            Db_Helper.DeleteTag(item.TagId);
                        }
                    }
                }
            }

            var dialog = new MessageDialog("Данные синхронизированы");
            await dialog.ShowAsync();
            Frame.Navigate(typeof(Sync));
        }
    }
}
