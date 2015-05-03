using System;
using System.IO;
using log4net;

namespace GlashartEpg
{
    public class Configuration : IConfiguration
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Configuration));
        public string DataFolder { get; private set; }
        public string EpgUrl { get; private set; }
        public int Days { get; private set; }

        public void Load()
        {
            Logger.Info("Read GlashartEpg.ini");
            try
            {
                using (var reader = new StreamReader("GlashartEpg.ini"))
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if(string.IsNullOrWhiteSpace(line)) continue;
                    Logger.DebugFormat("Process line {0}", line);
                    ReadConfigItem(line);
                }
                
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load the configuration", ex);
            }
        }

        private void ReadConfigItem(string line)
        {
            var keyvalue = line.Split('=');
            if (keyvalue.Length < 2)
            {
                Logger.WarnFormat("Failed to read configuration line: {0}", line);
                return;
            }
            SetValue(keyvalue[0], keyvalue[1]);
        }

        private void SetValue(string key, string value)
        {
            switch (key)
            {
                case "EpgUrl":
                    EpgUrl = value;
                    break;
                case "DataFolder":
                    DataFolder = value;
                    break;
                case "Days":
                    Days = int.Parse(value);
                    break;
                default:
                    Logger.WarnFormat("Unknown configuration key: {0}", key);
                    return;
            }
            Logger.DebugFormat("Read configuration item {0} with value {1}", key, value);
        }
    }
}