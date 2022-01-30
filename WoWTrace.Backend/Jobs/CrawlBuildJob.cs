using FluentScheduler;
using LinqToDB;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using TACT.Net;
using TACT.Net.Configs;
using TACT.Net.Encoding;
using TACT.Net.Network;
using WoWTrace.Backend.DataModels;
using WoWTrace.Backend.Exceptions;
using WoWTrace.Backend.Queue.Message.V1;
using WoWTrace.Backend.Tact;
using Logger = NLog.Logger;

namespace WoWTrace.Backend.Jobs
{
    class CrawlBuildJob : IJob
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void Execute()
        {
            logger.Info("Start Crawl builds");
            List<Product> productList;

            using (var db = new WowtraceDB(Settings.Instance.DBConnectionOptions()))
                productList = db.Products.Select(p => p).ToList();

            foreach (var product in productList)
                ProcessProductByName(product);
        }

        private void ProcessProductByName(Product product)
        {
            logger.Info($"Process product {product.ProductColumn}");

            ManifestContainer manifest = null;
            ConfigContainer configContainer = null;
            CDNClient cdnClient = null;
            EncodingFile encoding = null;
            string remoteCacheDirectory = (Settings.Instance.CacheEnabled ? TactHandler.CachePath : null);

            try
            {
                manifest = GetManifestByProduct(product.ProductColumn, Locale.EU, remoteCacheDirectory);
                bool hasBuild = true;

                if (manifest.BuildConfigMD5.Value == null)
                    throw new Exception("Cant read build config!");

                // Check if build already exists in database
                using (var db = new WowtraceDB(Settings.Instance.DBConnectionOptions()))
                    hasBuild = db.Builds.Any(build => build.BuildConfig == manifest.BuildConfigMD5.ToString());

                if (hasBuild)
                    throw new SkipException($"Build {manifest.BuildConfigMD5} already exists");

                // Update product
                using (var db = new WowtraceDB(Settings.Instance.DBConnectionOptions()))
                {
                    product.LastBuildConfig = manifest.BuildConfigMD5.ToString();
                    product.LastVersion = manifest.VersionsName;
                    product.Detected = DateTime.Now;
                    db.Update(product);
                }

                // Skip encrypted builds to prevent useless error messages in log
                if (product.Encrypted)
                    throw new SkipException($"Skip encrypted build {manifest.BuildConfigMD5}");

                configContainer = new ConfigContainer();
                cdnClient = new CDNClient(manifest, false, remoteCacheDirectory);

                configContainer.OpenRemote(manifest, remoteCacheDirectory);

                if (configContainer.EncodingEKey.Value == null)
                    throw new Exception($"Cant find encoding systemfile by Ekey {configContainer.EncodingEKey} in buildConfig {manifest.BuildConfigMD5}");

                ulong? id = null;
                encoding = new EncodingFile(cdnClient, configContainer.EncodingEKey, true);

                if (!encoding.TryGetCKeyEntry(configContainer.RootCKey, out EncodingContentEntry rootEncodingEntry))
                    throw new Exception($"Cant find root systemfile by CKey {configContainer.RootCKey} in encoding file (EKey: {configContainer.EncodingEKey})");

                if (!encoding.TryGetCKeyEntry(configContainer.InstallCKey, out EncodingContentEntry installEncodingEntry))
                    throw new Exception($"Cant find install systemfile by CKey {configContainer.InstallCKey} in encoding file (EKey: {configContainer.EncodingEKey})");

                if (!encoding.TryGetCKeyEntry(configContainer.DownloadCKey, out EncodingContentEntry downloadEncodingEntry))
                    throw new Exception($"Cant find download systemfile by CKey {configContainer.DownloadCKey} in encoding file (EKey: {configContainer.EncodingEKey})");

                string sizeCKey = null;
                string sizeEKey = null;
                if (configContainer.DownloadSizeCKey.Value != null && encoding.TryGetCKeyEntry(configContainer.DownloadSizeCKey, out EncodingContentEntry sizeEncodingEntry))
                {
                    sizeCKey = sizeEncodingEntry.CKey.ToString();
                    sizeEKey = sizeEncodingEntry.EKeys.First().ToString();
                }

                Version version = Version.Parse(manifest.VersionsName);

                // Save new build
                using (var db = new WowtraceDB(Settings.Instance.DBConnectionOptions()))
                {
                    id = (ulong)db.Builds
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
                    throw new Exception($"Cant insert build {manifest.BuildConfigMD5} into database");

                ProcessRootMessage.Publish(id.Value);
                ProcessExecutableMessage.Publish(id.Value);
            }
            catch (SkipException ex)
            {
                logger.Trace(ex.Message);
            }
            catch (Exception ex)
            {
                logger.Warn($"Cant process product {product.ProductColumn}:\n {ex.Message}");
            }

            Console.WriteLine(" "); // new line

            encoding?.Close();
            cdnClient = null;
            configContainer = null;
            manifest = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private ManifestContainer GetManifestByProduct(string product, Locale locale = Locale.EU, string remoteCacheDirectory = null)
        {
            ManifestContainer manifest = new ManifestContainer(product, Locale.EU);
            manifest.OpenRemote(remoteCacheDirectory);

            return manifest;
        }
    }
}
