using CASCLib;
using System;
using System.IO;
using System.Linq;
using WoWTrace.Backend.DataModels;

namespace WoWTrace.Backend.Casc
{
    public sealed class CASCHandlerWoWTrace : CASCHandlerBase, IDisposable
    {
        public EncodingHandler Encoding;
        public WowRootHandler Root;
        public InstallHandler Install;
        protected LocaleFlags? locale;
        protected static object instanceLock = new object();

        private CASCHandlerWoWTrace(CASCConfig config, bool loadOnlyEncoding = false, LocaleFlags? locale = null, BackgroundWorkerEx worker = null) : base(config, worker)
        {
            if (Config.GameType != CASCGameType.WoW)
                throw new Exception("Unsupported game " + Config.BuildUID);

            this.locale = locale;

            using (var fs = OpenEncodingFile(this))
                Encoding = new EncodingHandler(fs, worker);

            if (!loadOnlyEncoding)
                LoadEntries();

            GC.Collect();
        }

        protected void LoadEntries(BackgroundWorkerEx worker = null)
        {
            if (Root != null || Install != null)
                return;

            KeyService.LoadKeys();
            KeyOnlineService.LoadKeys();

            using (var fs = OpenRootFile(Encoding, this))
                Root = new WowRootHandler(fs, worker);

            if (locale != null)
                Root.SetFlags(locale.Value, false, false);

            using (var fs = OpenInstallFile(Encoding, this))
                Install = new InstallHandler(fs, worker);

        }

        protected override Stream OpenFileOnline(in MD5Hash key)
        {
            IndexEntry idxInfo = CDNIndex.GetIndexInfo(key);
            return OpenFileOnlineInternal(idxInfo, key);
        }

        protected override void ExtractFileOnline(in MD5Hash key, string path, string name)
        {
            IndexEntry idxInfo = CDNIndex.GetIndexInfo(key);
            ExtractFileOnlineInternal(idxInfo, key, path, name);
        }

        public static CASCHandlerWoWTrace OpenOnlineStorage(string product, bool loadOnlyEncoding = false, LocaleFlags? locale = null, string region = "us", BackgroundWorkerEx worker = null)
        {
            CASCConfig config = CASCConfig.LoadOnlineStorageConfig(product, region);

            return Open(config, loadOnlyEncoding, locale, worker);
        }

        public static CASCHandlerWoWTrace OpenOnlineStorageWithBuild(Build build, bool loadOnlyEncoding = false, LocaleFlags? locale = null, string region = "us", BackgroundWorkerEx worker = null)
        {
            CASCConfigWoWTrace config = CASCConfigWoWTrace.LoadOnlineStorageConfigWithBuild(build, region);

            return Open(config, loadOnlyEncoding, locale, worker);
        }

        public static CASCHandlerWoWTrace Open(CASCConfig config, bool loadOnlyEncoding = false, LocaleFlags? locale = null, BackgroundWorkerEx worker = null)
        {
            return new CASCHandlerWoWTrace(config, loadOnlyEncoding, locale, worker);
        }

        public override bool FileExists(int fileDataId)
        {
            LoadEntries();

            if (Root is WowRootHandler rh)
                return rh.FileExist(fileDataId);
            return false;
        }

        public override bool FileExists(string file) => FileExists(Hasher.ComputeHash(file));

        public override bool FileExists(ulong hash)
        {
            LoadEntries();

            return Root.GetAllEntries(hash).Any();
        }

        public bool GetEncodingEntry(ulong hash, out EncodingEntry enc)
        {
            if (GetCKeyForHash(hash, out MD5Hash cKey))
                return Encoding.GetEntry(cKey, out enc);

            enc = default;
            return false;
        }

        public bool GetEncodingEntry(in MD5Hash cKey, out EncodingEntry enc)
        {
            return Encoding.GetEntry(cKey, out enc);
        }

        public long GetFileSize(ulong hash)
        {
            if (GetEncodingEntry(hash, out EncodingEntry enc))
                return enc.Size;

            return 0;
        }

        public long GetFileSize(in MD5Hash cKey)
        {
            if (GetEncodingEntry(cKey, out EncodingEntry enc))
                return enc.Size;

            return 0;
        }

        private bool GetCKeyForHash(ulong hash, out MD5Hash cKey)
        {
            LoadEntries();

            var rootInfos = Root.GetEntries(hash);
            if (rootInfos.Any())
            {
                cKey = rootInfos.First().MD5;
                return true;
            }

            var installInfos = Install.GetEntries().Where(e => Hasher.ComputeHash(e.Name) == hash && e.Tags.Any(t => t.Type == 1 && t.Name == Root.Locale.ToString()));
            if (installInfos.Any())
            {
                cKey = installInfos.First().MD5;
                return true;
            }

            installInfos = Install.GetEntries().Where(e => Hasher.ComputeHash(e.Name) == hash);
            if (installInfos.Any())
            {
                cKey = installInfos.First().MD5;
                return true;
            }

            cKey = default;
            return false;
        }

        public bool GetEncodingKey(ulong hash, out MD5Hash eKey)
        {
            if (GetCKeyForHash(hash, out MD5Hash cKey))
                return Encoding.TryGetBestEKey(cKey, out eKey);

            eKey = default;
            return false;
        }

        public bool GetEncodingKey(in MD5Hash cKey, out MD5Hash eKey)
        {
            return Encoding.TryGetBestEKey(cKey, out eKey);
        }

        public override Stream OpenFile(int fileDataId)
        {
            LoadEntries();

            if (Root is WowRootHandler rh)
                return OpenFile(rh.GetHashByFileDataId(fileDataId));

            if (CASCConfig.ThrowOnFileNotFound)
                throw new FileNotFoundException($"FileData: {fileDataId}");
            return null;
        }

        public override Stream OpenFile(string name) => OpenFile(Hasher.ComputeHash(name));

        public override Stream OpenFile(ulong hash)
        {
            if (GetEncodingKey(hash, out MD5Hash eKey))
                return OpenFile(eKey);

            if (CASCConfig.ThrowOnFileNotFound)
                throw new FileNotFoundException($"{hash:X16}");
            return null;
        }

        public override Stream OpenFile(in MD5Hash key)
        {
            lock (instanceLock)
            {
                return base.OpenFile(key);
            }
        }

        public override void SaveFileTo(ulong hash, string extractPath, string fullName)
        {
            if (GetEncodingKey(hash, out MD5Hash eKey))
            {
                SaveFileTo(eKey, extractPath, fullName);
                return;
            }

            if (CASCConfig.ThrowOnFileNotFound)
                throw new FileNotFoundException(fullName);
        }

        protected override Stream GetLocalDataStream(in MD5Hash key)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Encoding?.Clear();
            Root?.Clear();
            Install?.Clear();
            LocalIndex?.Clear();
            CDNIndex?.Clear();

            GC.Collect();
        }
    }
}
