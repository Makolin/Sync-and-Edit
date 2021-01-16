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
                db.CreateTable<Sync_Device>();
                db.CreateTable<Music_format>();
                db.CreateTable<Device_format>();
            }
        }

        public static void ClearDatabase()
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                db.DeleteAll<Device>();
                db.DeleteAll<Song>();
                db.DeleteAll<Tag>();
                db.DeleteAll<Sync_Device>();
                db.DeleteAll<Device_format>();
            }
        }


        public void Insert(Music_format objFormat) //Заполнение таблицы форматов
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var existingformat = db.Find<Music_format>(c => c.Name_format == objFormat.Name_format);
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
        public void Insert_Device_Format(Device_format objDeviceFormat)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                db.RunInTransaction(() =>
                {
                    db.Insert(objDeviceFormat);
                });
            }
        }
        //Добавление песен
        public void Insert_Song(Song objSong)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                objSong.Date_change = objSong.Date_change.AddHours(5);
                db.RunInTransaction(() =>
                {
                    db.Insert(objSong);
                });
            }
        }


        public void Insert_Sync_Device(Sync_Device objSync)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var song_format = db.Find<Song>(c => c.SongID == objSync.SongID);
                var exist_format = db.Find<Device_format>(c => c.DeviceID == objSync.DeviceID && c.Music_formatID == song_format.Format_Id);
                if (exist_format == null)
                {
                    objSync.FormatToFormat = true;
                }
                objSync.Synchronization = true; //Изначально всегда синхронизировать

                db.RunInTransaction(() =>
                {
                    db.Insert(objSync);
                });
            }
        }

        public void Update_Sync_Device(Sync_Device objSync)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                var exist = db.Query<Sync_Device>("select * from Sync_Device where SongID =" + objSync.SongID +
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

        public ObservableCollection<Device> ReadAllDevices() //Используется для вывода всего списка
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

        public ObservableCollection<Sync_Device> ReadAllSync() //Используется для вывода всего списка
        {
            try
            {
                using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                {
                    List<Sync_Device> myCollection = db.Table<Sync_Device>().ToList<Sync_Device>();
                    ObservableCollection<Sync_Device> DeviceList = new ObservableCollection<Sync_Device>(myCollection);
                    return DeviceList;
                }
            }
            catch
            {
                return null;
            }
        }

        public ObservableCollection<Combinated_Sync> ReadAllCombinated(Device CurrentDevice) //Используется для вывода всего списка
        {
            try
            {
                using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
                {
                    List<Combinated_Sync> myCollection = new List<Combinated_Sync>();
                    var sync = db.Table<Sync_Device>().ToList();
                    foreach (var temp in sync)
                    {
                        var test = db.Find<Song>(c => c.SongID == temp.SongID);
                        bool visible = false;
                        if (temp.Date_sync.ToString() != "01.01.0001 0:00:00")
                        {
                            visible = true;
                        }
                        if (test.Deleted != true)
                        {
                            myCollection.Add(new Combinated_Sync(temp.DeviceID, temp.SongID, test.Name_song, temp.Synchronization,
                                temp.FormatToFormat, temp.Date_sync, visible));
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

        public ObservableCollection<Song> ReadAllSongs() //Используется для вывода всего списка
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
                var existingdevice = db.Find<Device_format>(c => c.DeviceID == deviceId && c.Music_formatID == formatId);
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
                Device_format existingdevice;
                existingdevice = db.Query<Device_format>("select * from Device_format where DeviceID =" + Id).FirstOrDefault();
                do
                {
                    db.RunInTransaction(() =>
                    {
                        db.Delete(existingdevice);
                    });
                    existingdevice = db.Query<Device_format>("select * from Device_format where DeviceID =" + Id).FirstOrDefault();
                }
                while (existingdevice != null);

            }
        }

        public void DeleteDevice_sync(int Id)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                Sync_Device existingdevice;
                existingdevice = db.Query<Sync_Device>("select * from Sync_Device where DeviceID =" + Id).FirstOrDefault();
                do
                {
                    db.RunInTransaction(() =>
                    {
                        db.Delete(existingdevice);
                    });
                    existingdevice = db.Query<Sync_Device>("select * from Sync_Device where DeviceID =" + Id).FirstOrDefault();
                }
                while (existingdevice != null);

            }
        }
        public void Delete_Song_in_Device_sync(int Id)
        {
            using (SQLiteConnection db = new SQLiteConnection(App.DB_PATH))
            {
                Sync_Device existingsong;
                existingsong = db.Query<Sync_Device>("select * from Sync_Device where SongID =" + Id).FirstOrDefault();
                do
                {
                    db.RunInTransaction(() =>
                    {
                        db.Delete(existingsong);
                    });
                    existingsong = db.Query<Sync_Device>("select * from Sync_Device where SongID =" + Id).FirstOrDefault();
                }
                while (existingsong != null);
            }
        }
    }
}
