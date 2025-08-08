# File Integrity Watcher

A lightweight C# console application that monitors a directory for file changes (create, modify, delete, rename), computes SHA256 hashes for modified files, and logs events both to the console and a JSON audit log.

---

## Features

- Watches any folder (including subdirectories) for file system changes
- Debounces rapid changes to avoid duplicate events
- Retries file hashing to avoid conflicts with files locked by other processes
- Logs detailed event info including timestamps, change type, file paths, and SHA256 hashes
- Colorful console output powered by [Spectre.Console](https://spectreconsole.net/)
- Export audit logs to JSON file for later analysis
- Easy configuration via `appsettings.json`

---

## Why This Matters

File integrity monitoring is a fundamental security control used to detect unauthorized changes to critical files, which can be an early indicator of malware, ransomware, or insider threats.

This project is designed to showcase:

- Core C# and .NET skills
- Robust file system event handling and error management
- Use of modern libraries for polished CLI output
- Concepts important in endpoint security, aligning with ThreatLocker’s mission
- Foundation for DevOps practices like automation, monitoring, and CI/CD

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Optional: [Spectre.Console](https://spectreconsole.net/) (already included via NuGet)

### Installation

1. Clone the repo or download the source code
2. Create and configure `appsettings.json` (example below)
3. Build and run the app

### Sample `appsettings.json`

```json
{
  "WatchDirectory": "./watched",
  "LogFile": "./logs/audit-log.json"
}
```

### Usage

```bash
dotnet run
```

The watcher will:
- Create the watch directory if it doesn't exist
- Start monitoring for file changes
- Log events to the console with colors and to the JSON audit logs

Try adding, modifying, renaming, or deleting files in the watch directory and see live updates!

### Commands
- Type ```log``` + Enter to display a formatted audit log table in the console
- Press Enter without typing anything to exit

---

### Next steps
- [ ] Containerize with Docker for easy deployment
- [ ] Integrate CI/CD pipelines (Github Actions) for automated testing and deployment
- [ ] Add env var support for 12-factor app compliance [12 Factor](https://12factor.net/)
- [ ] Push logs to centralized monitoring tools
- [ ] Implement alerting on suspicious file changes
- [ ] Write Unit tests

---

### License
MIT License © [Jake Reck]