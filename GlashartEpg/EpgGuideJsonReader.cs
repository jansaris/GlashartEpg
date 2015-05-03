using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlashartEpg.Models;
using log4net;
using Newtonsoft.Json;

namespace GlashartEpg
{
    public class EpgGuideJsonReader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EpgGuideJsonReader));
        private readonly IConfiguration _config;
        private readonly IEpgDownloader _epgDownloader;

        public EpgGuideJsonReader(IConfiguration config, IEpgDownloader epgDownloader)
        {
            _config = config;
            _epgDownloader = epgDownloader;
            _epgDownloader.DownloadedPart += Parse;
        }

        private void Parse(object sender, EpgGuideFile e)
        {
            var data = LoadFileFromDisk(e.UnzippedFile);
            var list = ParseData(data);
            list.AsParallel().ForAll(channel => channel.Programs.ForEach(p =>
            {
                var json = _epgDownloader.DownloadDetails(p);
                var detailedProgram = JsonConvert.DeserializeObject<Models.Program>(json);
                p.Details = detailedProgram.Details;
                p.Theme = detailedProgram.Theme;
            }));
            Logger.Info("Done");
        }

        private List<Channel> ParseData(string data)
        {
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, List<Models.Program>>>(data);
            return dictionary.Select(kv => new Channel {Name = kv.Key, Programs = kv.Value}).ToList();
        }

        private string LoadFileFromDisk(string unzippedFile)
        {
            try
            {
                Logger.DebugFormat("Load json from disk: {0}", unzippedFile);
                return File.ReadAllText(unzippedFile);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Failed to read the unzipped file from disk: {0}",unzippedFile), ex);
                return null;
            }
        }
    }
}