using DotNetWorkQueue;
using DotNetWorkQueue.Transport.SQLite.Basic;
using LinqToDB.Data;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TACT.Net.Root;
using WoWTrace.Backend.DataModels;
using WoWTrace.Backend.Extensions;
using WoWTrace.Backend.Queue.Message.V1;
using WoWTrace.Backend.Tact;
using Logger = NLog.Logger;

namespace WoWTrace.Backend.Queue.Consumer.V1
{
    public class ProcessRootConsumer : ConsumerBase, IConsumer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void Listen()
        {
            using (var queueContainer = new QueueContainer<SqLiteMessageQueueInit>())
            {
                using (var queue = queueContainer.CreateConsumer(QueueManager.Instance.RootV1Queue))
                {
                    queue.Start<ProcessRootMessage>(HandleMessages);
                    Thread.Sleep(Timeout.Infinite);
                }
            }
        }

        private void HandleMessages(IReceivedMessage<ProcessRootMessage> message, IWorkerNotification notifications)
        {
            Build build = null;
            using (var db = new WowtraceDB(Settings.Instance.DBConnectionOptions()))
            {
                build = db.Builds.First(b => b.Id == message.Body.BuildId);
            }

            if (build == null)
            {
                logger.Error($"Cant find build for id {message.Body.BuildId}");
                return;
            }

            Process(build, message.Body.Force);
        }

        public void Process(Build build, bool force = false)
        {
            if (!force && AlreadyProcessedCheck(build.Id))
            {
                logger.Trace($"Build {build.Id} already processed");
                return;
            }

            logger.Trace($"Process Build {build.Id}");

            long rootEntryCount = 0;
            List<Listfile> listFileQueryCache = new List<Listfile>();
            List<ListfileBuild> listFileBuildQueryCache = new List<ListfileBuild>();
            List<ListfileVersion> listFileVersionQueryCache = new List<ListfileVersion>();

            using (TactHandler tact = new TactHandler(build))
            {
                foreach (var rootBlock in tact.Repo.RootFile.GetAllBlocks())
                {
                    // Skip chinese and non enUS blocks
                    if (rootBlock.ContentFlags.HasFlag(ContentFlags.LowViolence) || !rootBlock.LocaleFlags.HasFlag(LocaleFlags.enUS))
                        continue;

                    foreach (var rootRecord in rootBlock.Records)
                    {
                        string lookup = null;
                        if (!rootBlock.ContentFlags.HasFlag(ContentFlags.NoNameHash))
                            lookup = string.Format("{0:X}", rootRecord.Value.NameHash).ToLower();

                        ulong? fileSize = null;

                        if (tact.Repo.EncodingFile.TryGetCKeyEntry(rootRecord.Value.CKey, out var encodingEntry))
                            fileSize = encodingEntry.DecompressedSize;

                        listFileQueryCache.Add(new Listfile() { Id = rootRecord.Value.FileId, Lookup = lookup, Verified = false, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });
                        listFileBuildQueryCache.Add(new ListfileBuild() { Id = rootRecord.Value.FileId, BuildId = build.Id });
                        listFileVersionQueryCache.Add(new ListfileVersion()
                        {
                            Id = rootRecord.Value.FileId,
                            ContentHash = rootRecord.Value.CKey.ToString(),
                            Encrypted = rootBlock.ContentFlags.HasFlag(ContentFlags.Encrypted),
                            FileSize = (uint?)fileSize.Value,
                            FirstBuildId = build.Id,
                            ClientBuild = build.ClientBuild,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });

                        rootEntryCount++;
                        if (listFileQueryCache.Count >= Settings.Instance.DBBulkSize)
                            SaveBulk(ref listFileQueryCache, ref listFileBuildQueryCache, ref listFileVersionQueryCache);
                    }
                }
            }

            logger.Trace($"Processed {rootEntryCount} root entries in build {build.Id}");
            MarkAsProcessed(build.Id);
        }

        private void SaveBulk(ref List<Listfile> listFileQueryCache, ref List<ListfileBuild> listFileBuildQueryCache, ref List<ListfileVersion> listFileVersionQueryCache)
        {

            using (var db = new WowtraceDB(Settings.Instance.DBConnectionOptions()))
            {
                db.MultiInsertOnDuplicateUpdateRaw(db.Listfiles, listFileQueryCache, new List<string>() { "`lookup` = IF(values(`lookup`) IS NOT NULL, values(`lookup`), `lookup`)" }, new BulkCopyOptions() { KeepIdentity = true });
                db.MultiInsertIgnore(db.ListfileBuilds, listFileBuildQueryCache, new BulkCopyOptions() { KeepIdentity = true });
                db.MultiInsertIgnore(db.ListfileVersions, listFileVersionQueryCache, new BulkCopyOptions() { KeepIdentity = true });
            }

            listFileQueryCache.Clear();
            listFileBuildQueryCache.Clear();
            listFileVersionQueryCache.Clear();
        }
    }
}
