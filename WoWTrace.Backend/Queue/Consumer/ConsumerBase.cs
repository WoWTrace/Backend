using LinqToDB;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WoWTrace.Backend.DataModels;

namespace WoWTrace.Backend.Queue.Consumer
{
    public abstract class ConsumerBase
    {
        public bool AlreadyProcessedCheck(Build build)
        {
            HashSet<string> processedBy = JsonSerializer.Deserialize<HashSet<string>>(build.ProcessedBy);
            return processedBy.Contains(GetType().FullName);
        }

        public void MarkAsProcessed(Build build)
        {
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
