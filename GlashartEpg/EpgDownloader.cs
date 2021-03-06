﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using log4net;

namespace GlashartEpg
{
    public class EpgDownloader : IEpgDownloader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EpgDownloader));

        private string _url;
        private int _days;
        private DirectoryInfo _folder;
        private readonly IConfiguration _config;
        public event EventHandler<EpgGuideFile> DownloadedPart;

        protected virtual void OnDownloadedPart(EpgGuideFile e)
        {
            var handler = DownloadedPart;
            if (handler != null) handler(this, e);
        }

        public EpgDownloader(IConfiguration config)
        {
            _config = config;
        }

        public void Download()
        {
            LoadSettings();
            if(!_folder.Exists) _folder.Create();
            var data = GenerateList();
            DownloadList(data);
        }

        public string DownloadDetails(Models.Program program)
        {
            var url = GenerateDetailsUrl(program.Id);
            if (string.IsNullOrWhiteSpace(url)) return null;

            try
            {
                using (var webClient = new WebClient())
                {
                    return webClient.DownloadString(url);
                }
            }
            catch (WebException ex)
            {
                Logger.Info(string.Format("Failed to download details for {0} with id {1}", program.Name, program.Id), ex);
                return null;
            }
        }

        private string GenerateDetailsUrl(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length < 2) return null;
            var dir = id.Substring(id.Length - 2, 2);
            return string.Format("{0}/{1}/{2}.json", _url, dir, id);
        }

        private void LoadSettings()
        {
            _url = _config.EpgUrl;
            _days = _config.Days;
            _folder = new DirectoryInfo(Path.Combine(_config.DataFolder, "EpgDownloader"));
        }

        private void DownloadList(IEnumerable<EpgGuideFile> data)
        {
            data.AsParallel().ForAll(obj =>
            {
                var epgData = DownloadFile(obj.Url);
                if (epgData == null || epgData.Length == 0) return;
                if (!SaveDataToFile(epgData, obj.File)) return;
                if (!ExtractGZipSample(obj.File, obj.UnzippedFile)) return;
                File.Delete(obj.File);
                Logger.InfoFormat("Succesfully extracted: {0}", obj.UnzippedFile);
                OnDownloadedPart(obj);
            });
        }

        // Evxtracts the file contained within a GZip to the target dir.
        // A GZip can contain only one file, which by default is named the same as the GZip except
        // without the extension.
        //
        private bool ExtractGZipSample(string gzipFileName, string filename)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gzipFileName))
                {
                    throw new Exception("Null gzipFieName");
                }

                // Use a 4K buffer. Any larger is a waste.    
                var dataBuffer = new byte[4096];

                using (Stream fs = new FileStream(gzipFileName, FileMode.Open, FileAccess.Read))
                using (var gzipStream = new GZipInputStream(fs))
                {
                    // Change this to your needs
                    //var fnOut = Path.Combine(targetDir, Path.GetFileNameWithoutExtension(gzipFileName));
                    using (var fsOut = File.Create(filename))
                    {
                        StreamUtils.Copy(gzipStream, fsOut, dataBuffer);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Failed to extract {0}",gzipFileName), ex);
                return false;
            }
        }

        private bool SaveDataToFile(byte[] data, string file)
        {
            try
            {
                Logger.DebugFormat("Write downloaded data into {0}", file);
                using (var writer = new StreamWriter(file))
                {
                    writer.BaseStream.Write(data,0,data.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Failed to save the data to {0}", file), ex);
                return false;
            }
        }

        private byte[] DownloadFile(string url)
        {
            try
            {
                Logger.InfoFormat("Start downloading {0}", url);
                using (var webClient = new WebClient())
                {
                    return webClient.DownloadData(url);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Failed to download {0}", url), ex);
                return null;
            }
        }


        //w.zt6.nl/epgdata/epgdata.yyyyMMdd.?.json.gz
        private IEnumerable<EpgGuideFile> GenerateList()
        {
            Logger.DebugFormat("Start generating a list of EpgObjects for {0} days",_days);
            var list = new List<EpgGuideFile>();
            var now = DateTime.Now;
            var end = DateTime.Now.AddDays(_days);
            var count = 0;
            while (now < end)
            {
                var date = now.ToString("yyyyMMdd");
                list.AddRange(Enumerable.Range(0, 8).Select(nr => new EpgGuideFile
                {
                    Id = count + nr,
                    Url = string.Format("{0}epgdata.{1}.{2}.json.gz",_url,date,nr),
                    File = string.Format("epgdata.{0}.{1}.json.gz",date,nr),
                    UnzippedFile = string.Format("epgdata.{0}.{1}.json",date,nr)
                }));
                count += 8;
                now = now.AddDays(1);
            }
            list.AsParallel().ForAll(obj => obj.File = Path.Combine(_folder.FullName, obj.File));
            list.AsParallel().ForAll(obj => obj.UnzippedFile = Path.Combine(_folder.FullName, obj.UnzippedFile));
            Logger.DebugFormat("Generated a list of {0} epg files to download", list.Count);
            return list;
        }
    }
}