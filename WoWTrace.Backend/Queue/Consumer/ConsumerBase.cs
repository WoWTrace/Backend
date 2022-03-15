using System.Text.Json;

using LinqToDB;

using WoWTrace.Backend.DataModels;

namespace WoWTrace.Backend.Queue.Consumer
{
    public abstract class ConsumerBase
    {
        public bool AlreadyProcessedCheck(ulong buildId)
        {
            Build? build;
            using (var db = new WowtraceDB(Settings.Instance.DbConnectionOptions()))
                build = db.Builds.First(b => b.Id == buildId);

            if (build == null)
                return false;

            var processedBy = JsonSerializer.Deserialize<HashSet<string>>(build.ProcessedBy);
            return processedBy != null && processedBy.Contains(GetType().FullName ?? throw new("Parent type is not initialized"));
        }

        public void MarkAsProcessed(ulong buildId)
        {
            Build? build;
            using (var db = new WowtraceDB(Settings.Instance.DbConnectionOptions()))
                build = db.Builds.First(b => b.Id == buildId);

            if (build == null)
                return;

            var processedBy = JsonSerializer.Deserialize<HashSet<string>>(build.ProcessedBy);
            processedBy?.Add(GetType().FullName ?? throw new("Parent type is not initialized"));

            using (var db = new WowtraceDB(Settings.Instance.DbConnectionOptions()))
            {
                db.Builds
                    .Where(b => b.Id == build.Id)
                    .Set(b => b.ProcessedBy, JsonSerializer.Serialize(processedBy))
                    .Update();
            }
        }
    }
}