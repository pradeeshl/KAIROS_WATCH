KAIROS WATCH

Windows security monitoring tool that detects log tampering and automatically backs up security logs.

Overview

Kairos Watch is a .NET 9.0 console app that monitors Windows Security Event Logs in real-time. It ensures logs are preserved for forensic purposes.

Monitored Events:

1102 — Security audit log cleared

7036 — Critical service start/stop

Action: Automatically backs up logs and logs alerts on detection.

Features

Real-time Windows Event Log monitoring

Automatic timestamped log backups

Tamper detection for cleared security logs

Critical service monitoring

Persistent alert logging (KairosWatch_Alerts.txt)

Color-coded console output (red = critical, yellow = info)

Installation & Usage

Clone & build:

git clone <repo-url>
cd KAIROS_WATCH
dotnet build --configuration Release


Run as Administrator:

cd bin\Release\net9.0
KAIROS_WATCH.exe


Stop with Ctrl+C.

Output:

backups/ — timestamped .evtx log backups

KairosWatch_Alerts.txt — alert log

Technical Details

Platform: Windows 10/11 / Server

Architecture: Single-threaded, event-driven

Monitoring: EventLogWatcher

Backup: Windows wevtutil

Error Handling: Robust exception handling

Security Notes

Requires admin privileges

Preserves full log metadata in backups

Backup folder can be permission-restricted

Example Output
[INFO] Monitoring Windows Security Logs...
[ALERT] Event ID 1102 detected — Security log cleared!
[INFO] Backup created: backups\Security_20251003_112045.evtx