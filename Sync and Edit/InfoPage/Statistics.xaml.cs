using SQLite;
using Sync_and_Edit.DataBase;
using Sync_and_Edit.Menu;
using System;
using System.Linq;
using Windows.UI.Xaml.Controls;


namespace Sync_and_Edit.InfoPage
{
    public sealed partial class Statistics : Page
    {
        public Statistics()
        {
            this.InitializeComponent();
            Info_Page.my_list_box.SelectedIndex = 1;
            Insert_data();
        }

        public void Insert_data()
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var vstavka = "     -  ";
                var count_song = db.Query<Song>("Select * from Song").Count.ToString();
                var count_device = db.Query<Device>("Select * from Device").Count.ToString();

                double song_size = 0;
                var songs = db.Query<Song>("select * from Song");
                foreach (var song in songs)
                {
                    song_size += song.Size;
                }
                song_size = Math.Round(song_size / 1024 / 1024 / 1024, 2);

                Main_stat.Text = vstavka + "аудиотека содержит " + count_song + " композиций;" + "\n" +
                    vstavka + "аудиотека занимает " + song_size + " Gb на диске;" + "\n" +
                    vstavka + "в базу внесено " + count_device + " устройств(а);";

                var count_format = db.Query<Song>("Select * from Song");
                var count_mp3 = count_format.Count(c => c.Format_Id == 1);
                var count_flac = count_format.Count(c => c.Format_Id == 2);
                var count_alac = count_format.Count(c => c.Format_Id == 3);
                var count_ape = count_format.Count(c => c.Format_Id == 4);
                var count_aac = count_format.Count(c => c.Format_Id == 5);
                var count_ogg = count_format.Count(c => c.Format_Id == 6);
                var count_wav = count_format.Count(c => c.Format_Id == 7);
                var count_wma = count_format.Count(c => c.Format_Id == 8);
                var text = "";

                if (count_mp3 != 0)
                {
                    text = vstavka + "аудиотека содержит " + count_mp3 + " аудиофайл(а) формата mp3;\n";
                }
                if (count_flac != 0)
                {
                    text += vstavka + "аудиотека содержит " + count_flac + " аудиофайл(а) формата flac;\n";
                }
                if (count_alac != 0)
                {
                    text += vstavka + "аудиотека содержит " + count_alac + " аудиофайл(а) формата alac;\n";
                }
                if (count_ape != 0)
                {
                    text += vstavka + "аудиотека содержит " + count_ape + " аудиофайл(а) формата ape;\n";
                }
                if (count_aac != 0)
                {
                    text += vstavka + "аудиотека содержит " + count_aac + " аудиофайл(а) формата aac;\n";
                }
                if (count_ogg != 0)
                {
                    text += vstavka + "аудиотека содержит " + count_ogg + " аудиофайл(а) формата ogg;\n";
                }
                if (count_wav != 0)
                {
                    text += vstavka + "аудиотека содержит " + count_wav + " аудиофайл(а) формата wav;\n";
                }
                if (count_wma != 0)
                {
                    text += vstavka + "аудиотека содержит " + count_wma + " аудиофайл(а) формата wma. \n";
                }
                Song_stat.Text = text;
            }
        }
    }
}
