using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Threading;

class Program
{
    // Configuration of the project
    private static readonly string BackupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");
    private static readonly string AlertFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KairosWatch_Alerts.txt");
    private static readonly int BackupTimeoutMs = 30_000; // 30s for wevtutil

    static void Main()
    {
        Console.WriteLine("KairosWatch — starting up...");

        // Ensure backup folder exists
        Directory.CreateDirectory(BackupDir);

        TrySecureBackupFolder(BackupDir);

        #if WINDOWS
                // Subscribe to Event ID 1102 (audit log cleared) and 7036 (service state changes)
                string query = "*[System[(EventID=1102) or (EventID=7036)]]";
                var eventsQuery = new EventLogQuery("Security", PathType.LogName, query);
        
                using (var watcher = new EventLogWatcher(eventsQuery))
                {
                    watcher.EventRecordWritten += EventLogEventRead;
                    watcher.Enabled = true;
        
                    Console.WriteLine("Monitoring Windows Event Logs (Security). Press Ctrl+C to exit.");
                    // Keep app alive
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
    
                // Build an alert summary
                string alert = $"[{time}] ALERT: EventID={id} Source={source} Message=\"{Shorten(message, 300)}\"";
    
                // If it's the tamper event (1102) or service stop, act
                if (id == 1102)
                {
                    WriteAlert(alert);
                    // Backup logs immediately
                    BackupSecurityLog();
                }
                else if (id == 7036)
                {
                    // optional: detect service stopped states in message text
                    if (message?.IndexOf("stopped", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        message?.IndexOf("stopping", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        WriteAlert(alert + " (service stopped detected)");
                        BackupSecurityLog();
                    }
                }
                else
                {
                    // For other subscribed events, just log them
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
        // Console (red)
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(text);
        Console.ResetColor();

        // Append to alert file
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

    private static void TrySecureBackupFolder(string folder)
    {
        // try
        // {
        //     // Grant full control to SYSTEM and Administrators, remove inheritance.
        //     // This requires elevated privileges.
        //     string args = $"\"{folder}\" /grant \"SYSTEM:(OI)(CI)F\" \"Administrators:(OI)(CI)F\" /inheritance:r";

        //     var psi = new ProcessStartInfo
        //     {
        //         FileName = "icacls",
        //         Arguments = args,
        //         CreateNoWindow = true,
        //         UseShellExecute = false,
        //         RedirectStandardOutput = true,
        //         RedirectStandardError = true
        //     };

        //     using (var proc = Process.Start(psi))
        //     {
        //         if (proc == null) return;
        //         proc.WaitForExit();
        //         if (proc.ExitCode != 0)
        //         {
        //             string err = proc.StandardError.ReadToEnd();
        //             LogToConsole($"icacls returned {proc.ExitCode}: {Shorten(err, 300)}");
        //         }
        //         else
        //         {
        //             LogToConsole("Backup folder ACL updated (SYSTEM + Administrators).");
        //         }
        //     }
        // }
        // catch (Exception ex)
        // {
        //     LogToConsole($"Could not secure backup folder: {ex.Message}");
        // }
        LogToConsole("Skipping ACL hardening (icacls disabled for safe run).");
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
