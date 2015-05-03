using System;
using System.IO;
using log4net;

namespace GlashartEpg
{
    public class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));
        private readonly Configuration _config;
        private readonly IEpgDownloader _epgDownloader;
        private readonly EpgGuideJsonReader _epgGuideJsonReader;

        public Program()
        {
            _config = new Configuration();
            _epgDownloader = new LocalEpgDownloader(_config);
            _epgGuideJsonReader = new EpgGuideJsonReader(_config, _epgDownloader);
            
        }

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
            _config.Load();
            Logger.Debug("Test data folder");
            var dir = new DirectoryInfo(_config.DataFolder);
            if(!dir.Exists) dir.Create();
        }

        private void DownloadGuide()
        {
            Logger.InfoFormat("Start downloading the guide at {0}", _config.EpgUrl);
            _epgDownloader.Download();
        }
    }
}
