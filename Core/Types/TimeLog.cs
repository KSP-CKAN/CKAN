using System;
using System.IO;
using System.Diagnostics;

using Newtonsoft.Json;

namespace CKAN
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TimeLog
    {
        [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan Time;

        public static string GetPath(string dir)
            => Path.Combine(dir, filename);

        public static TimeLog Load(string path)
        {
            try
            {
                return JsonConvert.DeserializeObject<TimeLog>(File.ReadAllText(path));
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public override string ToString()
            => Time.TotalHours.ToString("N1");

        private readonly Stopwatch playTime = new Stopwatch();

        public void Start()
        {
            playTime.Restart();
        }

        public void Stop(string dir)
        {
            if (playTime.IsRunning)
            {
                playTime.Stop();
                Time += playTime.Elapsed;
                Save(GetPath(dir));
            }
        }

        private const string filename = "playtime.json";
    }
}
