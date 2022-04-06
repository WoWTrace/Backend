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

/// <summary>
/// Temp job to dump all builds from a TACT backup server ;) 
/// </summary>
namespace WoWTrace.Backend.Jobs
{
    struct BuildCSVEntry
    {
        public string patch;
        public int build;
        public string product;
        public string buildConfig;
        public string cdnConfig;
        public DateTime? compiledAt;
        public DateTime? detectedAt;
    }

    
    internal class DumpBuildInformationJob : IJob
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly string buildCsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "build.csv");

        public void Execute()
        {
            if (!File.Exists(buildCsvPath))
            {
                _logger.Fatal("build.csv not found!");
                return;
            }

            List<BuildCSVEntry> buildCSVEntries = ReadBuildCsv(buildCsvPath);
            buildCSVEntries.Reverse();

            foreach (var buildCsvEntry in buildCSVEntries)
                ProcessBuildCsvEntry(buildCsvEntry);
        }

        private List<BuildCSVEntry> ReadBuildCsv(string buildCsvPath)
        {
            var result = new List<BuildCSVEntry>();
            using (var reader = new StreamReader(buildCsvPath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        continue;

                    var values = line.Split(',');
                    if (values.Length != 7)
                        continue;

                    result.Add(new BuildCSVEntry
                    {
                        patch = values[0],
                        build = int.Parse(values[1]),
                        product = values[2],
                        buildConfig = values[3],
                        cdnConfig = values[4],
                        compiledAt = (values[5].Trim() != "" ? DateTime.Parse(values[5]) : null),
                        detectedAt = (values[6].Trim() != "" ? DateTime.Parse(values[6]) : null)
                    });
                }
            }

            return result;
        }

        private void ProcessBuildCsvEntry(BuildCSVEntry buildCsvEntry)
        {
            _logger.Info($"Process build {buildCsvEntry.patch}.{buildCsvEntry.build} ({buildCsvEntry.buildConfig})");
            ManifestContainer? manifest;
            EncodingFile? encoding = null;

            var remoteCacheDirectory = Settings.Instance.CacheEnabled ? TactHandler.CachePath : null;
            try
            {
                manifest = GetManifestFromBuildCsvEntry(buildCsvEntry, remoteCacheDirectory: remoteCacheDirectory);
                if (manifest.BuildConfigMD5.Value == null)
                    throw new EncryptedBuildConfigException("Cant read build config! Encrypted");


                // Get product
                Product product;
                using (var db = new WowtraceDB(Settings.Instance.DbConnectionOptions()))
                    product = db.Products.First(product => product.ProductColumn == buildCsvEntry.product);

                if (product == null)
                    throw new Exception($"Product {buildCsvEntry.product} not found!");

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
                    product.Detected = buildCsvEntry.detectedAt ?? DateTime.Now;
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
                        .Value(p => p.PatchConfig,
                            configContainer.PatchConfigMD5.Value != null
                                ? configContainer.PatchConfigMD5.ToString()
                                : null)
                        .Value(p => p.ProductConfig, manifest.ProductConfig)
                        .Value(p => p.ProductKey, buildCsvEntry.product)
                        .Value(p => p.Expansion, version.Major.ToString())
                        .Value(p => p.Major, version.Minor.ToString())
                        .Value(p => p.Minor, version.Build.ToString())
                        .Value(p => p.ClientBuild, (uint)version.Revision)
                        .Value(p => p.Name,
                            configContainer.BuildConfig.GetValue("build-name") ??
                            $"WOW-{version.Revision}patch{version.Major}.{version.Minor}.{version.Build}")
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
                        .Value(p => p.CreatedAt, buildCsvEntry.detectedAt ?? DateTime.Now)
                        .Value(p => p.UpdatedAt, DateTime.Now)
                        .Value(p => p.CompiledAt, buildCsvEntry.compiledAt)
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
            catch (EncryptedBuildConfigException ex)
            {
                _logger.Warn(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error($"Cant process build {buildCsvEntry.patch}.{buildCsvEntry.build} ({buildCsvEntry.buildConfig}):\n {ex}");
            }

            encoding?.Close();

            /*
            ManifestContainer? manifest;
            EncodingFile? encoding = null;

            var remoteCacheDirectory = Settings.Instance.CacheEnabled ? TactHandler.CachePath : null;
            try
            {
                manifest = GetManifestByProduct(buildCsvEntry.ProductColumn, remoteCacheDirectory: remoteCacheDirectory);
                if (manifest.BuildConfigMD5.Value == null)
                    throw new EncryptedBuildConfigException("Cant read build config! Encrypted");

                // Check if build already exists in database
                bool hasBuild;
                using (var db = new WowtraceDB(Settings.Instance.DbConnectionOptions()))
                    hasBuild = db.Builds.Any(build => build.BuildConfig == manifest.BuildConfigMD5.ToString());

                if (hasBuild)
                    throw new SkipException($"Build {manifest.BuildConfigMD5} already exists");

                // Update product
                using (var db = new WowtraceDB(Settings.Instance.DbConnectionOptions()))
                {
                    buildCsvEntry.LastBuildConfig = manifest.BuildConfigMD5.ToString();
                    buildCsvEntry.LastVersion = manifest.VersionsName;
                    buildCsvEntry.Detected = DateTime.Now;
                    db.Update(buildCsvEntry);
                }

                if (buildCsvEntry.Encrypted)
                    throw new SkipException($"Skip encrypted build {buildCsvEntry.ProductColumn} - {buildCsvEntry.LastVersion} - {manifest.BuildConfigMD5}");

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
                if (configContainer.DownloadSizeCKey.Value != null &&
                    encoding.TryGetCKeyEntry(configContainer.DownloadSizeCKey, out var sizeEncodingEntry))
                {
                    sizeCKey = sizeEncodingEntry.CKey.ToString();
                    sizeEKey = sizeEncodingEntry.EKeys.First().ToString();
                }

                var version = Version.Parse(manifest.VersionsName);

                // Save new build
                using (var db = new WowtraceDB(Settings.Instance.DbConnectionOptions()))
                {
                    id = (ulong?) db.Builds
                        .Value(p => p.BuildConfig, manifest.BuildConfigMD5.ToString())
                        .Value(p => p.CdnConfig, manifest.CDNConfigMD5.ToString())
                        .Value(p => p.PatchConfig,
                            configContainer.PatchConfigMD5.Value != null
                                ? configContainer.PatchConfigMD5.ToString()
                                : null)
                        .Value(p => p.ProductConfig, manifest.ProductConfig)
                        .Value(p => p.ProductKey, buildCsvEntry.ProductColumn)
                        .Value(p => p.Expansion, version.Major.ToString())
                        .Value(p => p.Major, version.Minor.ToString())
                        .Value(p => p.Minor, version.Build.ToString())
                        .Value(p => p.ClientBuild, (uint) version.Revision)
                        .Value(p => p.Name,
                            configContainer.BuildConfig.GetValue("build-name") ??
                            $"WOW-{version.Revision}patch{version.Major}.{version.Minor}.{version.Build}")
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
            catch (EncryptedBuildConfigException ex)
            {
                _logger.Warn(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error($"Cant process product {buildCsvEntry.ProductColumn}:\n {ex}");
            }

            encoding?.Close();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            */
        }

        private static ManifestContainer GetManifestFromBuildCsvEntry(BuildCSVEntry buildCSVEntry, Locale locale = Locale.US, string? remoteCacheDirectory = null)
        {
            var manifest = new ManifestContainer(buildCSVEntry.product, locale, null, buildCSVEntry.build, $"{buildCSVEntry.patch}.{buildCSVEntry.build}", buildCSVEntry.buildConfig, buildCSVEntry.cdnConfig);
            manifest.OpenRemote();
            
            string cdnHosts = (manifest.CDNsFile.GetValue("Hosts", locale) ?? "");
            if (!cdnHosts.Contains("wow.tools"))
            {
                cdnHosts += " wow.tools";
                manifest.CDNsFile.SetValue("Hosts", cdnHosts.Trim());
            }

            string cdnServs = (manifest.CDNsFile.GetValue("Servers", locale) ?? "");
            if (!cdnServs.Contains("wow.tools"))
            {
                cdnServs += " https://wow.tools/";
                manifest.CDNsFile.SetValue("Servers", cdnServs.Trim());
            }
            
            string remoteCacheDirectoryWithProduct = Path.Combine(remoteCacheDirectory, "manifest", manifest.VersionsFile.GetValue("BuildConfig", locale));            
            manifest.Save(remoteCacheDirectoryWithProduct);

            return manifest;
        }
    }
}