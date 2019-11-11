using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using NLog;
using SimpleLyricsMaker.Logs.Models;

namespace SimpleLyricsMaker.Logs
{
    public static class LoggerService
    {
        private const string LogFilePath = "ms-appdata:///local/logs/main.log";

        private static readonly Dictionary<LoggerMembers, Logger> AllLoggers = new Dictionary<LoggerMembers, Logger>();

        static LoggerService()
        {
            LogManager.Configuration.Variables["localFolderPath"] = ApplicationData.Current.LocalFolder.Path;
            LogManager.Configuration.Variables["tempFolderPath"] = ApplicationData.Current.TemporaryFolder.Path;
        }

        public static Logger GetLogger(LoggerMembers member)
        {
            if (!AllLoggers.ContainsKey(member))
                AllLoggers[member] = LogManager.GetLogger(member.ToString());

            return AllLoggers[member];
        }

        public static async Task<string> ReadLogs(int maxLines = 30)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(LogFilePath));

            var lines = await FileIO.ReadLinesAsync(file);

            return String.Join("\r\n", lines.Take(maxLines));
        }
    }
}
