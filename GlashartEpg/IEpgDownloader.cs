using System;

namespace GlashartEpg
{
    public interface IEpgDownloader
    {
        event EventHandler<EpgGuideFile> DownloadedPart;
        void Download();
        string DownloadDetails(Models.Program program);
    }
}