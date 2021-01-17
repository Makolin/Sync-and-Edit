using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Sync_and_Edit.Json
{
    public class Json_options
    {
        public string Json_source_1 { get; set; }
        public string Json_source_2 { get; set; }
        public string Json_source_3 { get; set; }
        public string Json_audioteca { get; set; }
        public string Json_mp3 { get; set; }
        public string Json_alac { get; set; }
        public string Json_flac { get; set; }
        public string Json_aac { get; set; }
        private static string json_path = ApplicationData.Current.LocalFolder.Path;

        public async Task<Json_options> ReadJson()
        {
            var path_source = new Json_options()
            {
                Json_audioteca = string.Empty,
                Json_source_1 = string.Empty,
                Json_source_2 = string.Empty,
                Json_source_3 = string.Empty,
                /*Json_audioteca = @"C:\Users\Admin\Music\Audioteca",
                Json_source_1 = @"C:\Users\Admin\Music\Source_1",
                Json_source_2 = @"C:\Users\Admin\Music\Source_2",
                Json_source_3 = @"C:\Users\Admin\Music\Source_3",*/
                Json_mp3 = "High",
                Json_alac = "Medium",
                Json_flac = "Medium",
                Json_aac = "High"
            };
            bool exist = false;
            await Task.Run(() =>
            {
                exist = File.Exists(json_path + "\\options.json");
            });

            if (exist != false)
            {
                await Task.Run(() =>
                {
                    path_source = JsonConvert.DeserializeObject<Json_options>(File.ReadAllText(json_path + "\\options.json"));
                });
                return path_source;
            }
            else
            {
                return path_source;
            }
        }
        public async void Write_Json(string source_1,
            string source_2, string source_3, string audio, string mp3, string alac,
            string flac, string aac)
        {
            var New_source = new Json_options
            {
                Json_source_1 = source_1,
                Json_source_2 = source_2,
                Json_source_3 = source_3,
                Json_audioteca = audio,
                Json_mp3 = mp3,
                Json_alac = alac,
                Json_aac = aac,
                Json_flac = flac
            };
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await storageFolder.CreateFileAsync("options.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sampleFile, JsonConvert.SerializeObject(New_source));
        }
    }
}
