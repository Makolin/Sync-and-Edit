using Sync_and_Edit.DataBase;
using Sync_and_Edit.Menu;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Sync_and_Edit.SyncPage
{

    public sealed partial class Devices : Page
    {
        ObservableCollection<Device> DB_DeviceList = new ObservableCollection<Device>();
        DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
        public Devices()
        {

            this.InitializeComponent();
            this.Loaded += ReadDeviceList_Loaded;
            Sync_Page.my_list_box.SelectedIndex = 0;
            this.Active();

        }

        private void Active()
        {
            if (Sync_Page.Main_Current_Device.Model != null)
            {
                DeviceList.SelectedItem = 0;
            }
        }

        private void ReadDeviceList_Loaded(object sender, RoutedEventArgs e)
        {
            ReadAllDeviceList dbdevices = new ReadAllDeviceList();
            DB_DeviceList = dbdevices.GetAllDevices();
            DeviceList.ItemsSource = DB_DeviceList.OrderByDescending(i => i.Id).ToList();
        }

        private void New_device_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(New_Device));
        }
        private void Edit_device_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Edit_Device));
        }

        private async void Delete_device_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Удалить " + Sync_Page.Main_Current_Device.Name + " " + Sync_Page.Main_Current_Device.Model + " из базы? ");
            dialog.Commands.Add(new UICommand { Label = "Да", Id = 0 });
            dialog.Commands.Add(new UICommand { Label = "Нет", Id = 1 });
            var result = await dialog.ShowAsync();
            if ((int)result.Id == 0)
            {
                Db_Helper.DeleteDevice(Sync_Page.Main_Current_Device.Id);
                Db_Helper.DeleteDevice_format(Sync_Page.Main_Current_Device.Id);
                Db_Helper.DeleteDevice_sync(Sync_Page.Main_Current_Device.Id);
                Frame.Navigate(typeof(Devices));
            }
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Edit));
        }

        private void deviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeviceList.SelectedItem != null)
            {
                Sync_Page.Main_Current_Device = DeviceList.SelectedItem as Device;
                delete_device.IsEnabled = true;
                edit_device.IsEnabled = true;
            }
            else
            {
                delete_device.IsEnabled = false;
                edit_device.IsEnabled = false; //кнопку нельзя использовать

            }
        }
    }
}
