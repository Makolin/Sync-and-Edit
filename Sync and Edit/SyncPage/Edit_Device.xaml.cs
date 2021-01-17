using SQLite;
using Sync_and_Edit.DataBase;
using Sync_and_Edit.Menu;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


namespace Sync_and_Edit.SyncPage
{
    public sealed partial class Edit_Device : Page
    {
        string[] ListFormat = new string[] { "mp3", "flac", "alac", "m4a", "ape", "ogg", "wav", "wma", "aac" };
        DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
        Device EditDevice = Sync_Page.Main_Current_Device;
        List<int> OldFormat = new List<int>();
        List<int> NewFormat = new List<int>();

        public Edit_Device()
        {
            this.InitializeComponent();
            Check();
        }

        private async void Check()
        {
            try
            {
                var name = "Device" + EditDevice.Id;
                var Main_folder = await Windows.Storage.AccessCache.StorageApplicationPermissions
                    .FutureAccessList.GetFolderAsync(name);
                Source.Text = Main_folder.Path;
                if (Show_Device_music(1))
                {
                    OldFormat.Add(1);
                    Format_0.IsChecked = true;
                }
                if (Show_Device_music(2))
                {
                    OldFormat.Add(2);
                    Format_1.IsChecked = true;
                }
                if (Show_Device_music(3))
                {
                    OldFormat.Add(3);
                    Format_2.IsChecked = true;
                }
                if (Show_Device_music(4))
                {
                    OldFormat.Add(4);
                    Format_3.IsChecked = true;
                }
                if (Show_Device_music(5))
                {
                    OldFormat.Add(5);
                    Format_4.IsChecked = true;
                }
                if (Show_Device_music(6))
                {
                    OldFormat.Add(6);
                    Format_5.IsChecked = true;
                }
                if (Show_Device_music(7))
                {
                    OldFormat.Add(7);
                    Format_6.IsChecked = true;
                }
                if (Show_Device_music(8))
                {
                    OldFormat.Add(8);
                    Format_7.IsChecked = true;
                }
                if (Show_Device_music(9))
                {
                    OldFormat.Add(9);
                    Format_8.IsChecked = true;
                }
            }
            catch
            {
                var dialog = new MessageDialog("Устройство не подключено");
                await dialog.ShowAsync();
                Frame.Navigate(typeof(Devices));
            }

        }

        private bool Show_Device_music(int formatId)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var exist = db.Find<DeviceFormat>(c => c.DeviceID == EditDevice.Id && c.MusicFormatID == formatId);
                if (exist == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private void Enter_Device_music(int formatId)
        {
            Db_Helper.Insert_Device_Format(new DeviceFormat(EditDevice.Id, formatId));
        }

        private async void Save_file()
        {
            var name = "Device" + EditDevice.Id;
            var Main_folder = await Windows.Storage.AccessCache.StorageApplicationPermissions
                .FutureAccessList.GetFolderAsync(name);
            StorageFile id = await Main_folder.CreateFileAsync(name + ".ini", CreationCollisionOption.ReplaceExisting);
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
                //храним пути до папок
                var name = "Device" + EditDevice.Id;
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                    FutureAccessList.AddOrReplace(name, folder);
                Source.Text = folder.Path;
            }
            else
            {

            }
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
                    EditDevice.Name = Name_device.Text;
                    EditDevice.Model = Model.Text;
                    EditDevice.Memory = Convert.ToDouble(Size.Text);
                    Db_Helper.Update_Device(EditDevice);
                    //Апдейт device

                    if (Format_0.IsChecked == true)
                    {
                        NewFormat.Add(1);
                        Enter_Device_music(1);
                    }
                    if (Format_1.IsChecked == true)
                    {
                        NewFormat.Add(2);
                        Enter_Device_music(2);
                    }
                    if (Format_2.IsChecked == true)
                    {
                        NewFormat.Add(3);
                        Enter_Device_music(3);
                    }
                    if (Format_3.IsChecked == true)
                    {
                        NewFormat.Add(4);
                        Enter_Device_music(4);
                    }
                    if (Format_4.IsChecked == true)
                    {
                        NewFormat.Add(5);
                        Enter_Device_music(5);
                    }
                    if (Format_5.IsChecked == true)
                    {
                        NewFormat.Add(6);
                        Enter_Device_music(6);
                    }
                    if (Format_6.IsChecked == true)
                    {
                        NewFormat.Add(7);
                        Enter_Device_music(7);
                    }
                    if (Format_7.IsChecked == true)
                    {
                        NewFormat.Add(8);
                        Enter_Device_music(8);
                    }

                    if (Format_8.IsChecked == true)
                    {
                        NewFormat.Add(9);
                        Enter_Device_music(9);
                    }
                    EditFormats(OldFormat, NewFormat);
                    Save_file(); //
                    var dialog = new MessageDialog("Устройство успешно изменено");
                    dialog.Commands.Add(new UICommand { Label = "Okay", Id = 0 });
                    await dialog.ShowAsync();
                    Frame.Navigate(typeof(Devices));
                }
            }
        }
        private void EditFormats(List<int> oldformat, List<int> newformat)
        {
            foreach (int i in oldformat)
            {
                if (!newformat.Contains(i))
                {

                    Db_Helper.DeleteFormat(EditDevice.Id, i);
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
    }
}
