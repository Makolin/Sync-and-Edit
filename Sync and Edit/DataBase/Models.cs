using SQLite;
using System;

namespace Sync_and_Edit.DataBase
{
    /*interface ISong
    {
        int SongID { get; }
    }*/
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
        public int TrackNumber { get; set; }
        public string Artist { get; set; }
        public string NameSong { get; set; }
        public string Albom { get; set; }
        public string ArtistAlbom { get; set; }
        public int Year { get; set; }
        public string Genge { get; set; }
        //public string Comment { get; set; }
        public string Composer { get; set; }

        public Tag() { }
        public Tag(int trackNumber, string artist, string nameSong, string albom, string artistAlbom,
            int year, string genge, string composer)
        {
            TrackNumber = trackNumber;
            Artist = artist;
            NameSong = nameSong;
            Albom = albom;
            ArtistAlbom = artistAlbom;
            Year = year;
            Genge = genge;
            //Comment = comment;
            Composer = composer;
        }
    }
    public class Song 
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int SongID { get; set; }
        [NotNull]
        public int TagId { get; set; }
        [NotNull]
        public string NameSong { get; set; }
        [NotNull]
        public int FormatId { get; set; }
        [NotNull]
        public DateTime DateChange { get; set; }
        [NotNull]
        public int Bitrait { get; set; }
        [NotNull]
        public int Size { get; set; }
        public string Path { get; set; }
        public bool Deleted { get; set; }

        public Song() { }
        public Song(int tagId, string nameSong, int formatId,
            DateTime dateChange, int bitrait, int size, string path, bool deleted)
        {
            TagId = tagId;
            NameSong = nameSong;
            FormatId = formatId;
            DateChange = dateChange;
            Bitrait = bitrait;
            Size = size;
            Path = path;
            Deleted = deleted;
        }
    }

    public class MusicFormat
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id { get; set; }
        [NotNull]
        public string NameFormat { get; set; }
        public MusicFormat() { }
        public MusicFormat(string name_format)
        {
            NameFormat = name_format;
        }
    }

    public class DeviceFormat
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id { get; set; }
        public int DeviceID { get; set; }
        public int MusicFormatID { get; set; }

        public DeviceFormat() { }
        public DeviceFormat(int deviceId, int musicFormatId)
        {
            DeviceID = deviceId;
            MusicFormatID = musicFormatId;

        }
    }

    public class SyncDevice
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id { get; set; }
        public int DeviceID { get; set; }
        public int SongID { get; set; }
        public bool Synchronization { get; set; }
        public bool FormatToFormat { get; set; }
        public DateTime DateSync { get; set; }

        public SyncDevice() { }
        public SyncDevice(int deviceId, int songId)
        {
            DeviceID = deviceId;
            SongID = songId;
        }
        public SyncDevice(int deviceId, int songId, bool synchronization, bool formatToFormat, DateTime dateSync)
        {
            DeviceID = deviceId;
            SongID = songId;
            Synchronization = synchronization;
            FormatToFormat = formatToFormat;
            DateSync = dateSync;
        }
    }
    public class Combinated_Sync
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id { get; set; }
        public int DeviceID { get; set; }
        public int SongID { get; set; }
        public string NameSong { get; set; }
        public bool Synchronization { get; set; }
        public bool FormatToFormat { get; set; }
        public DateTime DateChange { get; set; }
        bool Visible { get; set; }

        public Combinated_Sync(int deviceId, int songId, string nameSong, bool synchronization, bool formatToFormat,
            DateTime dateChange, bool visible)
        {
            DeviceID = deviceId;
            SongID = songId;
            NameSong = nameSong;
            Synchronization = synchronization;
            FormatToFormat = formatToFormat;
            DateChange = dateChange;
            Visible = visible;
        }
    }
}
