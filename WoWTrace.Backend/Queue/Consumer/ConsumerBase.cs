using LinqToDB;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WoWTrace.Backend.DataModels;

namespace WoWTrace.Backend.Queue.Consumer
{
    public abstract class ConsumerBase
    {
        public bool AlreadyProcessedCheck(ulong buildId)
        {
            Build build = null;
            using (var db = new WowtraceDB(Settings.Instance.DBConnectionOptions()))
            {
                build = db.Builds.First(b => b.Id == buildId);
            }

            if (build == null)
                return false;

            HashSet<string> processedBy = JsonSerializer.Deserialize<HashSet<string>>(build.ProcessedBy);
            return processedBy.Contains(GetType().FullName);
        }

        public void MarkAsProcessed(ulong buildId)
        {
            Build build = null;
            using (var db = new WowtraceDB(Settings.Instance.DBConnectionOptions()))
            {
                build = db.Builds.First(b => b.Id == buildId);
            }

            if (build == null)
                return;

            HashSet<string> processedBy = JsonSerializer.Deserialize<HashSet<string>>(build.ProcessedBy);
            processedBy.Add(GetType().FullName);

            using (var db = new WowtraceDB(Settings.Instance.DBConnectionOptions()))
            {
                db.Builds
                    .Where(b => b.Id == build.Id)
                    .Set(b => b.ProcessedBy, JsonSerializer.Serialize(processedBy))
                    .Update();
            }
        }
    }
}
