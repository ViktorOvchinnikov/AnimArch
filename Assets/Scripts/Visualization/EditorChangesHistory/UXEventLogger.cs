using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;

namespace EditorChangesHistory
{
    public static class UXEventLogger
    {
        private const string DefaultLogFileName = "ux_events.jsonl";
        private static readonly object WriteLock = new object();
        private static readonly Stopwatch ProgramStopwatch = Stopwatch.StartNew();

        private static string _logFilePath;

        private sealed class LogEntry
        {
            public string EventName { get; set; }
            public string TimestampUtc { get; set; }
            public double SecondsSinceStartup { get; set; }
            public string SourceFile { get; set; }
            public string SourceMember { get; set; }
            public int SourceLine { get; set; }
            public object Payload { get; set; }
        }

        public static string LogFilePath
        {
            get
            {
                EnsureLogPath();
                return _logFilePath;
            }
            set
            {
                _logFilePath = string.IsNullOrWhiteSpace(value) ? GetDefaultLogFilePath() : value;
                EnsureDirectoryExists(_logFilePath);
            }
        }

        public static void SetLogFilePath(string filePath)
        {
            LogFilePath = filePath;
        }

        public static void DebugLog(
            string eventName,
            object payload = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            try
            {
                EnsureLogPath();

                var entry = new LogEntry
                {
                    EventName = string.IsNullOrWhiteSpace(eventName) ? "unknown_event" : eventName,
                    TimestampUtc = DateTime.UtcNow.ToString("o"),
                    SecondsSinceStartup = ProgramStopwatch.Elapsed.TotalSeconds,
                    SourceFile = Path.GetFileName(callerFilePath),
                    SourceMember = callerMemberName,
                    SourceLine = callerLineNumber,
                    Payload = payload
                };

                string line = JsonConvert.SerializeObject(entry, Formatting.None);

                lock (WriteLock)
                {
                    using (var stream = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(line);
                        writer.Flush();
                        stream.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                UnityDebug.LogError($"UXEventLogger write failed: {ex}");
            }
        }

        private static void EnsureLogPath()
        {
            if (!string.IsNullOrWhiteSpace(_logFilePath))
            {
                EnsureDirectoryExists(_logFilePath);
                return;
            }

            _logFilePath = GetDefaultLogFilePath();
            EnsureDirectoryExists(_logFilePath);
        }

        private static string GetDefaultLogFilePath()
        {
            string executableDirectory = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrWhiteSpace(executableDirectory))
            {
                executableDirectory = Application.persistentDataPath;
            }

            return Path.Combine(executableDirectory, DefaultLogFileName);
        }

        private static void EnsureDirectoryExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
