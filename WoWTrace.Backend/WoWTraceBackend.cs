using System.Reflection;

using FluentScheduler;
using NLog;

using WoWTrace.Backend.DataModels;
using WoWTrace.Backend.Jobs;
using WoWTrace.Backend.Queue;
using WoWTrace.Backend.Queue.Attribute;
using WoWTrace.Backend.Queue.Message;
using static WoWTrace.Backend.Program;

namespace WoWTrace.Backend
{
    internal class WoWTraceBackend
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public WoWTraceBackend(Options options)
        {
            Console.WriteLine();

            QueueManager.Instance.Initialize();


            // Crawl at startup
            new DumpBuildInformationJob()
                .Execute();

            return;
            
            // Crawl at startup
            new CrawlBuildJob()
                .Execute();

            InitializeJobs(options.EnqueueAllBuildsEveryFiveHours);

            if (options.EnqueueAllBuilds)
                EnqueueAllBuilds();
        }

        private static void InitializeJobs(bool enqueueAllBuildsEveryFiveHours = false)
        {
#if RELEASE
            _logger.Info("Initialize jobs");
            JobManager.Initialize();
            JobManager.AddJob<CrawlBuildJob>(s => s.NonReentrant().ToRunEvery(5).Minutes());

            if (enqueueAllBuildsEveryFiveHours)
                JobManager.AddJob(EnqueueAllBuilds, s => s.NonReentrant().ToRunEvery(5).Hours());
#else
            if (enqueueAllBuildsEveryFiveHours)
                EnqueueAllBuilds();
#endif
        }

        private static void EnqueueAllBuilds()
        {
            using (var db = new WowtraceDB(Settings.Instance.DbConnectionOptions()))
            {
                foreach (var build in db.Builds)
                {
                    var messages = AppDomain
                        .CurrentDomain
                        .GetAssemblies()
                        .SelectMany(messages => messages.GetTypes())
                        .Where(typeof(IQueueMessage).IsAssignableFrom)
                        .Where(messageType => !messageType.IsInterface)
                        .Where(messageType => messageType.GetCustomAttribute<MessageType>()?.Type == MessageType.TypeBuild)
                        .Select(patchInstance => (IQueueMessage?)Activator.CreateInstance(patchInstance, build.Id, false))
                        .ToArray();

                    if (!messages.Any())
                        return;

                    foreach (var message in messages)
                        message?.PublishMessage();
                }
            }
        }
    }
}