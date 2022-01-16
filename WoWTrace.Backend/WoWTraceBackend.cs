using CASCLib;
using FluentScheduler;
using NLog;
using WoWTrace.Backend.Jobs;
using Logger = NLog.Logger;

namespace WoWTrace.Backend
{
    class WoWTraceBackend
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public WoWTraceBackend()
        {
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
        }
    }
}
