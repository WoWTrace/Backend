using CASCLib;
using System.Collections.Generic;
using System.IO;
using WoWTrace.Backend.DataModels;

namespace WoWTrace.Backend.Casc
{
    public class CASCConfigWoWTrace : CASCConfig
    {
        public static CASCConfigWoWTrace LoadOnlineStorageConfigWithBuild(Build build, string region = "us", bool useCurrentBuild = false, ILoggerOptions loggerOptions = null)
        {
            Logger.Init(loggerOptions);

            var config = new CASCConfigWoWTrace { OnlineMode = true, Region = region, Product = build.ProductKey };

            using (var ribbit = new RibbitClient("us"))
            using (var cdnsStream = ribbit.GetAsStream($"v1/products/{build.ProductKey}/cdns"))
            {
                config._CDNData = VerBarConfig.ReadVerBarConfig(cdnsStream);
            }

            using (var ribbit = new RibbitClient("us"))
            using (var versionsStream = ribbit.GetAsStream($"v1/products/{build.ProductKey}/versions"))
            {
                config._VersionsData = VerBarConfig.ReadVerBarConfig(versionsStream);
            }

            for (int i = 0; i < config._VersionsData.Count; ++i)
            {
                if (config._VersionsData[i]["Region"] == region)
                {
                    config._versionsIndex = i;
                    break;
                }
            }

            CDNCache.Init(config);

            config.GameType = CASCGame.DetectGameByUid(build.ProductKey);

            string cdnKey = build.CdnConfig;
            using (Stream stream = CDNIndexHandler.OpenConfigFileDirect(config, cdnKey))
            {
                config._CDNConfig = KeyValueConfig.ReadKeyValueConfig(stream);
            }

            config.ActiveBuild = 0;
            config._Builds = new List<KeyValueConfig>();

            if (config._CDNConfig["builds"] != null)
            {
                for (int i = 0; i < config._CDNConfig["builds"].Count; i++)
                {
                    try
                    {
                        using (Stream stream = CDNIndexHandler.OpenConfigFileDirect(config, config._CDNConfig["builds"][i]))
                        {
                            var cfg = KeyValueConfig.ReadKeyValueConfig(stream);
                            config._Builds.Add(cfg);
                        }
                    }
                    catch
                    {

                    }
                }

                if (useCurrentBuild)
                {
                    string curBuildKey = config._VersionsData[config._versionsIndex]["BuildConfig"];

                    int buildIndex = config._CDNConfig["builds"].IndexOf(curBuildKey);

                    if (buildIndex != -1)
                        config.ActiveBuild = buildIndex;
                }
            }

            string buildKey = build.BuildConfig;
            using (Stream stream = CDNIndexHandler.OpenConfigFileDirect(config, buildKey))
            {
                var cfg = KeyValueConfig.ReadKeyValueConfig(stream);
                config._Builds.Add(cfg);
            }

            return config;
        }

    }
}
