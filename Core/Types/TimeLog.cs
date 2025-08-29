using System;
using System.IO;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json;

using CKAN.Extensions;

namespace CKAN
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TimeLog
    {
        [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan Time;

        public static string GetPath(string dir)
            => Path.Combine(dir, filename);

        public static TimeLog? Load(string path)
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

        [ExcludeFromCodeCoverage]
        public void Save(string path)
        {
            JsonConvert.SerializeObject(this, Formatting.Indented)
                       .WriteThroughTo(path);
        }

        public override string ToString()
            => Time.TotalHours.ToString("N1");

        private readonly Stopwatch playTime = new Stopwatch();

        [ExcludeFromCodeCoverage]
        public void Start()
        {
            playTime.Restart();
        }

        [ExcludeFromCodeCoverage]
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
