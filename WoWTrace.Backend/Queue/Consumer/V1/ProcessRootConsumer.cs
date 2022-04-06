using DotNetWorkQueue;
using DotNetWorkQueue.Transport.SQLite.Basic;
using NLog;
using TACT.Net.Root;

using WoWTrace.Backend.DataModels;
using WoWTrace.Backend.Extensions;
using WoWTrace.Backend.Queue.Message.V1;
using WoWTrace.Backend.Tact;

namespace WoWTrace.Backend.Queue.Consumer.V1
{
    public class ProcessRootConsumer : ConsumerBase, IConsumer
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void Listen()
        {
            using (var queueContainer = new QueueContainer<SqLiteMessageQueueInit>())
            using (var queue = queueContainer.CreateConsumer(QueueManager.Instance.RootV1Queue))
            {
                queue.Start<ProcessRootMessage>(HandleMessages);
                Thread.Sleep(Timeout.Infinite);
            }
        }

        private void HandleMessages(IReceivedMessage<ProcessRootMessage> message, IWorkerNotification notifications)
        {
            Build? build;
            using (var db = new WowtraceDB(Settings.Instance.DbConnectionOptions()))
                build = db.Builds.First(b => b.Id == message.Body.BuildId);

            if (build == null)
            {
                _logger.Error($"Cant find build for id {message.Body.BuildId}");
                return;
            }

            Process(build, message.Body.Force);
        }

        private void Process(Build build, bool force = false)
        {
            if (!force && AlreadyProcessedCheck(build.Id))
            {
                _logger.Trace($"Build {build.Id} already processed");
                return;
            }

            _logger.Trace($"Process Build {build.Id}");

            var rootEntryCount = 0L;
            var listFileQueryCache = new List<Listfile>();
            var listFileBuildQueryCache = new List<ListfileBuild>();
            var listFileVersionQueryCache = new List<ListfileVersion>();

            using (var tact = new TactHandler(build))
            {
                if (tact.Repo == null)
                {
                    _logger.Error($"Build {build.Id} has invalid tact repo");
                    return;
                }

                foreach (var rootBlock in tact.Repo.RootFile.GetAllBlocks())
                {
                    // Skip chinese and non enUS blocks
                    if (rootBlock.ContentFlags.HasFlag(ContentFlags.LowViolence) || !rootBlock.LocaleFlags.HasFlag(LocaleFlags.enUS))
                        continue;

                    foreach (var (_, rootRecord) in rootBlock.Records)
                    {
                        string? lookup = null;
                        if (!rootBlock.ContentFlags.HasFlag(ContentFlags.NoNameHash))
                            lookup = $"{rootRecord.NameHash:X}".ToLower();

                        ulong? fileSize = null;
                        if (tact.Repo.EncodingFile.TryGetCKeyEntry(rootRecord.CKey, out var encodingEntry))
                            fileSize = encodingEntry.DecompressedSize;

                        listFileQueryCache.Add(new() { Id = rootRecord.FileId, Lookup = lookup, Verified = false, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });
                        listFileBuildQueryCache.Add(new() { Id = rootRecord.FileId, BuildId = build.Id });
                        listFileVersionQueryCache.Add(new()
                        {
                            Id = rootRecord.FileId,
                            ContentHash = rootRecord.CKey.ToString(),
                            Encrypted = rootBlock.ContentFlags.HasFlag(ContentFlags.Encrypted),
                            FileSize = (uint?)fileSize,
                            FirstBuildId = build.Id,
                            ClientBuild = build.ClientBuild,
                            ProcessedBy = "[]",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });

                        rootEntryCount++;
                        if (listFileQueryCache.Count >= Settings.Instance.DbBulkSize || listFileBuildQueryCache.Count >= Settings.Instance.DbBulkSize || listFileVersionQueryCache.Count >= Settings.Instance.DbBulkSize)
                            SaveBulk(ref listFileQueryCache, ref listFileBuildQueryCache, ref listFileVersionQueryCache);
                    }
                }

                SaveBulk(ref listFileQueryCache, ref listFileBuildQueryCache, ref listFileVersionQueryCache);
            }

            _logger.Info($"Processed {rootEntryCount} root entries in build {build.Id}");
            MarkAsProcessed(build.Id);
        }

        private static void SaveBulk(ref List<Listfile> listFileQueryCache, ref List<ListfileBuild> listFileBuildQueryCache, ref List<ListfileVersion> listFileVersionQueryCache)
        {
            using (var db = new WowtraceDB(Settings.Instance.DbConnectionOptions()))
            {
                db.MultiInsertOnDuplicateUpdateRaw(db.Listfiles, listFileQueryCache, new List<string>
                {
                    "`lookup` = IF(values(`lookup`) IS NOT NULL, values(`lookup`), `lookup`)"
                }, new() { KeepIdentity = true });
                db.MultiInsertIgnore(db.ListfileBuilds, listFileBuildQueryCache, new() { KeepIdentity = true });
                db.MultiInsertIgnore(db.ListfileVersions, listFileVersionQueryCache, new() { KeepIdentity = true });
            }

            listFileQueryCache.Clear();
            listFileBuildQueryCache.Clear();
            listFileVersionQueryCache.Clear();
        }
    }
}