using CASCLib;
using DotNetWorkQueue;
using DotNetWorkQueue.Transport.SQLite.Basic;
using LinqToDB;
using LinqToDB.Data;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using WoWTrace.Backend.Casc;
using WoWTrace.Backend.DataModels;
using WoWTrace.Backend.Extension;
using WoWTrace.Backend.Queue.Message.V1;
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
                using (var queue = queueContainer.CreateConsumer(QueueManager.Instance.FastlineQueue))
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
                logger.Error($"Cant find Build for id {message.Body.BuildId}");
                return;
            }

            Process(build, message.Body.Force);
        }

        public void Process(Build build, bool force = false)
        {
            if (!force && AlreadyProcessedCheck(build))
            {
                logger.Trace($"Build {build.Id} already processed");
                return;
            }

            List<Listfile> listFileQueryCache = new List<Listfile>();
            List<ListfileBuild> listFileBuildQueryCache = new List<ListfileBuild>();
            List<ListfileVersion> listFileVersionQueryCache = new List<ListfileVersion>();

            using (CASCHandlerWoWTrace cascHandler = CASCHandlerWoWTrace.OpenOnlineStorageWithBuild(build, false, LocaleFlags.enUS))
            {
                foreach (var rootEntry in cascHandler.Root.GetAllEntries())
                {
                    // Skip chinese models
                    if (rootEntry.Value.ContentFlags.HasFlag(ContentFlags.Alternate) || !rootEntry.Value.LocaleFlags.HasFlag(LocaleFlags.enUS))
                        continue;

                    string lookup = null;
                    if (!rootEntry.Value.ContentFlags.HasFlag(ContentFlags.NoNameHash))
                        lookup = string.Format("{0:X}", rootEntry.Key).ToLower();

                    int fileDataId = cascHandler.Root.GetFileDataIdByHash(rootEntry.Key);
                    long fileSize = cascHandler.GetFileSize(rootEntry.Value.MD5);

                    listFileQueryCache.Add(new Listfile() { Id = (ulong)fileDataId, Lookup = lookup, Verified = false, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now});
                    listFileBuildQueryCache.Add(new ListfileBuild() { Id = (ulong)fileDataId, BuildId = build.Id });
                    listFileVersionQueryCache.Add(new ListfileVersion()
                    {
                        Id = (ulong)fileDataId,
                        ContentHash = rootEntry.Value.MD5.ToHexString().ToLower(),
                        Encrypted = rootEntry.Value.ContentFlags.HasFlag(ContentFlags.Encrypted),
                        FileSize = (uint)fileSize,
                        FirstBuildId = build.Id,
                        ClientBuild = build.ClientBuild,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });

                    if (listFileQueryCache.Count >= Settings.Instance.DBBulkSize)
                    {
                        SaveBulk(ref listFileQueryCache, ref listFileBuildQueryCache, ref listFileVersionQueryCache);
                    }
                }
            }

            MarkAsProcessed(build);
        }

        private void SaveBulk(ref List<Listfile> listFileQueryCache, ref List<ListfileBuild> listFileBuildQueryCache, ref List<ListfileVersion> listFileVersionQueryCache)
        {

            using (var db = new WowtraceDB(Settings.Instance.DBConnectionOptions()))
            {
                db.MultiInsertOnDuplicateUpdateRaw(db.Listfiles, listFileQueryCache, new List<string>() { "`lookup` = IF(values(`lookup`) IS NOT NULL, values(`lookup`), `lookup`)" }, new BulkCopyOptions() { KeepIdentity = true });
                db.MultiInsertIgnore(db.ListfileBuilds, listFileBuildQueryCache, new BulkCopyOptions() { KeepIdentity = true});
                db.MultiInsertIgnore(db.ListfileVersions, listFileVersionQueryCache, new BulkCopyOptions() { KeepIdentity = true});
            }

            listFileQueryCache.Clear();
            listFileBuildQueryCache.Clear();
            listFileVersionQueryCache.Clear();
        }
    }
}
