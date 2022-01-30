using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using LinqToDB.Configuration;
using System.Threading;

namespace WoWTrace.Backend
{
    
    public class Settings
    {
        [JsonIgnore]
        private static string SavePath => Path.Combine(Environment.CurrentDirectory, "settings.json");

        [JsonIgnore]
        private static readonly Lazy<Settings> lazy = new Lazy<Settings>(() => {
            if (!File.Exists(SavePath))
            {
                var settings = new Settings();
                settings.Save();
                return settings;
            }

            return JsonSerializer.Deserialize<Settings>(File.ReadAllText(SavePath));
        }, LazyThreadSafetyMode.ExecutionAndPublication);
        
        [JsonIgnore]
        public static Settings Instance { get { return lazy.Value; } }

        [JsonPropertyName("cacheEnabled")]
        public bool CacheEnabled { get; set; } = true;

        [JsonPropertyName("cacheDir")]
        public string CachePath { get; set; } = "cache";

        [JsonPropertyName("dbConnectionString")]
        public string DBConnectionString { get; set; } = "Server=127.0.0.1;Port=3306;Database=wowtrace;Uid=root;Pwd=;";

        [JsonPropertyName("dbBulkSize")]
        public int DBBulkSize { get; set; } = 10000;

        [JsonPropertyName("queueConnectionString")]
        public string QueueConnectionString { get; set; } = @"FullUri=file:queue.db3?mode=memory&cache=shared;Version=3;";

        private Settings()
        {
            //
        }

        [JsonConstructor]
        public Settings(bool CacheEnabled, string CachePath, string DBConnectionString, int DBBulkSize, string QueueConnectionString)
        {
            this.CacheEnabled = CacheEnabled;
            this.CachePath = CachePath;
            this.DBConnectionString = DBConnectionString;
            this.DBBulkSize = DBBulkSize;
            this.QueueConnectionString = QueueConnectionString;
        }

        ~Settings()
        {
            Save();
        }

        public LinqToDbConnectionOptions DBConnectionOptions()
        {
            LinqToDbConnectionOptionsBuilder builder = new LinqToDbConnectionOptionsBuilder();
            builder.UseMySql(DBConnectionString);

            return builder.Build();
        }

        public void Save()
        {
            string jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(SavePath, jsonString);
        }
    }
}
