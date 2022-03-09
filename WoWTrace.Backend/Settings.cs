using System.Text.Json;
using System.Text.Json.Serialization;

using LinqToDB.Configuration;

namespace WoWTrace.Backend
{
    public class Settings
    {
        [JsonIgnore]
        private static string SavePath => Path.Combine(Environment.CurrentDirectory, "settings.json");

        [JsonIgnore]
        private static readonly Lazy<Settings> _lazy = new(() =>
        {
            if (File.Exists(SavePath))
                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(SavePath)) ?? throw new($"Failed to load {SavePath}");

            var settings = new Settings();
            settings.Save();
            return settings;

        }, LazyThreadSafetyMode.ExecutionAndPublication);

        [JsonIgnore]
        public static Settings Instance => _lazy.Value;

        [JsonPropertyName("cacheEnabled")]
        public bool CacheEnabled { get; set; } = true;

        [JsonPropertyName("cacheDir")]
        public string CachePath { get; set; } = "cache";

        [JsonPropertyName("dbConnectionString")]
        public string DbConnectionString { get; set; } = "Server=127.0.0.1;Port=3306;Database=wowtrace;Uid=root;Pwd=;";

        [JsonPropertyName("dbBulkSize")]
        public int DbBulkSize { get; set; } = 10000;

        [JsonPropertyName("queueConnectionString")]
        public string QueueConnectionString { get; set; } = @"FullUri=file:queue.db3?mode=memory&cache=shared;Version=3;";

        private Settings()
        {
            //
        }

        [JsonConstructor]
        public Settings(bool cacheEnabled, string cachePath, string dbConnectionString, int dbBulkSize, string queueConnectionString)
        {
            CacheEnabled = cacheEnabled;
            CachePath = cachePath;
            DbConnectionString = dbConnectionString;
            DbBulkSize = dbBulkSize;
            QueueConnectionString = queueConnectionString;
        }

        ~Settings()
        {
            Save();
        }

        public LinqToDbConnectionOptions DbConnectionOptions()
        {
            var builder = new LinqToDbConnectionOptionsBuilder();
            builder.UseMySql(DbConnectionString);

            return builder.Build();
        }

        public void Save()
        {
            var jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SavePath, jsonString);
        }
    }
}