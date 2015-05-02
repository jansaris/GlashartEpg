using System;
using log4net;
using log4net.Config;

namespace GlashartEpg
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        static void Main()
        {
            try
            {
                var program = new Program();
                program.Run();
            }
            catch (Exception ex)
            {
                Logger.Fatal("An unhandled error occured", ex);
            }
        }

        private void Run()
        {
            Logger.Info("Weclome to Glashart EPG grabber");

        }
    }
}
