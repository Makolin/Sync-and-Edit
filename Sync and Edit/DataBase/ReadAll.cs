using System.Collections.ObjectModel;

namespace Sync_and_Edit.DataBase
{
    class ReadAllDeviceList
    {
        DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
        public ObservableCollection<Device> GetAllDevices()
        {
            return Db_Helper.ReadAllDevices();
        }
    }
    class ReadAllSongList
    {
        DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
        public ObservableCollection<Song> GetAllSongs()
        {
            return Db_Helper.ReadAllSongs();
        }
    }
    class ReadAllSyncList
    {
        DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
        public ObservableCollection<SyncDevice> GetAllSync()
        {
            return Db_Helper.ReadAllSync();
        }
    }
    class ReadAllCombinatedList
    {
        DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
        public ObservableCollection<Combinated_Sync> GetAllCombinated(Device CurrentDevice)
        {
            return Db_Helper.ReadAllCombinated(CurrentDevice);
        }
    }
}
