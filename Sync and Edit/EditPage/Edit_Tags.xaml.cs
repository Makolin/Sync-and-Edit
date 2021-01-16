using SQLite;
using Sync_and_Edit.DataBase;
using Sync_and_Edit.Json;
using Sync_and_Edit.Menu;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Sync_and_Edit.EditPage
{
    public sealed partial class Edit_Tags : Page
    {
        Json_options Json = new Json_options();
        Song CurrentSong = new Song();
        DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
        static string audioteca;
        static string text_audioteca;
        StorageFolder Main_folder;

        public Edit_Tags()
        {
            this.Read_Json();
            this.Loaded += ReadSongList_Loaded;
            this.InitializeComponent();
            Edit_Page.my_list_box.SelectedIndex = 1;
        }

        private async void Read_Json()
        {
            Json = await Json.Read_Json();
            audioteca = Json.Json_audioteca;
            text_audioteca = Json.Json_audioteca.Split('\\').Last();
        }

        private void ReadSongList_Loaded(object sender, RoutedEventArgs e)
        {

            ObservableCollection<Song> DB_SongList = new ObservableCollection<Song>(); //его обновлять
            ReadAllSongList dbsongs = new ReadAllSongList();
            DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
            DB_SongList = dbsongs.GetAllSongs();
            try
            {
                SongList.ItemsSource = DB_SongList.OrderBy(i => i.Name_song).ToList();
                Statictik.Text = "Количество песен в базе: " + DB_SongList.Count.ToString();
            }
            catch { }
        }


        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Edit_Files));
        }

        private void Clear_Tags()
        {
            //tb_Genge.Text = String.Empty;
            tb_Number.Text = String.Empty;
            tb_Albom.Text = String.Empty;
            tb_Artist.Text = String.Empty;
            //tb_Composer.Text = String.Empty;
            tb_Name_song.Text = String.Empty;
            tb_Year.Text = String.Empty;
            tb_Artist_Albom.Text = String.Empty;
        }

        private void SongList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Tags_edit.IsEnabled = true;
            CurrentSong = SongList.SelectedItem as Song;
            Clear_Tags();
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var CurrentTag = db.Find<Tag>(c => c.Id == CurrentSong.TagId);
                /*if (CurrentTag.Genge != null)
                {
                    tb_Genge.Text = CurrentTag.Genge;
                }*/
                if (CurrentTag.Track_Number.ToString() != null)
                {
                    tb_Number.Text = CurrentTag.Track_Number.ToString();
                }
                if (CurrentTag.Albom != null)
                {
                    tb_Albom.Text = CurrentTag.Albom;
                }
                if (CurrentTag.Artist != null)
                {
                    tb_Artist.Text = CurrentTag.Artist;
                }
                /*if (CurrentTag.Composer != null)
                {
                    tb_Composer.Text = CurrentTag.Composer;
                }*/
                if (CurrentTag.Name_song != null)
                {
                    tb_Name_song.Text = CurrentTag.Name_song;
                }
                if (CurrentTag.Year.ToString() != null)
                {
                    tb_Year.Text = CurrentTag.Year.ToString();
                }
                if (CurrentTag.Artist_Albom != null)
                {
                    tb_Artist_Albom.Text = CurrentTag.Artist_Albom;
                }
            }
        }

        private async void Tags_edit_Click(object sender, RoutedEventArgs e)
        {
            bool pometka_rename = false;
            bool pometka = false;
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var CurrentTag = db.Find<Tag>(c => c.Id == CurrentSong.TagId);
                //if (tb_Genge.Text != CurrentTag.Genge)
                //{
                //    CurrentTag.Genge = tb_Genge.Text;
                //    pometka = true;
                //}
                if (tb_Number.Text != CurrentTag.Track_Number.ToString())
                {
                    if (tb_Number.Text != "")
                    {
                        CurrentTag.Track_Number = Convert.ToInt32(tb_Number.Text);
                    }
                    else
                    {
                        CurrentTag.Track_Number = 0;
                    }
                    pometka = true;
                }
                if (tb_Albom.Text != CurrentTag.Albom)
                {
                    CurrentTag.Albom = tb_Albom.Text;
                    pometka = true;
                }
                if (tb_Artist.Text != CurrentTag.Artist)
                {
                    CurrentTag.Artist = tb_Artist.Text;
                    pometka_rename = true;
                }
                //if (tb_Composer.Text!=CurrentTag.Composer)
                //{
                //    CurrentTag.Composer = tb_Composer.Text;
                //    pometka = true;
                //}
                if (tb_Name_song.Text != CurrentTag.Name_song)
                {
                    CurrentTag.Name_song = tb_Name_song.Text;
                    pometka_rename = true;
                }
                if (tb_Year.Text != CurrentTag.Year.ToString())
                {
                    CurrentTag.Year = Convert.ToInt32(tb_Year.Text);
                    pometka = true;
                }
                if (tb_Artist_Albom.Text != CurrentTag.Artist_Albom)
                {
                    CurrentTag.Artist_Albom = tb_Artist_Albom.Text;
                    pometka = true;
                }
                if (pometka_rename || pometka)
                {
                    if (pometka_rename)
                    {
                        if (Check(CurrentTag))
                        {
                            Db_Helper.Update_Tag(CurrentTag); //Сохраняем в базу
                            Rename(CurrentTag);
                        }
                        else
                        {
                            await Delete_Copy(CurrentTag);
                            Frame.Navigate(typeof(Edit_Tags));
                        }
                    }
                    if (pometka_rename == false && pometka)
                    {
                        Db_Helper.Update_Tag(CurrentTag); //Сохраняем в базу
                        Rename(CurrentTag);
                    }
                }
            }
        }

        private async Task Delete_Copy(Tag currentTag)
        {
            ContentDialog Dialog = new ContentDialog
            {
                Title = "Песня с таким тегом уже существует",
                Content = "Удалить копию?",
                PrimaryButtonText = "Да",
                SecondaryButtonText = "Нет"
            };
            ContentDialogResult result = await Dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                {
                    var song = db.Find<Song>(c => c.TagId == currentTag.Id);
                    Db_Helper.DeleteSong(song.SongID);
                    Db_Helper.DeleteTag(song.TagId);
                    Db_Helper.Delete_Song_in_Device_sync(song.SongID); //удаляем из таблицы синхронизации

                    var temp_format = db.Find<Music_format>(c => c.Id == song.Format_Id);
                    var file = audioteca + "\\" + song.Path + song.Name_song + "." + temp_format.Name_format;

                    await Task.Run(() =>
                    {
                        File.Delete(file); //удаляем старый файл
                        });

                }
            }
            if (result == ContentDialogResult.Secondary)
            {

            }
        }

        private bool Check(Tag mytag)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var exist_tag = db.Find<Tag>(c => c.Name_song == mytag.Name_song && c.Artist == mytag.Artist);
                if (exist_tag != null)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
        }

        private async void Rename(Tag mytag)
        {
            CurrentSong = SongList.SelectedItem as Song; //Текущая выбранная песня
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                string import_name;
                var temp_format_id = db.Find<Music_format>(c => c.Id == CurrentSong.Format_Id);
                import_name = CurrentSong.Path + CurrentSong.Name_song + "." + temp_format_id.Name_format;

                Main_folder = await Windows.Storage.AccessCache.StorageApplicationPermissions
                .FutureAccessList.GetFolderAsync("Audio");
                StorageFile music_files = await Main_folder.GetFileAsync(import_name);

                var fileProperties = await music_files.Properties.GetMusicPropertiesAsync();

                //*Нормально не изменяет артиста
                StorageItemContentProperties rfileProperties = music_files.Properties;
                MusicProperties musicFileProperties = await rfileProperties.GetMusicPropertiesAsync();
                string[] contributingArtistsKey = { "System.Music.Artist" };
                IDictionary<string, object> contributingArtistsProperty =
                    await musicFileProperties.RetrievePropertiesAsync(contributingArtistsKey);
                string[] contributingArtists = contributingArtistsProperty["System.Music.Artist"] as string[];

                //fileProperties.Genre = mytag.Genge; \\только для чтения
                fileProperties.TrackNumber = Convert.ToUInt32(mytag.Track_Number);
                fileProperties.Album = mytag.Albom;
                fileProperties.Artist = mytag.Artist;
                //fileProperties.Composers = mytag.Composer; \\только для чтения
                fileProperties.Title = mytag.Name_song;
                fileProperties.Year = Convert.ToUInt32(mytag.Year);
                fileProperties.AlbumArtist = mytag.Artist_Albom;
                //*Ошибка доступа для Флер
                await fileProperties.SavePropertiesAsync();

                //Обновление даты изменения файла
                var temp_song = db.Find<Song>(c => c.TagId == mytag.Id);
                await Task.Run(() =>
                {
                    temp_song.Date_change = DateTime.Now.AddHours(5);
                    Db_Helper.Update_Song(temp_song);
                });
            }
            var dialog = new MessageDialog("Тег изменен");
            await dialog.ShowAsync();
        }
    }
}
