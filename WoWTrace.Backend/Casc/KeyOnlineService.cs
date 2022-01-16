using CASCLib;
using System;
using System.Globalization;
using System.IO;
using System.Net;

namespace WoWTrace.Backend.Casc
{
    class KeyOnlineService
    {
        public static void LoadKeys()
        {
            if (!Directory.Exists(CDNCache.CachePath))
                Directory.CreateDirectory(CDNCache.CachePath);

            string keyFile = Path.Combine(CDNCache.CachePath, "EncryptKeys.txt");

            if (!File.Exists(keyFile) || (DateTime.Now - File.GetLastWriteTime(keyFile)).TotalHours > 6)
            {
                WebClient webClient = new WebClient();
                webClient.BaseAddress = "https://raw.githubusercontent.com/wowdev/TACTKeys/master/";

                try
                {
                    File.WriteAllText(keyFile, webClient.DownloadString("WoW.txt"));
                }
                catch
                {
                    return;
                }
            }

            using (StreamReader sr = new StreamReader(keyFile))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] tokens = line.Split(' ');

                    if (tokens.Length != 2)
                        continue;

                    ulong keyName = ulong.Parse(tokens[0], NumberStyles.HexNumber);
                    string keyStr = tokens[1];

                    if (keyStr.Length != 32)
                        continue;

                    KeyService.SetKey(keyName, keyStr.FromHexString());
                }
            }
        }
    }
}
