using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using LogLevel = NLog.LogLevel;

namespace BiliLite.Services
{
    public class LogService
    {
        public static LoggingConfiguration config;
        public static Logger logger = LogManager.GetCurrentClassLogger();

        private static bool IsAutoClearLogFile => SettingService.GetValue<bool>(SettingConstants.Other.AUTO_CLEAR_LOG_FILE, true);
        private static int AutoClearLogFileDay => SettingService.GetValue<int>(SettingConstants.Other.AUTO_CLEAR_LOG_FILE_DAY, 7);
        private static bool IsProtectLogInfo => SettingService.GetValue<bool>(SettingConstants.Other.PROTECT_LOG_INFO, true);

        private static int LogLowestLevel => SettingService.GetValue(SettingConstants.Other.LOG_LEVEL, 1);

        public static ILoggerFactory Factory
        {
            get
            {
                return new CustomLoggerFactory();
            }
        }

        public static void Init()
        {
            config = new LoggingConfiguration();
            var storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var logfile = new NLog.Targets.FileTarget()
            {
                Name = "logfile",
                CreateDirs = true,
                FileName = storageFolder.Path + @"\log\" + DateTime.Now.ToString("yyyyMMdd") + ".log",
                Layout = "${longdate}" +
                         "|${level:uppercase=true}" +
                         "|${threadid}" +
                         "|${event-properties:item=type}.${event-properties:item=method}" +
                         "|${message}" +
                         "|${event-properties:item=exception}"
            };
            config.AddRule(LogLevel.Trace, LogLevel.Trace, logfile);
            config.AddRule(LogLevel.Debug, LogLevel.Debug, logfile);
            config.AddRule(LogLevel.Info, LogLevel.Info, logfile);
            config.AddRule(LogLevel.Warn, LogLevel.Warn, logfile);
            config.AddRule(LogLevel.Error, LogLevel.Error, logfile);
            config.AddRule(LogLevel.Fatal, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
        }

        public static async Task DeleteExpiredLogFile()
        {
            var storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            if (IsAutoClearLogFile)
            {
                await DeleteFile(storageFolder.Path + @"\log\");
            }
        }

        private static async Task DeleteFile(string path)
        {
            var pattern = "yyyyMMdd";
            var days = AutoClearLogFileDay;
            var folder = await StorageFolder.GetFolderFromPathAsync(path);

            var files = await folder.GetFilesAsync();

            foreach (var file in files)
            {
                var fileName = file.DisplayName;
                if (!DateTimeOffset.TryParseExact(fileName, pattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fileDate))
                {
                    continue;
                }
                if (fileDate < DateTimeOffset.Now.AddDays(-days))
                {
                    File.Delete(file.Path);
                }
            }
        }

        public static void Log(string message, LogType type, Exception ex = null, [CallerMemberName] string methodName = null, string typeName = "unknowType")
        {
            Debug.WriteLine("[" + LogType.Info.ToString() + "]" + message);
            if ((int)type < LogLowestLevel) return;
            if (IsProtectLogInfo)
                message = message.ProtectValues("access_key", "csrf", "access_token", "sign");

            var logEvent = new LogEventInfo(LogLevel.Info, null, message);

            var exception = "";
            if (ex != null && IsProtectLogInfo)
            {
                exception = ex.Message + "\n" + ex.StackTrace;
                exception = exception.ProtectValues("access_key", "csrf", "access_token", "sign");
                logEvent.Properties["exception"] = exception;
            }
            switch (type)
            {
                case LogType.Trace:
                    logEvent.Level = LogLevel.Trace;
                    break;
                case LogType.Debug:
                    logEvent.Level = LogLevel.Debug;
                    break;
                case LogType.Info:
                    logEvent.Level = LogLevel.Info;
                    break;
                case LogType.Warn:
                    logEvent.Level = LogLevel.Warn;
                    break;
                case LogType.Error:
                    logEvent.Level = LogLevel.Error;
                    break;
                case LogType.Fatal:
                    logEvent.Level = LogLevel.Fatal;
                    break;
                case LogType.Necessary:
                    logEvent.Level = LogLevel.Info;
                    break;
                default:
                    break;
            }

            logEvent.Properties["type"] = typeName;
            logEvent.Properties["method"] = methodName;
            logger.Log(logEvent);
        }
    }
}


public class CustomLoggerFactory : ILoggerFactory
{
    public void AddProvider(ILoggerProvider provider)
    {
    }

    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        return new CustomLogger();
    }

    public void Dispose()
    {
    }
}

public class CustomLogger : Microsoft.Extensions.Logging.ILogger
{
    private static readonly BiliLite.Services.ILogger _logger = GlobalLogger.FromCurrentType();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (formatter == null)
            throw new ArgumentNullException(nameof(formatter));

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message) && exception == null)
            return;

        // 映射到自定义日志级别
        switch (logLevel)
        {
            case Microsoft.Extensions.Logging.LogLevel.Trace:
                _logger.Trace(message, exception);
                break;
            case Microsoft.Extensions.Logging.LogLevel.Debug:
                _logger.Debug(message, exception);
                break;
            case Microsoft.Extensions.Logging.LogLevel.Information:
                _logger.Info(message, exception);
                break;
            case Microsoft.Extensions.Logging.LogLevel.Warning:
                _logger.Warn(message, exception);
                break;
            case Microsoft.Extensions.Logging.LogLevel.Error:
                _logger.Error(message, exception);
                break;
            case Microsoft.Extensions.Logging.LogLevel.Critical:
                _logger.Fatal(message, exception);
                break;
            case Microsoft.Extensions.Logging.LogLevel.None:
                // 不记录
                break;
            default:
                _logger.Log(message, LogType.Info, exception);
                break;

        }
    }

    private class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();

        public void Dispose()
        {
            // 空实现
        }
    }
}