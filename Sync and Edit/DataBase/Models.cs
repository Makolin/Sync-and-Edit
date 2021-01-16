using SQLite;
using System;

namespace Sync_and_Edit.DataBase
{
    interface ISong
    {
        int SongID { get; }
    }
    public class Device
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id { get; set; }
        [NotNull]
        public string Name { get; set; }
        public string Model { get; set; }
        [NotNull]
        public double Memory { get; set; }

        public Device() { }
        public Device(string name, string model, double memory)
        {
            Name = name;
            Model = model;
            Memory = memory;
        }
        public Device(int id, string name, string model, double memory)
        {
            Id = id;
            Name = name;
            Model = model;
            Memory = memory;
        }
    }
    public class Tag
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id { get; set; }
        public int Track_Number { get; set; }
        public string Artist { get; set; }
        public string Name_song { get; set; }
        public string Albom { get; set; }
        public string Artist_Albom { get; set; }
        public int Year { get; set; }
        public string Genge { get; set; }
        //public string Comment { get; set; }
        public string Composer { get; set; }

        public Tag() { }
        public Tag(int track_number, string artist, string name_song, string albom, string artist_albom,
            int year, string genge, string composer)
        {
            Track_Number = track_number;
            Artist = artist;
            Name_song = name_song;
            Albom = albom;
            Artist_Albom = artist_albom;
            Year = year;
            Genge = genge;
            //Comment = comment;
            Composer = composer;
        }
    }
    public class Song : ISong
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int SongID { get; set; }
        [NotNull]
        public int TagId { get; set; }
        [NotNull]
        public string Name_song { get; set; }
        [NotNull]
        public int Format_Id { get; set; }
        [NotNull]
        public DateTime Date_change { get; set; }
        [NotNull]
        public int Bitrait { get; set; }
        [NotNull]
        public int Size { get; set; }
        public string Path { get; set; }
        public bool Deleted { get; set; }

        public Song() { }
        public Song(int tag_id, string name_song, int format_id,
            DateTime date_change, int bitrait, int size, string path, bool deleted)
        {
            TagId = tag_id;
            Name_song = name_song;
            Format_Id = format_id;
            Date_change = date_change;
            Bitrait = bitrait;
            Size = size;
            Path = path;
            deleted = Deleted;
        }
    }


    public class Music_format
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id { get; set; }
        [NotNull]
        public string Name_format { get; set; }

        public Music_format() { }
        public Music_format(string name_format)
        {
            Name_format = name_format;

        }
    }

    public class Device_format
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id { get; set; }
        public int DeviceID { get; set; }
        public int Music_formatID { get; set; }

        public Device_format() { }
        public Device_format(int device_id, int music_format_id)
        {
            DeviceID = device_id;
            Music_formatID = music_format_id;

        }
    }

    public class Sync_Device : ISong
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id { get; set; }
        public int DeviceID { get; set; }
        public int SongID { get; set; }
        public bool Synchronization { get; set; }
        public bool FormatToFormat { get; set; }
        public DateTime Date_sync { get; set; }


        public Sync_Device() { }
        public Sync_Device(int deviceId, int songId)
        {
            DeviceID = deviceId;
            SongID = songId;
        }
        public Sync_Device(int deviceId, int songId, bool synchronization, bool formatToformat, DateTime date_sync)
        {
            DeviceID = deviceId;
            SongID = songId;
            Synchronization = synchronization;
            FormatToFormat = formatToformat;
            Date_sync = date_sync;
        }
    }
    public class Combinated_Sync
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id { get; set; }
        public int DeviceID { get; set; }
        public int SongID { get; set; }
        public string Name_song { get; set; }
        public bool Synchronization { get; set; }
        public bool FormatToFormat { get; set; }
        public DateTime Date_change { get; set; }
        bool Visible { get; set; }

        public Combinated_Sync(int deviceId, int songId, string name_song, bool synchronization, bool formatToformat,
            DateTime date_change, bool visible)
        {
            DeviceID = deviceId;
            SongID = songId;
            Name_song = name_song;
            Synchronization = synchronization;
            FormatToFormat = formatToformat;
            Date_change = date_change;
            Visible = visible;
        }
    }
}
