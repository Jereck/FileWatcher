using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using System.Timers;
using FileIntegrityWatcher.Models;
using Spectre.Console;
using Timer = System.Timers.Timer;

namespace FileIntegrityWatcher.Services;

public class IntegrityWatcher
{
    private readonly string _watchDir;
    private readonly string _logFile;
    private readonly List<FileChangeEvent> _events = new();

    private readonly ConcurrentDictionary<string, DateTime> _pendingFiles = new();
    private readonly Timer _debounceTimer;

    private FileSystemWatcher? _watcher;

    public IntegrityWatcher(string watchDir, string logFile)
    {
        _watchDir = Path.GetFullPath(watchDir);
        _logFile = Path.GetFullPath(logFile);
        Directory.CreateDirectory(Path.GetDirectoryName(_logFile)!);
        Directory.CreateDirectory(_watchDir);

        _debounceTimer = new Timer(500);
        _debounceTimer.Elapsed += OnDebounceTimerElapsed;
        _debounceTimer.AutoReset = true;
    }

    public void Start()
    {
        _watcher = new FileSystemWatcher(_watchDir)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
        };

        _watcher.Created += OnFileEvent;
        _watcher.Changed += OnFileEvent;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;

        Console.WriteLine($"[Watcher] Monitoring {_watchDir}... Press Ctrl+C to exit");
        _debounceTimer.Start();
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        _pendingFiles[e.FullPath] = DateTime.UtcNow;
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        LogEvent("Deleted", e.FullPath, null);
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        LogEvent("Renamed (from)", e.OldFullPath, null);

        try
        {
            string? hash = File.Exists(e.FullPath) ? ComputeSha256WithRetry(e.FullPath) : null;
            LogEvent("Renamed (to)", e.FullPath, hash);
        }
        catch { }
    }

    private void OnDebounceTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        var now = DateTime.UtcNow;
        var debounceMs = 1000;

        foreach (var file in _pendingFiles.Keys.ToList())
        {
            if (_pendingFiles.TryGetValue(file, out var lastEventTime))
            {
                if ((now - lastEventTime).TotalMilliseconds > debounceMs)
                {
                    _pendingFiles.TryRemove(file, out _);
                    ProcessFileChange(file);
                }
            }
        }
    }

    private void ProcessFileChange(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        string? hash = ComputeSha256WithRetry(filePath);

        if (hash != null)
        {
            LogEvent("Changed", filePath, hash);
        }
        else
        {
            Console.WriteLine($"[Error] Could not process {filePath} after retries.");
        }
    }

    private void LogEvent(string changeType, string path, string? hash)
    {
        var evt = new FileChangeEvent
        {
            ChangeType = changeType,
            FilePath = path,
            Sha256Hash = hash
        };

        _events.Add(evt);

        var timeStr = evt.TimestampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        var hashStr = hash != null ? $"[green]{hash}[/]" : "";

        AnsiConsole.MarkupLine($"{timeStr} [yellow]{changeType}[/] -> [blue]{Path.GetFileName(path)}[/] {hashStr}");

        // Console.WriteLine($"[{evt.TimestampUtc}] {changeType} -> {path} {(hash != null ? $"(SHA256: {hash})" : "")}");
        File.WriteAllText(_logFile, JsonSerializer.Serialize(_events, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static string ComputeSha256(string filePath)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private string? ComputeSha256WithRetry(string filePath, int retries = 5, int delayMs = 200)
    {
        for (int i = 0; i < retries; i++)
        {
            try
            {
                return ComputeSha256(filePath);
            }
            catch (IOException)
            {
                Thread.Sleep(delayMs);
            }
        }
        return null;
    }

    public void PrintAuditLogTable()
    {
        var table = new Table();
        table.AddColumn("Timestamp");
        table.AddColumn("Change Type");
        table.AddColumn("File");
        table.AddColumn("SHA256");

        foreach (var evt in _events)
        {
            table.AddRow(
                evt.TimestampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                evt.ChangeType,
                Path.GetFileName(evt.FilePath),
                evt.Sha256Hash ?? "-"
            );
        }

        AnsiConsole.Write(table);
    }
}