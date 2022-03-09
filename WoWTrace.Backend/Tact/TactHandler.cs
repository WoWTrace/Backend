using NLog;
using TACT.Net;
using TACT.Net.Configs;

using WoWTrace.Backend.DataModels;

namespace WoWTrace.Backend.Tact
{
    public class TactHandler : IDisposable
    {
        public TACTRepo? Repo { get; private set; }

        public static readonly string CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Instance.CachePath);

        private static readonly object _instanceLock = new();
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public TactHandler(string product, string? patchUrl = null, int buildId = 0, string? versionName = null, string? buildConfig = null, string? cdnConfig = null, Locale locale = Locale.US)
        {
            Initialize(new(product, locale, patchUrl, buildId, versionName, buildConfig, cdnConfig));
        }

        public TactHandler(Build build, Locale locale = Locale.US)
        {
            Initialize(new(build.ProductKey, locale, null, 0, null, build.BuildConfig, build.CdnConfig));
        }

        public TactHandler(ManifestContainer manifest)
        {
            Initialize(manifest);
        }

        private void Initialize(ManifestContainer manifest)
        {
            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);

            lock (_instanceLock)
            {
                _logger.Info($"Open remote product: {manifest.Product}");

                Repo = new()
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
            Repo?.Close();
            Repo = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}