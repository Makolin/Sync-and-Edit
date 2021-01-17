using SQLite;
using Sync_and_Edit.DataBase;
using Sync_and_Edit.Menu;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Sync_and_Edit.SyncPage
{
    public sealed partial class Edit : Page
    {
        ObservableCollection<Combinated_Sync> DB_SуnсList;
        DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
        Device CurrentDevice = new Device();
        double size_song = 0;
        static double chislo;
        bool all = true; // введна, чтобы при измении одной галочки не слимались все, после снятия галочки ALL
        List<bool> List_Convert = new List<bool>();
        List<bool> List_Sync = new List<bool>();
        public Edit()
        {
            this.Loaded += ReadDeviceList_Loaded;
            this.InitializeComponent();
        }

        //выполняет слишком много раз
        private void MainText_Checkbox()
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                foreach (var song in DB_SуnсList)
                {
                    if (song.Synchronization == true)
                    {
                        var temp_song = db.Find<Song>(c => c.SongID == song.SongID);
                        if (song.FormatToFormat == true)
                        {
                            //пока что примерно, так как нет формулы для определения размера
                            size_song += (temp_song.Size / 3.3);
                        }
                        else
                        {
                            size_song += temp_song.Size;
                        }
                    }
                }
            }

            size_song = Math.Round(size_song / 1024 / 1024 / 1024, 2);
            chislo = Math.Round((100 - (100 * (CurrentDevice.Memory - size_song)) / CurrentDevice.Memory), 0);
            InCircle.Text = (chislo + "%").ToString();
            string text = (CurrentDevice.Memory - size_song).ToString();
            Percent.Text = "На устройстве будет свободно " + text + " Gb";
            //RadialProgressBarControl.Value = (size_song * 100 / CurrentDevice.Memory);
        }


        private void Load_checked(bool click)
        {
            foreach (var song in DB_SуnсList)
            {
                List_Convert.Add(song.FormatToFormat);
                List_Sync.Add(song.Synchronization);
            }
            if (!List_Convert.Contains(false))
            {
                if (click == false)
                {
                    all = true;
                }
                AllConvert.IsChecked = true;
            }

            if (!List_Sync.Contains(false))
            {
                if (click == false)
                {
                    all = true;
                }
                AllSync.IsChecked = true;
            }
            List_Convert.Clear();
            List_Sync.Clear();
        }

        //Вывод списка всех песен относящихся к выбранному устройству
        private void ReadSongList_Loaded()
        {

            ReadAllCombinatedList dbsync = new ReadAllCombinatedList();
            DB_SуnсList = dbsync.GetAllCombinated(CurrentDevice);
            try
            {
                SongList.ItemsSource = DB_SуnсList;
                MainText_Checkbox();
                Load_checked(false);
            }
            catch
            {

            }

        }

        //Для отображения списка устройств и выбора активного в комбобоксе
        private void ReadDeviceList_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Device> ComboList = new ObservableCollection<Device>();
            ReadAllDeviceList dbdevices = new ReadAllDeviceList();
            DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
            ComboList = dbdevices.GetAllDevices();
            try
            {
                ComboBox.ItemsSource = ComboList.OrderBy(i => i.Id).ToList();
                if (Sync_Page.Main_Current_Device.Model != null)
                {
                    CurrentDevice = Sync_Page.Main_Current_Device;
                    foreach (var device in ComboList)
                    {
                        if (device.Name == CurrentDevice.Name)
                        {
                            var index = ComboList.IndexOf(device);
                            ComboBox.SelectedIndex = index;
                        }
                    }
                    ReadSongList_Loaded();
                }
                else
                {
                    Percent.Text = "На устройстве будет свободно __ Gb";
                }
            }
            catch
            {
                //Пустое значение
            }
        }

        private async void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (chislo > 99)
            {
                var dialog = new MessageDialog("На устройстве не хватает памяти");
                await dialog.ShowAsync();
            }
            else
            {
                if (!Check())
                {
                    Last_Dialog();
                }
                else
                {
                    Insert();
                }
            }
        }

        private async void Insert()
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                foreach (var item in DB_SуnсList)
                {
                    var change = db.Find<SyncDevice>(c => c.SongID == item.SongID && c.DeviceID == item.DeviceID);
                    if (change.FormatToFormat != item.FormatToFormat || change.Synchronization != item.Synchronization)
                    {
                        change.FormatToFormat = item.FormatToFormat;
                        change.Synchronization = item.Synchronization;
                        Db_Helper.Update_Sync_Device(change);
                    }
                }
            }

            var dialog = new MessageDialog("Данные сохранены");
            await dialog.ShowAsync();
            Frame.Navigate(typeof(Sync));
        }

        private bool Check()
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                foreach (var item in DB_SуnсList)
                {
                    var song_format = db.Find<Song>(c => c.SongID == item.SongID);
                    var exist_format = db.Find<DeviceFormat>(c => c.DeviceID == item.DeviceID &&
                        c.MusicFormatID == song_format.FormatId);
                    if (exist_format == null && item.FormatToFormat == false)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private async void Last_Dialog()
        {
            ContentDialog noWifiDialog = new ContentDialog
            {
                Title = "Не поддерживает данный аудиоформат",
                Content = "Некоторые файлы не будут воспроизведены на устройстве, произвести их конвертирование?",
                PrimaryButtonText = "Да",
                SecondaryButtonText = "Нет"
            };
            ContentDialogResult result = await noWifiDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                foreach (var item in DB_SуnсList)
                {
                    using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                    {
                        var song_format = db.Find<Song>(c => c.SongID == item.SongID);
                        var exist_format = db.Find<DeviceFormat>(c => c.DeviceID == item.DeviceID &&
                            c.MusicFormatID == song_format.FormatId);
                        if (exist_format == null)
                        {
                            item.FormatToFormat = true;
                        }
                    }
                }
                Insert();
            }
            if (result == ContentDialogResult.Secondary)
            {
                Insert();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox.IsEnabled = true;
            CurrentDevice = ComboBox.SelectedItem as Device;
            ReadSongList_Loaded();
            MainText_Checkbox();
        }



        private void Work_with_SongList(string type, bool value)
        {
            try
            {
                for (int i = 0; i < DB_SуnсList.Count; i++)
                {
                    if (type == "Sync")
                    {
                        DB_SуnсList[i].Synchronization = value;
                    }
                    if (type == "Convert")
                    {
                        DB_SуnсList[i].FormatToFormat = value;
                    }
                }
                //SongList.ItemsSource = DB_SуnсList;
            }
            catch
            {
                //Пустое значение
            }
        }

        private void AllSync_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (all == true)
                {
                    Work_with_SongList("Sync", true);
                    MainText_Checkbox();
                    SongList.ItemsSource = null;
                    SongList.ItemsSource = DB_SуnсList;
                }
                all = true;
            }
            catch
            {

            }

        }

        private void AllSync_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (all == true)
                {
                    Work_with_SongList("Sync", false);
                    MainText_Checkbox();
                    SongList.ItemsSource = null;
                    SongList.ItemsSource = DB_SуnсList;
                }
                all = true;
            }
            catch
            {

            }
        }

        private void AllConvert_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (all == true)
                {
                    Work_with_SongList("Convert", true);
                    MainText_Checkbox();
                    SongList.ItemsSource = null;
                    SongList.ItemsSource = DB_SуnсList;
                }
                all = true;
            }
            catch
            {

            }

        }
        private void AllConvert_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (all == true)
                {
                    Work_with_SongList("Convert", false);
                    MainText_Checkbox();
                    SongList.ItemsSource = null;
                    SongList.ItemsSource = DB_SуnсList;
                }
                all = true;
            }
            catch
            {

            }

        }

        private void Formats_Checked(object sender, RoutedEventArgs e)
        {
            MainText_Checkbox();
            Load_checked(false);
        }

        private void Sync_Checked(object sender, RoutedEventArgs e)
        {
            MainText_Checkbox();
            Load_checked(false);
        }

        private void Sync_Unchecked(object sender, RoutedEventArgs e)
        {
            MainText_Checkbox();
            if (AllSync.IsChecked == true)
            {
                all = false;
                AllSync.IsChecked = false;
            }
        }

        private void Formats_Unchecked(object sender, RoutedEventArgs e)
        {
            MainText_Checkbox();
            if (AllConvert.IsChecked == true)
            {
                all = false;
                AllConvert.IsChecked = false;
            }
        }
    }
}
