using CASCLib;
using FluentScheduler;
using NLog;
using System.Linq;
using WoWTrace.Backend.DataModels;
using WoWTrace.Backend.Jobs;
using WoWTrace.Backend.Queue;
using WoWTrace.Backend.Queue.Consumer.V1;
using Logger = NLog.Logger;

namespace WoWTrace.Backend
{
    class WoWTraceBackend
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public WoWTraceBackend()
        {
            QueueManager.Instance.Initialize();
            InitializeCache();
            InitializeJobs();
        }

        private void InitializeCache()
        {
            CDNCache.Enabled = Settings.Instance.CacheEnabled;
            CDNCache.CacheData = Settings.Instance.CacheData;
            CDNCache.Validate = Settings.Instance.CacheValidate;
            CDNCache.ValidateFast = Settings.Instance.CacheValidateFast;
            CDNCache.CachePath = Settings.Instance.CachePath;
        }

        private void InitializeJobs()
        {
            JobManager.Initialize();
            JobManager.AddJob<CrawlBuildJob>(s => s.NonReentrant().ToRunEvery(5).Minutes());

            (new CrawlBuildJob()).Execute();
        }
    }
}
