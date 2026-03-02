# KAIROS WATCH

**Windows Security Log Guardian — Real-Time Monitoring, Backup & Tamper Detection**

> **PROOF OF CONCEPT**  
> This project is purely a proof of concept demonstration and it is intended only for the educational and testing purposes only. Not recommended for production use without further security hardening, testing, and validation.

---

## Overview

Kairos Watch is a Windows security monitoring tool built with .NET 9.0, designed to detect log tampering and automatically back up critical security logs.

It continuously monitors Windows Security Event Logs in real time, ensuring that all crucial log data is preserved for forensic and audit purposes — even in cases of log clearing or service manipulation.

## Monitored Events

| Event ID | Description | Action |
|----------|-------------|---------|
| 1102 | Security audit log cleared | Backup triggered & alert logged |
| 7036 | Critical service stopped | Backup triggered & alert logged |

## Key Features

- **Real-Time Monitoring** — Watches Windows Security Logs continuously
- **Automatic Backups** — Creates timestamped .evtx backups upon detection
- **Tamper Detection** — Detects when logs are cleared (Event ID 1102)
- **Critical Service Monitoring** — Monitors when critical services stop (Event ID 7036)
- **Persistent Alert Logging** — All alerts saved to KairosWatch_Alerts.txt
- **Color-Coded Console Output** —
  - Red: Critical alerts and backups
  - Yellow: Informational messages

## Installation & Usage

### 1. Clone the Repository
```bash
git clone https://github.com/pradeeshl/KAIROS_WATCH.git
cd KAIROS_WATCH
```

### 2. Build the Project
```bash
dotnet build --configuration Release
```

### 3. Run as Administrator
```cmd
cd KAIROS_WATCH\bin\Release\net9.0
KAIROS_WATCH.exe
```

> **Note:** Right-click and select "Run as Administrator" or use an elevated command prompt/PowerShell.

### 4. Stop Anytime
Press `Ctrl + C` to safely terminate monitoring.

## Security Considerations

- **Requires Administrator Privileges** — Necessary for accessing Windows Security Event Logs
- **Preserves full log metadata during backup** — Complete .evtx files are created
- **Folder Protection** — You can restrict permissions on the `backups/` folder for integrity protection
- **Proof of Concept Limitations** — This tool demonstrates core functionality but requires additional security hardening for production environments

### Ideal Use Cases

- Educational and learning purposes
- Forensics research and testing
- SOC monitoring proof of concept
- Incident response testing environments

---

## Example Console Output

```
KairosWatch — starting up...
Monitoring Windows Event Logs (Security). Press Ctrl+C to exit.
[2026-03-02T14:32:10.1234567Z] ALERT: EventID=1102 Source=Microsoft-Windows-Eventlog Message="The audit log was cleared..."
[BACKUP] Security log exported to C:\...\backups\Security_20260302_143210.evtx
```

---

## Technical Details

### System Requirements

- **.NET 9.0 Runtime**
- **Windows Operating System** (uses wevtutil for log backup)
- **Administrator Privileges** (required for Security event log access)

### Project Structure

```
KAIROS_WATCH/
├── KAIROS_WATCH.sln
├── README.md
└── KAIROS_WATCH/
    ├── KAIROS_WATCH.csproj
    ├── Program.cs
    └── bin/
        └── Debug/
            └── net9.0/
                └── backups/
```

---

## License

This project is licensed under the MIT License — free to use, modify, and distribute with attribution.