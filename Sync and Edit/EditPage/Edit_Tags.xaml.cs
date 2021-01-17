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
            Json = await Json.ReadJson();
            audioteca = Json.Json_audioteca;
            text_audioteca = Json.Json_audioteca.Split('\\').Last();
        }

        private void ReadSongList_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Song> DB_SongList = new ObservableCollection<Song>();
            ReadAllSongList dbsongs = new ReadAllSongList();
            DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
            DB_SongList = dbsongs.GetAllSongs();
            try
            {
                SongList.ItemsSource = DB_SongList.OrderBy(i => i.NameSong).ToList();
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
                if (CurrentTag.TrackNumber.ToString() != null)
                {
                    tb_Number.Text = CurrentTag.TrackNumber.ToString();
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
                if (CurrentTag.NameSong != null)
                {
                    tb_Name_song.Text = CurrentTag.NameSong;
                }
                if (CurrentTag.Year.ToString() != null)
                {
                    tb_Year.Text = CurrentTag.Year.ToString();
                }
                if (CurrentTag.ArtistAlbom != null)
                {
                    tb_Artist_Albom.Text = CurrentTag.ArtistAlbom;
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
                if (tb_Number.Text != CurrentTag.TrackNumber.ToString())
                {
                    if (tb_Number.Text != "")
                    {
                        CurrentTag.TrackNumber = Convert.ToInt32(tb_Number.Text);
                    }
                    else
                    {
                        CurrentTag.TrackNumber = 0;
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
                if (tb_Name_song.Text != CurrentTag.NameSong)
                {
                    CurrentTag.NameSong = tb_Name_song.Text;
                    pometka_rename = true;
                }
                if (tb_Year.Text != CurrentTag.Year.ToString())
                {
                    CurrentTag.Year = Convert.ToInt32(tb_Year.Text);
                    pometka = true;
                }
                if (tb_Artist_Albom.Text != CurrentTag.ArtistAlbom)
                {
                    CurrentTag.ArtistAlbom = tb_Artist_Albom.Text;
                    pometka = true;
                }
                if (pometka_rename || pometka)
                {
                    if (pometka_rename)
                    {
                        if (Check(CurrentTag))
                        {
                            Db_Helper.Update_Tag(CurrentTag);
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
                        Db_Helper.Update_Tag(CurrentTag);
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
                    Db_Helper.Delete_Song_in_Device_sync(song.SongID);

                    var temp_format = db.Find<MusicFormat>(c => c.Id == song.FormatId);
                    var file = audioteca + "\\" + song.Path + song.NameSong + "." + temp_format.NameFormat;

                    await Task.Run(() =>
                    {
                        File.Delete(file); 
                    });

                }
            }
            if (result == ContentDialogResult.Secondary) { }
        }

        private bool Check(Tag mytag)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var exist_tag = db.Find<Tag>(c => c.NameSong == mytag.NameSong && c.Artist == mytag.Artist);
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
            CurrentSong = SongList.SelectedItem as Song; 

            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                string import_name;
                var temp_format_id = db.Find<MusicFormat>(c => c.Id == CurrentSong.FormatId);
                import_name = CurrentSong.Path + CurrentSong.NameSong + "." + temp_format_id.NameFormat;

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
                fileProperties.TrackNumber = Convert.ToUInt32(mytag.TrackNumber);
                fileProperties.Album = mytag.Albom;
                fileProperties.Artist = mytag.Artist;
                //fileProperties.Composers = mytag.Composer; \\только для чтения
                fileProperties.Title = mytag.NameSong;
                fileProperties.Year = Convert.ToUInt32(mytag.Year);
                fileProperties.AlbumArtist = mytag.ArtistAlbom;
                //*Ошибка доступа для Флер
                await fileProperties.SavePropertiesAsync();

                var temp_song = db.Find<Song>(c => c.TagId == mytag.Id);
                await Task.Run(() =>
                {
                    temp_song.DateChange = DateTime.Now.AddHours(5);
                    Db_Helper.Update_Song(temp_song);
                });
            }
            var dialog = new MessageDialog("Тег изменен");
            await dialog.ShowAsync();
        }
    }
}
