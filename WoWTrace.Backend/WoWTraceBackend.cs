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

namespace WoWTrace.Backend
{
    class WoWTraceBackend
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public WoWTraceBackend(Options options)
        {
            Console.WriteLine();

            QueueManager.Instance.Initialize();

            // Crawl at startup
            (new CrawlBuildJob()).Execute();

            InitializeJobs(options.EnqueueAllBuildsEveryFiveHours);

            if (options.EnqueueAllBuilds)
                EnqueueAllBuilds();
        }

        private void InitializeJobs(bool enqueueAllBuildsEveryFiveHours = false)
        {
#if (RELEASE)
            logger.Info("Initialize jobs");
            JobManager.Initialize();
            JobManager.AddJob<CrawlBuildJob>(s => s.NonReentrant().ToRunEvery(5).Minutes());
            
            if (enqueueAllBuildsEveryFiveHours)
                JobManager.AddJob(() => EnqueueAllBuilds(), s => s.NonReentrant().ToRunEvery(5).Hours());
#else
            if (enqueueAllBuildsEveryFiveHours)
                EnqueueAllBuilds();
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
                        .Where(messageType => messageType.GetCustomAttributes<MessageType>().Type == MessageType.TypeBuild)
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
