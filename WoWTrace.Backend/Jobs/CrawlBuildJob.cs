using CASCLib;
using FluentScheduler;
using LinqToDB;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WoWTrace.Backend.Casc;
using WoWTrace.Backend.DataModels;
using WoWTrace.Backend.Queue;
using WoWTrace.Backend.Queue.Message.V1;
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

            using (var db = new WowtraceNetDB(Settings.Instance.DBConnectionOptions()))
            {
                productList = db.Products.Select(p => p).ToList();
            }

            foreach (var product in productList)
                ProcessProductByName(product);
        }

        private void ProcessProductByName(Product product)
        {
            logger.Info($"Process product {product.ProductColumn}");

            try
            {
                var versionData = GetVersionDataByProduct(product.ProductColumn);
                bool hasBuild = true;

                // Check if build already exists in database
                using (var db = new WowtraceNetDB(Settings.Instance.DBConnectionOptions()))
                {
                    hasBuild = db.Builds.Any(b => b.BuildConfig == versionData["BuildConfig"]);
                }

                if (hasBuild)
                {
                    logger.Trace("Build already exists");
                    Console.WriteLine(" "); // new line
                    return;
                }

                // Update product
                using (var db = new WowtraceNetDB(Settings.Instance.DBConnectionOptions()))
                {
                    product.LastBuildConfig = versionData["BuildConfig"];
                    product.LastVersion = versionData["VersionsName"];
                    product.Detected = DateTime.Now;
                    db.Update(product);
                }

                // Skip encrypted builds to prevent useless error messages in log
                if (product.Encrypted)
                {
                    logger.Trace("Skip encrypted build");
                    Console.WriteLine(" "); // new line
                    return;
                }

                var version = Version.Parse(versionData["VersionsName"]);

                // Get build config and casc handler
                var cascConfig = CASCConfig.LoadOnlineStorageConfig(product.ProductColumn, "us", false);
                var cascHandler = CASCHandlerWoWTrace.Open(cascConfig, true);

                // Save new build
                using (var db = new WowtraceNetDB(Settings.Instance.DBConnectionOptions()))
                {
                    if (!cascHandler.GetEncodingKey(cascConfig.RootMD5, out MD5Hash rootCdnHash))
                        throw new Exception($"Cant find root systemfile by content hash {cascConfig.RootMD5.ToHexString().ToLower()} in encoding file {cascConfig.EncodingMD5.ToHexString().ToLower()}");

                    if (!cascHandler.GetEncodingKey(cascConfig.InstallMD5, out MD5Hash installCdnHash))
                        throw new Exception($"Cant find install systemfile by content hash {cascConfig.InstallMD5.ToHexString().ToLower()} in encoding file {cascConfig.EncodingMD5.ToHexString().ToLower()}");

                    if (!cascHandler.GetEncodingKey(cascConfig.DownloadMD5, out MD5Hash downloadCdnHash))
                        throw new Exception($"Cant find download systemfile by content hash {cascConfig.DownloadMD5.ToHexString().ToLower()} in encoding file {cascConfig.EncodingMD5.ToHexString().ToLower()}");

                    string? sizeContentHash = cascConfig.Builds[cascConfig.ActiveBuild]["size"][0] ?? null;
                    MD5Hash sizeCdnHash = new MD5Hash();
                    if (sizeContentHash != null)
                    {
                        if (!cascHandler.GetEncodingKey(sizeContentHash.FromHexString().ToMD5(), out sizeCdnHash))
                            throw new Exception($"Cant find size systemfile by content hash {sizeContentHash} in encoding file {cascConfig.EncodingMD5.ToHexString().ToLower()}");
                    }

                    ulong id = (ulong)db.Builds
                        .Value(p => p.BuildConfig, versionData["BuildConfig"])
                        .Value(p => p.CdnConfig, versionData["CDNConfig"])
                        .Value(p => p.PatchConfig, cascConfig.Builds[cascConfig.ActiveBuild]["patch-config"]?[0] ?? null)
                        .Value(p => p.ProductConfig, versionData["ProductConfig"] ?? null)
                        .Value(p => p.ProductKey, product.ProductColumn)
                        .Value(p => p.Expansion, version.Major.ToString())
                        .Value(p => p.Major, version.Minor.ToString())
                        .Value(p => p.Minor, version.Build.ToString())
                        .Value(p => p.ClientBuild, (uint)version.Revision)
                        .Value(p => p.Name, cascConfig.BuildName ?? $"WOW-{version.Revision}patch{version.Major}.{version.Minor}.{version.Build}")
                        .Value(p => p.EncodingContentHash, cascConfig.EncodingMD5.ToHexString().ToLower())
                        .Value(p => p.EncodingCdnHash, cascConfig.EncodingKey.ToHexString().ToLower())
                        .Value(p => p.RootContentHash, cascConfig.RootMD5.ToHexString().ToLower())
                        .Value(p => p.RootCdnHash, rootCdnHash.ToHexString().ToLower())
                        .Value(p => p.InstallContentHash, cascConfig.InstallMD5.ToHexString().ToLower())
                        .Value(p => p.InstallCdnHash, installCdnHash.ToHexString().ToLower())
                        .Value(p => p.DownloadContentHash, cascConfig.DownloadMD5.ToHexString().ToLower())
                        .Value(p => p.DownloadCdnHash, downloadCdnHash.ToHexString().ToLower())
                        .Value(p => p.SizeContentHash, sizeContentHash ?? null)
                        .Value(p => p.SizeCdnHash, (sizeCdnHash.highPart != 0 ? sizeCdnHash.ToHexString().ToLower() : null))
                        .Value(p => p.CreatedAt, DateTime.Now)
                        .Value(p => p.UpdatedAt, DateTime.Now)
                        .InsertWithIdentity();

                    QueueManager.Instance.Publish(new ProcessExecutableMessage() { BuildId = id }, QueueManager.Instance.FastlineQueue);
                }

            }
            catch (Exception ex)
            {
                logger.Warn($"Cant process product {product.ProductColumn}:\n {ex.Message}");
            }

            Console.WriteLine(" "); // new line
        }

        private Dictionary<string, string> GetVersionDataByProduct(string product, string region = "us")
        {
            VerBarConfig versionsData;
            int versionsIndex = 0;

            var a = (new RibbitClient(region)).Get($"v1/products/{product}/versions");
            using (var ribbit = new RibbitClient(region))
            using (var versionsStream = ribbit.GetAsStream($"v1/products/{product}/versions"))
            {
                versionsData = VerBarConfig.ReadVerBarConfig(versionsStream);
            }

            for (int i = 0; i < versionsData.Count; ++i)
            {
                if (versionsData[i]["Region"] == region)
                {
                    versionsIndex = i;
                    break;
                }
            }

            return versionsData[versionsIndex];
        }
    }
}
