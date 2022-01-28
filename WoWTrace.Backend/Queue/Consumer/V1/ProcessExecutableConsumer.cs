using CASCLib;
using DotNetWorkQueue;
using DotNetWorkQueue.Transport.SQLite.Basic;
using LinqToDB;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using WoWTrace.Backend.Casc;
using WoWTrace.Backend.DataModels;
using WoWTrace.Backend.Queue.Message.V1;
using Logger = NLog.Logger;

namespace WoWTrace.Backend.Queue.Consumer.V1
{
    public class ProcessExecutableConsumer : ConsumerBase, IConsumer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void Listen()
        {
            using (var schedulerContainer = new SchedulerContainer())
            {
                using (var scheduler = schedulerContainer.CreateTaskScheduler())
                {
                    var factory = schedulerContainer.CreateTaskFactory(scheduler);
                    factory.Scheduler.Configuration.MaximumThreads = 4;
                    factory.Scheduler.Start();
                    using (var queueContainer = new QueueContainer<SqLiteMessageQueueInit>())
                    {
                        using (var queue = queueContainer.CreateConsumerQueueScheduler(QueueManager.Instance.ExecutableV1Queue, factory))
                        {
                            queue.Start<ProcessExecutableMessage>(HandleMessages);
                            Thread.Sleep(Timeout.Infinite);
                        }
                    }
                }
            }
        }

        private void HandleMessages(IReceivedMessage<ProcessExecutableMessage> message, IWorkerNotification notifications)
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

            if (build.CompiledAt != null)
            {
                logger.Trace("Build has already compiledAt date");
                return;
            }

            Process(build, message.Body.Force);
        }

        public void Process(Build build, bool force = false)
        {
            logger.Trace($"Build process Build {build.Id}");

            if (!force && AlreadyProcessedCheck(build.Id))
            {
                logger.Trace($"Build {build.Id} already processed");
                return;
            }

            bool builtDateFound = false;
            string pattern = @"Exe\s+Built:\s+(\w{3}\s+\d{1,2}\s+\d{4}\s+\d{2}:\d{2}:\d{2})";
            RegexOptions options = RegexOptions.IgnoreCase;

            using (Stream stream = OpenExecutable(build))
            using (BinaryReader br = new BinaryReader(stream))
            {
                while (stream.Position != stream.Length)
                {
                    Match match = Regex.Match(br.ReadCString(), pattern, options);

                    if (match.Success && match.Groups.Count == 2)
                    {
                        logger.Trace($"Executable for build {build.Id} built on {match.Groups[1].Value}");

                        DateTime executableBuiltDate = DateTime.Parse(match.Groups[1].Value);

                        using (var db = new WowtraceDB(Settings.Instance.DBConnectionOptions()))
                        {
                            db.Builds
                                .Where(b => b.Id == build.Id)
                                .Set(b => b.CompiledAt, executableBuiltDate)
                                .Set(b => b.UpdatedAt, DateTime.Now)
                                .Update();
                        }

                        builtDateFound = true;
                        break;
                    }
                }
            }

            if (!builtDateFound)
            {
                logger.Error($"Cant find executable built date in BuildId: {build.Id}!");
                return;
            }

            MarkAsProcessed(build.Id);
        }

        private Stream OpenExecutable(Build build)
        {
            using (CASCHandlerWoWTrace cascHandler = CASCHandlerWoWTrace.OpenOnlineStorageWithBuild(build))
            {
                List<string> executableNames = new List<string>()
                {
                    // Mainline
                    "Wow.exe", "WowT.exe", "WowB.exe",
                    // Classic
                    "WowClassic.exe", "WowClassicT.exe", "WowClassicB.exe",
                    // Old 64-bit specific builds
                    "Wow-64.exe", "WowT-64.exe", "WowB-64.exe",
                };

                foreach (string executableName in executableNames)
                {
                    var executableInstallEntries = cascHandler.Install.GetEntriesByName(executableName);

                    if (!executableInstallEntries.Any())
                        continue;

                    cascHandler.GetEncodingKey(executableInstallEntries.First().MD5, out MD5Hash eKey);

                    if (eKey.highPart == 0)
                        continue;

                    return cascHandler.OpenFile(eKey);
                }
            }

            return null;
        }
    }
}
