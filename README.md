# 🕒 KAIROS WATCH

**Windows Security Log Guardian — Real-Time Monitoring, Backup & Tamper Detection**

## ⚡ Overview

Kairos Watch is a Windows security monitoring tool built with .NET 9.0, designed to detect log tampering and automatically back up critical security logs.

It continuously monitors Windows Security Event Logs in real time, ensuring that all crucial log data is preserved for forensic and audit purposes — even in cases of log clearing or service manipulation.

## 🔍 Monitored Events

| Event ID | Description | Action |
|----------|-------------|---------|
| 1102 | Security audit log cleared | 🔴 Backup triggered & alert logged |
| 7036 | Critical service start/stop | 🟡 Informational log recorded |

## 🧠 Key Features

- ✅ **Real-Time Monitoring** — Watches Windows Security Logs continuously
- 🕓 **Automatic Backups** — Creates timestamped .evtx backups upon detection
- 🧩 **Tamper Detection** — Detects when logs are cleared (Event ID 1102)
- 🛡️ **Critical Service Tracking** — Monitors key service start/stop events (Event ID 7036)
- 📝 **Persistent Alert Logging** — All alerts saved to KairosWatch_Alerts.txt
- 🎨 **Color-Coded Output** —
  - 🔴 Critical Alerts
  - 🟡 Informational Logs

## ⚙️ Installation & Usage

### 1️⃣ Clone the Repository
```bash
git clone https://github.com/pradeeshl/KAIROS_WATCH.git
cd KAIROS_WATCH
```

### 2️⃣ Build the Project
```bash
dotnet build --configuration Release
```

### 3️⃣ Run as Administrator
```cmd
cd bin\Release\net9.0
KAIROS_WATCH.exe
```

### 4️⃣ Stop Anytime
Press `Ctrl + C` to safely terminate monitoring.

## 🔐 Security Notes

- **Requires Administrator Privileges** — Necessary for accessing Windows Security Event Logs
- **Preserves full log metadata during backup** — Complete .evtx files are created
- **Folder Protection** — You can restrict permissions on the `backups/` folder for integrity protection
- **Ideal Use Cases** — Forensics, SOC monitoring, and incident response environments

## 🧾 Example Console Output

```
[INFO] Monitoring Windows Security Logs...
[ALERT] Event ID 1102 detected — Security log cleared!
[INFO] Backup created: backups\Security_20251003_112045.evtx
```

## 📁 Project Structure

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

## 🛠️ Requirements

- **.NET 9.0 Runtime**
- **Windows Operating System**
- **Administrator Privileges**

## 📜 License

This project is licensed under the MIT License — free to use, modify, and distribute with attribution.