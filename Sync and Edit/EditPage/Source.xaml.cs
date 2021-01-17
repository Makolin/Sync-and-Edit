using SQLite;
using Sync_and_Edit.DataBase;
using Sync_and_Edit.Json;
using Sync_and_Edit.Menu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


namespace Sync_and_Edit.EditPage
{
    public sealed partial class Source : Page
    {
        static bool check_copy = true;
        static int count_source;
        static string textAudioteca, text_source;
        static int count_song; //Используется для подсчета перенесенных песен
        static int count_song_insert; //Используется для подсчета перенесенных песен
        static int count_song_up; //Используется для подсчета перенесенных песен
        static int count_delete_song;
        Json_options Json = new Json_options();
        DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
        StorageFolder Main_folder;

        public Source()
        {
            count_source = 1;
            this.Read_Json();
            this.InitializeComponent();
            count_song = 0;
            count_song_insert = 0;
            count_song_up = 0;
            count_delete_song = 0;
            Edit_Page.my_list_box.SelectedIndex = 0;
        }
        private async void Read_Json()
        {
            Json = await Json.ReadJson();
            if (Json.Json_source_1 != string.Empty)
            {
                Source_1.Text = Json.Json_source_1;
            }
            if (Json.Json_source_2 != string.Empty)
            {
                Source_2.Text = Json.Json_source_2;
            }
            if (Json.Json_source_3 != string.Empty)
            {
                Source_3.Text = Json.Json_source_3;
            }
            if (Json.Json_audioteca != string.Empty)
            {
                Audio.Text = Json.Json_audioteca;
                textAudioteca = Json.Json_audioteca.Split('\\').Last();
            }
        }

        public static List<string> FindSongInDirectory(string path)
        {
            try
            {
                var files = Directory
                .EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(file => file.ToLower().EndsWith("mp3") ||
                file.ToLower().EndsWith("m4a") || file.ToLower().EndsWith("flac") ||
                file.ToLower().EndsWith("alac") || file.ToLower().EndsWith("ogg") ||
                file.ToLower().EndsWith("wav") || file.ToLower().EndsWith("wma") ||
                file.ToLower().EndsWith("ape") || file.ToLower().EndsWith("aac"))
                .ToList();
                return files;
            }
            catch { }

            return null;
        }

        private void CheckBox_Copy(object sender, TappedRoutedEventArgs e)
        {
            if (Copy.IsChecked == true)
            {
                check_copy = true;
            }
            else
            {
                check_copy = false;
            }
        }

        // Если найден файл с лучшим качеством записи
        private async Task Update_Audio(StorageFolder folder, string new_file, Song old_song, string path, string audio, int bitrait)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var format = db.Find<MusicFormat>(c => c.Id == old_song.FormatId);
                var old_file = audio + "\\" + old_song.NameSong + "." + format.NameFormat;
                await Task.Run(() =>
                {
                    File.Delete(old_file); 
                });

                string fileName = Path.GetFileName(new_file);
                string destFile = Path.Combine(audio, fileName);
                await Task.Run(() =>
                {
                    File.Copy(new_file, destFile, false);
                });

                string temp_format = Path.GetExtension(new_file).Replace(".", "");
                var insert_format = db.Find<MusicFormat>(c => c.NameFormat == temp_format);

                string import_name = Path.GetFullPath(new_file).Replace(path, "").Substring(1);
                StorageFile music_files = await folder.GetFileAsync(import_name);
                MusicProperties musicProperties = await music_files.Properties.GetMusicPropertiesAsync();

                StringBuilder fileProperties = new StringBuilder();
                BasicProperties basicProperties = await music_files.GetBasicPropertiesAsync();
                int fileSize = Convert.ToInt32(basicProperties.Size);

