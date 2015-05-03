using System;
using System.IO;
using System.Linq;
using System.Net;

namespace GlashartEpg
{
    public class LocalEpgDownloader : IEpgDownloader
    {
        private DirectoryInfo _folder;
        private readonly IConfiguration _config;

        public LocalEpgDownloader(IConfiguration config)
        {
            _config = config;
        }

        public event EventHandler<EpgGuideFile> DownloadedPart;

        protected virtual void OnDownloadedPart(EpgGuideFile e)
        {
            EventHandler<EpgGuideFile> handler = DownloadedPart;
            if (handler != null) handler(this, e);
        }

        public void Download()
        {
            _folder = new DirectoryInfo(Path.Combine(_config.DataFolder, "EpgDownloader"));
            var list = _folder.GetFiles().ToList().OrderBy(f => f.Name);
            int count = 0;
            foreach (var file in list)
            {
                var obj = new EpgGuideFile
                {
                    UnzippedFile = file.FullName, 
                    Id = count++
                };
                OnDownloadedPart(obj);
            }
        }

        public string DownloadDetails(Models.Program program)
        {
            var data = string.Format("'details': '{0} details',  'theme': 'Sport'", program.Name.Replace('\'','_'));
            return "{" + data + "}";
        }
    }
}