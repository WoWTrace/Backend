using NLog;
using System;
using System.IO;
using TACT.Net;
using TACT.Net.Configs;
using WoWTrace.Backend.DataModels;

namespace WoWTrace.Backend.Tact
{
    public class TactHandler : IDisposable
    {
        public TACTRepo Repo;

        public static string CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Instance.CachePath);
        protected static object instanceLock = new object();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public TactHandler(string product, string patchUrl = null, int buildId = 0, string versionName = null, string buildConfig = null, string cdnConfig = null, Locale locale = Locale.US)
        {
            Initilize(new ManifestContainer(product, locale, patchUrl, buildId, versionName, buildConfig, cdnConfig));
        }

        public TactHandler(Build build, Locale locale = Locale.US)
        {
            Initilize(new ManifestContainer(build.ProductKey, locale, null, 0, null, build.BuildConfig, build.CdnConfig));
        }

        public TactHandler(ManifestContainer manifest)
        {
            Initilize(manifest);
        }

        private void Initilize(ManifestContainer manifest)
        {
            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);

            lock (instanceLock)
            {
                logger.Info($"Open remote product: {manifest.Product}");

                Repo = new TACTRepo()
                {
                    RemoteCache = Settings.Instance.CacheEnabled,
                    RemoteCacheDirectory = CachePath,
                    ManifestContainer = manifest
                };

                Repo.OpenRemote(manifest.Product, manifest.Locale);
            }
        }

        public void Dispose()
        {
            Repo.Close();
            Repo = null;
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
