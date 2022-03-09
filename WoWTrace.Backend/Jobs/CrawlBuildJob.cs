using FluentScheduler;
using LinqToDB;
using NLog;
using TACT.Net;
using TACT.Net.Configs;
using TACT.Net.Encoding;
using TACT.Net.Network;

using WoWTrace.Backend.DataModels;
using WoWTrace.Backend.Exceptions;
using WoWTrace.Backend.Queue.Message.V1;
using WoWTrace.Backend.Tact;

namespace WoWTrace.Backend.Jobs
{
    internal class CrawlBuildJob : IJob
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void Execute()
        {
            _logger.Info("Start Crawl builds");
            List<Product> productList;

            using (var db = new WowtraceDB(Settings.Instance.DbConnectionOptions()))
                productList = db.Products.Select(p => p).ToList();

            foreach (var product in productList)
                ProcessProductByName(product);
        }

        private void ProcessProductByName(Product product)
        {
            _logger.Info($"Process product {product.ProductColumn}");

            ManifestContainer? manifest;
            EncodingFile? encoding = null;

            var remoteCacheDirectory = Settings.Instance.CacheEnabled ? TactHandler.CachePath : null;
            try
            {
                // Skip encrypted builds to prevent useless error messages in log
                if (product.Encrypted)
                {
                    // Update product
                    using (var db = new WowtraceDB(Settings.Instance.DbConnectionOptions()))
                    {
                        product.Detected = DateTime.Now;
                        db.Update(product);
                    }

                    throw new SkipException($"Skip encrypted build {product.ProductColumn} - {product.LastVersion}");
                }

                manifest = GetManifestByProduct(product.ProductColumn, Locale.EU, remoteCacheDirectory);
                if (manifest.BuildConfigMD5.Value == null)
                    throw new("Cant read build config!");

                // Check if build already exists in database
                bool hasBuild;
                using (var db = new WowtraceDB(Settings.Instance.DbConnectionOptions()))
                    hasBuild = db.Builds.Any(build => build.BuildConfig == manifest.BuildConfigMD5.ToString());

                if (hasBuild)
                    throw new SkipException($"Build {manifest.BuildConfigMD5} already exists");

                // Update product
                using (var db = new WowtraceDB(Settings.Instance.DbConnectionOptions()))
                {
                    product.LastBuildConfig = manifest.BuildConfigMD5.ToString();
                    product.LastVersion = manifest.VersionsName;
                    product.Detected = DateTime.Now;
                    db.Update(product);
                }

                var configContainer = new ConfigContainer();
                configContainer.OpenRemote(manifest, remoteCacheDirectory);

                if (configContainer.EncodingEKey.Value == null)
                    throw new($"Cant find encoding system file by EKey {configContainer.EncodingEKey} in buildConfig {manifest.BuildConfigMD5}");

                ulong? id;

                var cdnClient = new CDNClient(manifest, false, remoteCacheDirectory);
                encoding = new(cdnClient, configContainer.EncodingEKey, true);

                if (!encoding.TryGetCKeyEntry(configContainer.RootCKey, out var rootEncodingEntry))
                    throw new($"Cant find root system file by CKey {configContainer.RootCKey} in encoding file (EKey: {configContainer.EncodingEKey})");

                if (!encoding.TryGetCKeyEntry(configContainer.InstallCKey, out var installEncodingEntry))
                    throw new($"Cant find install system file by CKey {configContainer.InstallCKey} in encoding file (EKey: {configContainer.EncodingEKey})");

                if (!encoding.TryGetCKeyEntry(configContainer.DownloadCKey, out var downloadEncodingEntry))
                    throw new($"Cant find download system file by CKey {configContainer.DownloadCKey} in encoding file (EKey: {configContainer.EncodingEKey})");

                string? sizeCKey = null;
                string? sizeEKey = null;
                if (configContainer.DownloadSizeCKey.Value != null && encoding.TryGetCKeyEntry(configContainer.DownloadSizeCKey, out var sizeEncodingEntry))
                {
                    sizeCKey = sizeEncodingEntry.CKey.ToString();
                    sizeEKey = sizeEncodingEntry.EKeys.First().ToString();
                }

                var version = Version.Parse(manifest.VersionsName);

                // Save new build
                using (var db = new WowtraceDB(Settings.Instance.DbConnectionOptions()))
                {
                    id = (ulong?)db.Builds
                        .Value(p => p.BuildConfig, manifest.BuildConfigMD5.ToString())
                        .Value(p => p.CdnConfig, manifest.CDNConfigMD5.ToString())
                        .Value(p => p.PatchConfig, configContainer.PatchConfigMD5.Value != null ? configContainer.PatchConfigMD5.ToString() : null)
                        .Value(p => p.ProductConfig, manifest.ProductConfig)
                        .Value(p => p.ProductKey, product.ProductColumn)
                        .Value(p => p.Expansion, version.Major.ToString())
                        .Value(p => p.Major, version.Minor.ToString())
                        .Value(p => p.Minor, version.Build.ToString())
                        .Value(p => p.ClientBuild, (uint)version.Revision)
                        .Value(p => p.Name, configContainer.BuildConfig.GetValue("build-name") ?? $"WOW-{version.Revision}patch{version.Major}.{version.Minor}.{version.Build}")
                        .Value(p => p.EncodingContentHash, configContainer.EncodingCKey.ToString())
                        .Value(p => p.EncodingCdnHash, configContainer.EncodingEKey.ToString())
                        .Value(p => p.RootContentHash, configContainer.RootCKey.ToString())
                        .Value(p => p.RootCdnHash, rootEncodingEntry.EKeys.First().ToString())
                        .Value(p => p.InstallContentHash, configContainer.InstallCKey.ToString())
                        .Value(p => p.InstallCdnHash, installEncodingEntry.EKeys.First().ToString())
                        .Value(p => p.DownloadContentHash, configContainer.DownloadCKey.ToString())
                        .Value(p => p.DownloadCdnHash, downloadEncodingEntry.EKeys.First().ToString())
                        .Value(p => p.SizeContentHash, sizeCKey)
                        .Value(p => p.SizeCdnHash, sizeEKey)
                        .Value(p => p.ProcessedBy, "[]")
                        .Value(p => p.CreatedAt, DateTime.Now)
                        .Value(p => p.UpdatedAt, DateTime.Now)
                        .InsertWithInt64Identity();
                }

                if (id == null)
                    throw new($"Cant insert build {manifest.BuildConfigMD5} into database");

                ProcessRootMessage.Publish(id.Value);
                ProcessExecutableMessage.Publish(id.Value);
            }
            catch (SkipException ex)
            {
                _logger.Trace(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error($"Cant process product {product.ProductColumn}:\n {ex}");
            }

            encoding?.Close();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private static ManifestContainer GetManifestByProduct(string product, Locale locale = Locale.EU, string? remoteCacheDirectory = null)
        {
            var manifest = new ManifestContainer(product, locale);
            manifest.OpenRemote(remoteCacheDirectory);

            return manifest;
        }
    }
}