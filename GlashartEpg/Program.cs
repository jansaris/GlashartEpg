using System;
using System.IO;
using log4net;

namespace GlashartEpg
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));
        private Configuration _config;

        static void Main(params string[] args)
        {
            try
            {
                var level = "";
                if (args.Length > 0) level = args[0];
                LogSetup.Setup(level);
                var program = new Program();
                program.Run();
            }
            catch (Exception ex)
            {
                Logger.Fatal("An unhandled error occured", ex);
            }
            Console.ReadKey();
        }

        private void Run()
        {
            Logger.Info("Weclome to Glashart EPG grabber");
            LoadConfiguration();
            DownloadGuide();
        }

        private void LoadConfiguration()
        {
            Logger.Info("Read configuration");
            _config = new Configuration();
            _config.Load();
            Logger.Debug("Test data folder");
            var dir = new DirectoryInfo(_config.DataFolder);
            if(!dir.Exists) dir.Create();
        }

        private void DownloadGuide()
        {
            Logger.InfoFormat("Start downloading the guide at {0}", _config.EpgUrl);
            var downloader = new EpgDownloader(_config);
            downloader.DownloadedPart += downloader_DownloadedPart;
            downloader.Download();
        }

        void downloader_DownloadedPart(object sender, EpgObject e)
        {
            Logger.InfoFormat("Parse data of {0}",e.UnzippedFile);
        }
    }
}
