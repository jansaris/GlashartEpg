using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace GlashartEpg
{
    public static class LogSetup
    {
        public static void Setup(string level = "Info")
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var logLevel = GetLogLevel(level);

            var patternLayout = new PatternLayout
            {
                ConversionPattern = "%date %-5level %logger - %message%newline"
            };
            patternLayout.ActivateOptions();
            var consoleLayout = new PatternLayout
            {
                ConversionPattern = "%message%newline"
            };
            patternLayout.ActivateOptions();

            var roller = new RollingFileAppender
            {
                AppendToFile = true,
                File = @"GlashartEpg.log",
                Layout = patternLayout,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "100MB",
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true
            };
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            var console = new ConsoleAppender
            {
                Layout = consoleLayout,
                Name = "Console",
                Target = "Console.Out",
            };
            console.ActivateOptions();
            hierarchy.Root.AddAppender(console);

            hierarchy.Root.Level = logLevel;
            hierarchy.Configured = true;
        }

        private static Level GetLogLevel(string level)
        {
            if (string.IsNullOrWhiteSpace(level)) return Level.Info;
            var collection = new LevelCollection();
            foreach (var item in collection)
            {
                if (item.Name.ToLower().Equals(level.ToLower()))
                {
                    return item;
                }
            }
            return Level.Info;
        }
    }
}