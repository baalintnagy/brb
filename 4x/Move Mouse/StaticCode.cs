using ellabi.Schedules;
using ellabi.Wrappers;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Storage;

namespace ellabi
{
    public static class StaticCode
    {
        public delegate void ScheduleArrivedHandler(ScheduleBase.ScheduleAction action);
        //public delegate void ThemeUpdatedHandler(Theme theme);
        public delegate void UpdateAvailablityChangedHandler(bool updateAvailable);
        public delegate void RefreshSchedulesHandler();

        public static event ScheduleArrivedHandler ScheduleArrived;
        //public static event ThemeUpdatedHandler ThemeUpdated;
        public static event UpdateAvailablityChangedHandler UpdateAvailablityChanged;
        public static event RefreshSchedulesHandler RefreshSchedules;

        public const string PayPalUrl = "https://brb.no";
        public const string HomePageUrl = "https://brb.no";
        public const string HelpPageUrl = "https://brb.no";
        public const string TwitterUrl = "https://brb.no";
        public const string GitHubUrl = "https://brb.no";
        public const string CronHelpUrl = "https://brb.no";
        public const string TroubleshootingUrl = "https://brb.no";
        //public const string CronHelpUrl = "https://brb.no";
        //public const string ThemesXmlUrl = "https://brb.no";
        //public const string ThemesXmlUrl = "https://brb.no";
        public const string UpdateXmlUrl = "https://brb.no";
        public const string MailAddress = "brb@brb.no";
        public const string RunRegistryValueName = "Move Mouse";

        public static string WorkingDirectory = Path.Combine(Environment.ExpandEnvironmentVariables("%AppData%"), @"BRB\BRB");
        public static string TempDirectory = Path.Combine(Environment.ExpandEnvironmentVariables("%Temp%"), @"BRB\BRB");
        public static BrbSource DownloadSource = BrbSource.GitHub;

        public static string UpdateUrl;
        public static string ContactMailToAddress = $"mailto:{MailAddress}?subject=BRB%20Feedback";
        public static ILogger Logger;

        private static string _logPath;
        private static LoggingLevelSwitch _loggingLevelSwitch = new LoggingLevelSwitch();
     
        public enum BrbSource
        {
            MicrosoftStore,
            GitHub
        }

        public static string SettingsXmlPath => Path.Combine(WorkingDirectory, "Settings.xml");

        public static string LogPath => _logPath;

        public static void CreateLog()
        {
            try
            {
                _loggingLevelSwitch.MinimumLevel = (LogEventLevel)1 + (int)LogEventLevel.Fatal;
                _logPath = Path.Combine(StaticCode.DownloadSource == BrbSource.MicrosoftStore ? ApplicationData.Current.LocalFolder.Path : TempDirectory, "BRB.log");

                if (File.Exists(_logPath))
                {
                    try
                    {
                        File.Delete(_logPath);
                    }
                    catch (Exception)
                    {
                    }
                }

                var logConfiguration = new LoggerConfiguration()
                                        .MinimumLevel.ControlledBy(_loggingLevelSwitch)
                                        .WriteTo.File(_logPath,
                                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}\t[{Level:u3}]\t{MemberName}\t{Message}{NewLine}{Exception}");
                Logger = logConfiguration.CreateLogger();
            }
            catch (Exception ex)
            {
                Logger?.Here().Error(ex.Message);
            }
        }

        public static void EnableLog(LogEventLevel minimumLevel)
        {
            try
            {
                _loggingLevelSwitch.MinimumLevel = minimumLevel;
                Logger?.Here().Debug(LogPath);
            }
            catch (Exception ex)
            {
                Logger?.Here().Error(ex.Message);
            }
        }

        public static void DisableLog()
        {
            Logger?.Here().Debug(String.Empty);

            try
            {
                _loggingLevelSwitch.MinimumLevel = (LogEventLevel)1 + (int)LogEventLevel.Fatal;
            }
            catch (Exception ex)
            {
                Logger?.Here().Error(ex.Message);
            }
        }

        public static TimeSpan GetLastInputTime()
        {
            int idleTime = 0;

            try
            {
                var lastInputInfo = new NativeMethods.LASTINPUTINFO();
                lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
                lastInputInfo.dwTime = 0;

                if (NativeMethods.GetLastInputInfo(ref lastInputInfo))
                {
                    int lastInputTick = lastInputInfo.dwTime;
                    idleTime = Environment.TickCount - lastInputTick;
                }
            }
            catch (Exception ex)
            {
                Logger?.Here().Error(ex.Message);
            }

            return TimeSpan.FromMilliseconds(idleTime);
        }

        public static void OnScheduleArrived(ScheduleBase.ScheduleAction action)
        {
            Logger?.Here().Debug(action.ToString());
            ScheduleArrived?.Invoke(action);
        }

        //public static void OnThemeUpdated(Theme theme)
        //{
        //    ThemeUpdated?.Invoke(theme);
        //}

        public static void OnUpdateAvailablityChanged(bool updateAvailable)
        {
            UpdateAvailablityChanged?.Invoke(updateAvailable);
        }

        public static void OnRefreshSchedules()
        {
            RefreshSchedules?.Invoke();
        }
    }
}

public static class LoggerExtensions
{
    public static ILogger Here(this ILogger logger,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        return logger
            .ForContext("MemberName", memberName)
            .ForContext("FilePath", sourceFilePath)
            .ForContext("LineNumber", sourceLineNumber);
    }
}