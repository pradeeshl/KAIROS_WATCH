using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Threading;

class Program
{
    private static readonly string BackupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");    private static readonly string AlertFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KairosWatch_Alerts.txt");
    private static readonly int BackupTimeoutMs = 30_000;
    static void Main()
    {
        Console.WriteLine("KairosWatch — starting up...");
        Directory.CreateDirectory(BackupDir);

        TrySecureBackupFolder(BackupDir);

        #if WINDOWS
                string query = "*[System[(EventID=1102) or (EventID=7036)]]";
                var eventsQuery = new EventLogQuery("Security", PathType.LogName, query);
        
                using (var watcher = new EventLogWatcher(eventsQuery))
                {
                    watcher.EventRecordWritten += EventLogEventRead;
                    watcher.Enabled = true;
                    Console.WriteLine("Monitoring Windows Event Logs (Security). Press Ctrl+C to exit.");
                    var exitEvent = new ManualResetEvent(false);
                    Console.CancelKeyPress += (s, e) =>
                    {
                        Console.WriteLine("Shutting down...");
                        e.Cancel = true;
                        exitEvent.Set();
                    };
                    exitEvent.WaitOne();
                }
        #else
                Console.WriteLine("This application can only run on Windows.");
        #endif
    }

        private static void EventLogEventRead(object sender, EventRecordWrittenEventArgs e)
        {
    #if WINDOWS
            try
            {
                if (e.EventRecord == null)
                {
                    LogToConsole($"Received null event record. Exception: {e.EventException?.Message}");
                    return;
                }
                int id = e.EventRecord.Id;
                string time = DateTime.UtcNow.ToString("o");
                string source = e.EventRecord.ProviderName ?? "Unknown";
                string message = TrySafeFormat(e.EventRecord);
                string alert = $"[{time}] ALERT: EventID={id} Source={source} Message=\"{Shorten(message, 300)}\"";
                if (id == 1102)
                {
                    WriteAlert(alert);
                    BackupSecurityLog();
                }
                else if (id == 7036)
                {
                    if (message?.IndexOf("stopped", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        message?.IndexOf("stopping", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        WriteAlert(alert + " (service stopped detected)");
                        BackupSecurityLog();
                    }
                }
                else
                {
                    WriteAlert(alert);
                }
            }
            catch (Exception ex)
            {
                LogToConsole($"Exception in Event handler: {ex.Message}");
            }
    #else
            LogToConsole("EventLogEventRead called on non-Windows platform.");
    #endif
        }

    private static void WriteAlert(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(text);
        Console.ResetColor();
        try
        {
            File.AppendAllText(AlertFile, text + Environment.NewLine);
        }
        catch (Exception ex)
        {
            LogToConsole($"Failed to write alert file: {ex.Message}");
        }
    }

    private static void BackupSecurityLog()
    {
        try
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            string outPath = Path.Combine(BackupDir, $"Security_{timestamp}.evtx");
            var psi = new ProcessStartInfo
            {
                FileName = "wevtutil",
                Arguments = $"epl Security \"{outPath}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var proc = Process.Start(psi))
            {
                if (proc == null)
                {
                    LogToConsole("Failed to start wevtutil.");
                    return;
                }

                if (!proc.WaitForExit(BackupTimeoutMs))
                {
                    LogToConsole("wevtutil timed out.");
                    try { proc.Kill(); } catch { }
                    return;
                }

                string stdOut = proc.StandardOutput.ReadToEnd();
                string stdErr = proc.StandardError.ReadToEnd();

                if (proc.ExitCode == 0)
                {
                    WriteAlert($"[BACKUP] Security log exported to {outPath}");
                }
                else
                {
                    LogToConsole($"wevtutil failed (exit {proc.ExitCode}): {Shorten(stdErr, 300)}");
                }
            }
        }
        catch (Exception ex)
        {
            LogToConsole($"Exception during backup: {ex.Message}");
        }
    }
    private static void TrySecureBackupFolder(string path)
    {
        if (Directory.Exists(path))
        {
            LogToConsole($"Backup folder ready: {path}");
        }
    }
    private static void LogToConsole(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(msg);
        Console.ResetColor();
    }
        private static string TrySafeFormat(EventRecord rec)
        {
    #if WINDOWS
            try
            {
                return rec.FormatDescription() ?? "";
            }
            catch
            {
                return "";
            }
    #else
            return "";
    #endif
        }

    private static string Shorten(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Length <= max ? s : string.Concat(s.AsSpan(0, max), "...");
    }
}
