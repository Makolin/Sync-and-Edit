using SQLite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sync_and_Edit.DataBase
{
    class DatabaseHelperClass
    {
        public static void CreateDatabase()
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                db.CreateTable<Device>();
                db.CreateTable<Song>();
                db.CreateTable<Tag>();
                db.CreateTable<SyncDevice>();
                db.CreateTable<MusicFormat>();
                db.CreateTable<DeviceFormat>();
            }
        }

        public static void ClearDatabase()
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                db.DeleteAll<Device>();
                db.DeleteAll<Song>();
                db.DeleteAll<Tag>();
                db.DeleteAll<SyncDevice>();
                db.DeleteAll<DeviceFormat>();
            }
        }

        // Заполнение таблицы форматов
        public void Insert(MusicFormat objFormat) 
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var existingformat = db.Find<MusicFormat>(c => c.NameFormat == objFormat.NameFormat);
                if (existingformat == null)
                {
                    db.RunInTransaction(() =>
                    {
                        db.Insert(objFormat);
                    });
                }
            }
        }

        // Добавление нового устройства
        public void Insert_Device(Device objDevice)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                db.RunInTransaction(() =>
                {
                    db.Insert(objDevice);
                });
            }
        }

        public void Update_Device(Device objDevice)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var existingconact = db.Query<Device>("select * from Device where Id =" + objDevice.Id).FirstOrDefault();
                if (existingconact != null)
                {

                    db.RunInTransaction(() =>
                    {
                        db.Update(objDevice);
                    });
                }
            }
        }

        // Добавление тегов устройства
        public void Insert_Tag(Tag objTag)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                db.RunInTransaction(() =>
                {
                    db.Insert(objTag);
                });
            }

        }

        // Добавление типов форматов
        public void Insert_Device_Format(DeviceFormat objDeviceFormat)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                db.RunInTransaction(() =>
                {
                    db.Insert(objDeviceFormat);
                });
            }
        }

        // Добавление треков
        public void Insert_Song(Song objSong)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                objSong.DateChange = objSong.DateChange.AddHours(5);
                db.RunInTransaction(() =>
                {
                    db.Insert(objSong);
                });
            }
        }

        public void Insert_Sync_Device(SyncDevice objSync)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var song_format = db.Find<Song>(c => c.SongID == objSync.SongID);
                var exist_format = db.Find<DeviceFormat>(c => c.DeviceID == objSync.DeviceID && c.MusicFormatID == song_format.FormatId);
                if (exist_format == null)
                {
                    objSync.FormatToFormat = true;
                }

                // Изначально всегда синхронизировать
                objSync.Synchronization = true; 

                db.RunInTransaction(() =>
                {
                    db.Insert(objSync);
                });
            }
        }

        public void Update_Sync_Device(SyncDevice objSync)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var exist = db.Query<SyncDevice>("select * from Sync_Device where SongID =" + objSync.SongID +
                    " and DeviceId = " + objSync.DeviceID).FirstOrDefault();
                if (exist != null)
                {
                    db.RunInTransaction(() =>
                    {
                        db.Update(objSync);
                    });
                }
            }
        }

        public void Update_Song(Song objSong)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var exist = db.Query<Song>("select * from Song where SongID =" + objSong.SongID).FirstOrDefault();
                if (exist != null)
                {
                    db.RunInTransaction(() =>
                    {
                        db.Update(objSong);
                    });
                }
            }
        }

        public void Update_Tag(Tag objTag)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var existingconact = db.Query<Tag>("select * from Tag where Id =" + objTag.Id).FirstOrDefault();
                if (existingconact != null)
                {
                    db.RunInTransaction(() =>
                    {
                        db.Update(objTag);
                    });
                }
            }
        }

        // Используется для вывода всего списка
        public ObservableCollection<Device> ReadAllDevices() 
        {
            try
            {
                using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                {
                    List<Device> myCollection = db.Table<Device>().ToList<Device>();
                    ObservableCollection<Device> DeviceList = new ObservableCollection<Device>(myCollection);
                    return DeviceList;
                }
            }
            catch
            {
                return null;
            }
        }

        // Используется для вывода всего списка устройств
        public ObservableCollection<SyncDevice> ReadAllSync() 
        {
            try
            {
                using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                {
                    List<SyncDevice> myCollection = db.Table<SyncDevice>().ToList<SyncDevice>();
                    ObservableCollection<SyncDevice> DeviceList = new ObservableCollection<SyncDevice>(myCollection);
                    return DeviceList;
                }
            }
            catch
            {
                return null;
            }
        }

        // Используется для вывода всего списка объектов для синхронизации
        public ObservableCollection<Combinated_Sync> ReadAllCombinated(Device CurrentDevice) 
        {
            try
            {
                using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                {
                    List<Combinated_Sync> myCollection = new List<Combinated_Sync>();
                    var sync = db.Table<SyncDevice>().ToList();
                    foreach (var temp in sync)
                    {
                        var test = db.Find<Song>(c => c.SongID == temp.SongID);
                        bool visible = false;
                        if (temp.DateSync.ToString() != "01.01.0001 0:00:00")
                        {
                            visible = true;
                        }
                        if (test.Deleted != true)
                        {
                            myCollection.Add(new Combinated_Sync(temp.DeviceID, temp.SongID, test.NameSong, temp.Synchronization,
                                temp.FormatToFormat, temp.DateSync, visible));
                        }
                    }
                    ObservableCollection<Combinated_Sync> CombinatedList = new ObservableCollection<Combinated_Sync>(myCollection);
                    CombinatedList.OrderBy(i => i.SongID).Where(c => c.DeviceID == CurrentDevice.Id);
                    return CombinatedList;
                }
            }
            catch
            {
                return null;
            }
        }

        // Используется для вывода всего списка треков
        public ObservableCollection<Song> ReadAllSongs() 
        {
            try
            {
                using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                {
                    List<Song> myCollection = db.Table<Song>().Where(c => c.Deleted != true).ToList<Song>();
                    ObservableCollection<Song> SongList = new ObservableCollection<Song>(myCollection);
                    return SongList;
                }
            }
            catch
            {
                return null;
            }
        }
        public void DeleteDevice(int Id)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var existingdevice = db.Query<Device>("select * from Device where Id =" + Id).FirstOrDefault();
                if (existingdevice != null)
                {
                    db.RunInTransaction(() =>
                    {
                        db.Delete(existingdevice);
                    });
                }
            }
        }

        public void DeleteSong(int Id)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var existingsong = db.Query<Song>("select * from Song where SongId =" + Id).FirstOrDefault();
                if (existingsong != null)
                {
                    db.RunInTransaction(() =>
                    {
                        db.Delete(existingsong);
                    });
                }
            }
        }

        public void DeleteTag(int Id)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var existingtag = db.Query<Tag>("select * from Tag where Id =" + Id).FirstOrDefault();
                if (existingtag != null)
                {
                    db.RunInTransaction(() =>
                    {
                        db.Delete(existingtag);
                    });
                }
            }
        }

        public void DeleteFormat(int deviceId, int formatId)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var existingdevice = db.Find<DeviceFormat>(c => c.DeviceID == deviceId && c.MusicFormatID == formatId);
                if (existingdevice != null)
                {
                    db.RunInTransaction(() =>
                    {
                        db.Delete(existingdevice);
                    });
                }
            }
        }

        public void DeleteDevice_format(int Id)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                DeviceFormat existingdevice;
                existingdevice = db.Query<DeviceFormat>("select * from Device_format where DeviceID =" + Id).FirstOrDefault();
                do
                {
                    db.RunInTransaction(() =>
                    {
                        db.Delete(existingdevice);
                    });
                    existingdevice = db.Query<DeviceFormat>("select * from Device_format where DeviceID =" + Id).FirstOrDefault();
                }
                while (existingdevice != null);
            }
        }

        public void DeleteDevice_sync(int Id)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                SyncDevice existingdevice;
                existingdevice = db.Query<SyncDevice>("select * from Sync_Device where DeviceID =" + Id).FirstOrDefault();
                do
                {
                    db.RunInTransaction(() =>
                    {
                        db.Delete(existingdevice);
                    });
                    existingdevice = db.Query<SyncDevice>("select * from Sync_Device where DeviceID =" + Id).FirstOrDefault();
                }
                while (existingdevice != null);
            }
        }
        public void Delete_Song_in_Device_sync(int Id)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                SyncDevice existingsong;
                existingsong = db.Query<SyncDevice>("select * from Sync_Device where SongID =" + Id).FirstOrDefault();
                do
                {
                    db.RunInTransaction(() =>
                    {
                        db.Delete(existingsong);
                    });
                    existingsong = db.Query<SyncDevice>("select * from Sync_Device where SongID =" + Id).FirstOrDefault();
                }
                while (existingsong != null);
            }
        }
    }
}
