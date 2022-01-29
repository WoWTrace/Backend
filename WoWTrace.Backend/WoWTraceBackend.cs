using DotNetWorkQueue;
using FluentScheduler;
using NLog;
using System;
using System.Linq;
using WoWTrace.Backend.DataModels;
using WoWTrace.Backend.Extensions;
using WoWTrace.Backend.Jobs;
using WoWTrace.Backend.Queue;
using WoWTrace.Backend.Queue.Attribute;
using WoWTrace.Backend.Queue.Message;
using static WoWTrace.Backend.Program;
using Logger = NLog.Logger;

namespace WoWTrace.Backend
{
    class WoWTraceBackend
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public WoWTraceBackend(Options options)
        {
            QueueManager.Instance.Initialize();
            InitializeJobs(options.EnqueueAllBuildsEveryFiveHours);

            if (options.EnqueueAllBuilds)
                EnqueueAllBuilds();
        }

        private void InitializeJobs(bool enqueueAllBuildsEveryFiveHours = false)
        {
#if (!DEBUG)
            JobManager.Initialize();
            JobManager.AddJob<CrawlBuildJob>(s => s.NonReentrant().ToRunEvery(5).Minutes());
            
            if (enqueueAllBuildsEveryFiveHours)
                JobManager.AddJob(() => EnqueueAllBuilds(), s => s.NonReentrant().ToRunEvery(5).Hours());
#else
            (new CrawlBuildJob()).Execute();

            if (enqueueAllBuildsEveryFiveHours)
                EnqueueAllBuilds();

            logger.Info("Finish debug crawl build job run!");
#endif
        }

        private void EnqueueAllBuilds()
        {
            using (var db = new WowtraceDB(Settings.Instance.DBConnectionOptions()))
            {
                foreach (var build in db.Builds)
                {
                    IQueueMessage[] messages = AppDomain
                        .CurrentDomain
                        .GetAssemblies()
                        .SelectMany(messages => messages.GetTypes())
                        .Where(typeof(IQueueMessage).IsAssignableFrom)
                        .Where(messageType => !messageType.IsInterface)
                        .Where(messageType => (string)messageType.GetCustomAttributes<MessageType>().Type == MessageType.TypeBuild)
                        .Select(patchInstance => (IQueueMessage)Activator.CreateInstance(patchInstance, build.Id, false))
                        .ToArray();

                    if (!messages.Any())
                        return;

                    foreach (var message in messages)
                        message.PublishMessage();
                }
            }
        }
    }
}