                await Task.Run(() =>
                {
                    old_song.FormatId = insert_format.Id;
                    old_song.Bitrait = bitrait;
                    old_song.Size = fileSize;
                    Db_Helper.Update_Song(old_song);
                });
            }
            count_song_up++;
        }

        private async Task Move_InsertDB(string path, string audio, StorageFolder folder)
        {
            DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var files = FindSongInDirectory(path);
                if (files != null)
                {
                    foreach (string file in files)
                    {
                        await Task.Run(() =>
                        {
                            // Убираем атрибуты "только для чтения"
                            File.SetAttributes(file, System.IO.FileAttributes.Archive);
                        });

                        string fileName = Path.GetFileName(file);
                        string destFile = Path.Combine(audio, fileName);
                        string name_song = Path.GetFileNameWithoutExtension(file);//Имя файла без формата
                        string import_path = Path.GetDirectoryName(file);
                        if (import_path == path)
                        {
                            import_path = "";
                        }
                        else
                        {
                            import_path = import_path.Replace(path + @"\", "") + @"\";
                            //Получаются пути пример Земфира\
                        }
                        // Проверка на наличие данного названия трека в базе данных
                        var exist = db.Find<Song>(c => c.NameSong == name_song);
                        
                        if (exist == null)
                        {
                            string temp_format = Path.GetExtension(file).Replace(".", ""); //Формат файла
                            text_source = path.Split('\\').Last(); //имя папка откуда файл
                            var temp_format_id = db.Find<MusicFormat>(c => c.NameFormat == temp_format);
                            if (temp_format_id != null) //Если данный аудиоформат есть в базе данных
                            {
                                string import_name = Path.GetFullPath(file).Replace(path, "").Substring(1);
                                StorageFile music_files = await folder.GetFileAsync(import_name);
                                MusicProperties musicProperties = await music_files.Properties.GetMusicPropertiesAsync();

                                StringBuilder fileProperties = new StringBuilder(); //размер файла в байтах
                                BasicProperties basicProperties = await music_files.GetBasicPropertiesAsync();
                                int fileSize = Convert.ToInt32(basicProperties.Size);

                                if (path != audio) //если источник отличен от папки Аудиотеки
                                {
                                    await Task.Run(() =>
                                    {
                                        File.Copy(file, destFile, false);
                                        count_song++;
                                    });
                                }

                                await Task.Run(() =>
                                {
                                    var tag_genge = musicProperties.Genre.ToString(); //не считывает нормально 
                                        if (tag_genge == "System.__ComObject")
                                    {
                                        tag_genge = null;
                                    }
                                    var tag_composer = musicProperties.Composers.ToString(); //не считатывает нормально 
                                        if (tag_composer == "System.__ComObject")
                                    {
                                        tag_composer = null;
                                    }
                                    Db_Helper.Insert_Tag(new Tag(Convert.ToInt32(musicProperties.TrackNumber), musicProperties.Artist,
                                        musicProperties.Title, musicProperties.Album, musicProperties.AlbumArtist,
                                        Convert.ToInt32(musicProperties.Year), tag_genge,
                                        tag_composer));
                                });
                                var temp_tag_id = db.Query<Tag>("select * from Tag").Last(); //находим последний добавленный Тег
                                if (temp_format_id != null) //Если такой формат найден
                                {
                                    await Task.Run(() =>
                                    {
                                        Db_Helper.Insert_Song(new Song(temp_tag_id.Id, name_song,
                                            temp_format_id.Id, File.GetLastWriteTime(file)
                                            , Convert.ToInt32(musicProperties.Bitrate), fileSize, import_path, false));
                                        count_song_insert++;
                                    });
                                    var Devices_exist = db.Query<Device>("select * from Device").ToList();
                                    if (Devices_exist.Count() != 0)
                                    {
                                        var song = db.Query<Song>("select * from Song").Last();
                                        foreach (var device in Devices_exist)
                                        {
                                            Db_Helper.Insert_Sync_Device(new SyncDevice(device.Id, song.SongID));
                                        }
                                    }
                                }
                                else
                                {
                                    var dialog = new MessageDialog("Неизвестный формат песни " + name_song);
                                    dialog.Commands.Add(new UICommand { Label = "Ок", Id = 0 });
                                    await dialog.ShowAsync();
                                }
                            }
                        }
                        else
                        {
                            string import_name = Path.GetFullPath(file).Replace(path, "").Substring(1);

                            StorageFile music_files = await folder.GetFileAsync(import_name);
                            MusicProperties musicProperties = await music_files.Properties.GetMusicPropertiesAsync();
                            var current_song = db.Find<Song>(c => c.NameSong == name_song);
                            if (musicProperties.Bitrate != current_song.Bitrait)
                            {
                                await Best_file(folder, file, current_song, Convert.ToInt32(musicProperties.Bitrate),
                                    path, audio, current_song.NameSong + " " + current_song.Bitrait / 1000 + " кбит/c",
                                    name_song + " " + musicProperties.Bitrate / 1000 + " кбит/c");
                            }
                        }
                    }
                }
                else
                {
                    var dialog = new MessageDialog("Папки " + path + " не существует.");
                    dialog.Commands.Add(new UICommand { Label = "Okay", Id = 0 });
                    await dialog.ShowAsync();
                }
            }
        }


        private async void Source_1_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("Source_1", folder);
                this.Source_1.Text = folder.Path;
            }
            else
            {
                this.Source_1.Text = Json.Json_source_1;
            }
        }

        private async void Source_2_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("Source_2", folder);
                this.Source_2.Text = folder.Path;
            }
            else
            {
                this.Source_2.Text = Json.Json_source_2;
            }
        }

        private async void Source_3_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("Source_3", folder);
                this.Source_3.Text = folder.Path;
            }
            else
            {
                this.Source_3.Text = Json.Json_source_3;
            }
        }

        private async void Audio_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("Audio", folder);
                this.Audio.Text = folder.Path;
            }
            else
            {
                this.Audio.Text = Json.Json_audioteca;
            }
        }

        private void Add_source(object sender, RoutedEventArgs e)
        {
            count_source++;
            switch (count_source)
            {
                case 1:
                    Source_1.Visibility = Visibility.Visible;
                    Source_1_btn.Visibility = Visibility.Visible;
                    DeleteBtn.IsEnabled = true;
                    Copy.IsEnabled = true;
                    break;
                case 2:
                    Source_2.Visibility = Visibility.Visible;
                    Source_2_btn.Visibility = Visibility.Visible;
                    break;
                case 3:
                    Source_3.Visibility = Visibility.Visible;
                    Source_3_btn.Visibility = Visibility.Visible;
                    DeleteBtn.IsEnabled = true;
                    AddBtn.IsEnabled = false;
                    break;
            }
            count_song = 0;
        }

        private void Delete_source(object sender, RoutedEventArgs e)
        {
            count_source--;
            switch (count_source)
            {
                case 0:
                    Source_1.Visibility = Visibility.Collapsed;
                    Source_1_btn.Visibility = Visibility.Collapsed;
                    Copy.IsEnabled = false;
                    DeleteBtn.IsEnabled = false;
                    break;
                case 1:
                    Source_2.Visibility = Visibility.Collapsed;
                    Source_2_btn.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    Source_3.Visibility = Visibility.Collapsed;
                    Source_3_btn.Visibility = Visibility.Collapsed;
                    AddBtn.IsEnabled = true;
                    DeleteBtn.IsEnabled = true;
                    break;
            }
            count_song = 0;
        }
        private async void Accept_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                progressRing.IsActive = true;
                var temp_count = db.Query<Song>("Select * from Song");
                var files = FindSongInDirectory(Audio.Text);
                if (temp_count.Count() == 0 && files.Count != 0)
                {
                    if (Source_1.Visibility == Visibility.Collapsed)
                    {
                        Main_folder = await Windows.Storage.AccessCache.StorageApplicationPermissions
                            .FutureAccessList.GetFolderAsync("Audio");
                        await Move_InsertDB(Audio.Text, Audio.Text, Main_folder);
                    }
                    else
                    {
                        await Clear_or_Insert_Audioteca();
                    }
                }
                if (temp_count.Count() != 0 && temp_count.Count() != files.Count)
                {
                    Main_folder = await Windows.Storage.AccessCache.StorageApplicationPermissions
                        .FutureAccessList.GetFolderAsync("Audio");
                    await Move_InsertDB(Audio.Text, Audio.Text, Main_folder);
                }
                if (Source_1.Visibility == Visibility)
                {
                    Main_folder = await Windows.Storage.AccessCache.StorageApplicationPermissions
                        .FutureAccessList.GetFolderAsync("Source_1");
                    await Move_InsertDB(Source_1.Text, Audio.Text, Main_folder);
                    if (check_copy == false)
                    {
                        await ClearFolder(Source_1.Text);
                    }
                    if (Source_2.Visibility == Visibility)
                    {
                        Main_folder = await Windows.Storage.AccessCache.StorageApplicationPermissions
                        .FutureAccessList.GetFolderAsync("Source_2");
                        await Move_InsertDB(Source_2.Text, Audio.Text, Main_folder);
                        if (check_copy == false)
                        {
                            await ClearFolder(Source_2.Text);
                        }
                        if (Source_3.Visibility == Visibility)
                        {
                            Main_folder = await Windows.Storage.AccessCache.StorageApplicationPermissions
                            .FutureAccessList.GetFolderAsync("Source_3");
                            await Move_InsertDB(Source_3.Text, Audio.Text, Main_folder);
                            if (check_copy == false)
                            {
                                await ClearFolder(Source_3.Text);
                            }
                        }
                    }
                }
            }
            If_Deleted();
        }

        private async Task Best_file(StorageFolder folder, string file, Song current_song, int new_bitrait, string path, string audio,
            string file_1, string file_2)
        {
            ContentDialog Dialog = new ContentDialog
            {
                Title = "Замена файла",
                Content = "Найден музыкальный файл с одинаковым именем, заменить его?"
                + "\n" + file_1 + " ---> " + file_2,
                PrimaryButtonText = "Да",
                SecondaryButtonText = "Нет"
            };
            ContentDialogResult result = await Dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await Update_Audio(folder, file, current_song, path, audio, new_bitrait);
            }
            if (result == ContentDialogResult.Secondary)
            {
                if (path == audio)
                {
                    await Task.Run(() =>
                    {
                        File.Delete(file); //удаляем старый файл
                        });
                }
            }
        }

        private async Task Clear_or_Insert_Audioteca()
        {
            ContentDialog Dialog = new ContentDialog
            {
                Title = "Очищение папки Аудиотеки",
                Content = "Указанная папка под аудиотеку не пуста, очистить её?",
                PrimaryButtonText = "Да",
                SecondaryButtonText = "Нет"
            };
            ContentDialogResult result = await Dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ClearFolder(Audio.Text);
            }
            if (result == ContentDialogResult.Secondary)
            {
                Main_folder = await Windows.Storage.AccessCache.StorageApplicationPermissions
                .FutureAccessList.GetFolderAsync("Audio");
                await Move_InsertDB(Audio.Text, Audio.Text, Main_folder);
            }
        }

        private async Task ClearFolder(string path)
        {
            var files = FindSongInDirectory(path);
            foreach (string file in files)
            {
                await Task.Run(() =>
                {
                    File.Delete(file);
                });
            }
        }

        private void If_Deleted()
        {
            var files = FindSongInDirectory(Audio.Text);
            List<string> name_songs = new List<String>();
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var count_song_db = db.Query<Song>("Select * from Song").Count;
                if (count_song_db != files.Count && files.Count != 0)
                {
                    foreach (string file in files)
                    {
                        string name_song = Path.GetFileNameWithoutExtension(file);//Имя файла без формата
                        name_songs.Add(name_song);
                    }
                    var all_song = db.Query<Song>("Select * from Song");
                    foreach (var song in all_song)
                    {
                        if (!name_songs.Contains(song.NameSong))
                        {
                            count_delete_song++;
                            var exist = db.Query<SyncDevice>("Select * from Sync_Device where SongID == " + song.SongID +
                                " and Date_sync < '01.01.2018'");
                            if (exist.Count() != 0)
                            {
                                song.Deleted = true;
                                Db_Helper.Update_Song(song);
                            }
                            else
                            {
                                Db_Helper.DeleteSong(song.SongID);
                                Db_Helper.DeleteTag(song.TagId);
                                Db_Helper.Delete_Song_in_Device_sync(song.SongID); //удаляем из таблицы синхронизации
                            }
                        }
                    }
                }
                FinishDialog();
            }
        }

        private async void FinishDialog()
        {
            string text;
            progressRing.IsActive = false;
            Json.Write_Json(Source_1.Text, Source_2.Text, Source_3.Text, Audio.Text, Json.Json_mp3,
                Json.Json_alac, Json.Json_flac, Json.Json_aac);
            if (count_song == 0 && count_song_insert - count_song == 0 && count_song_up == 0 && count_delete_song == 0)
            {
                text = "Новых файлов нет!";
            }
            else
            {
                text = "Перенесено " + count_song + " файла(ов)." +
                "\nЗагружено из папки " + (count_song_insert - count_song) + " файла(ов)." +
                "\nИзменено " + (count_song_up) + " файла(ов)." +
                "\nУдалено " + (count_delete_song) + " файла(ов).";
            }
            ContentDialog noWifiDialog = new ContentDialog
            {
                Title = "Перенос музыкальных файлов завершен",
                Content = text,
                PrimaryButtonText = "Да"
            };
            ContentDialogResult result = await noWifiDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Frame.Navigate(typeof(Edit_Tags));
            }
        }
    }
}
