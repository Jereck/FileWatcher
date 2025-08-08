namespace FileIntegrityWatcher.Models;

public class FileChangeEvent
{
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string ChangeType { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string? Sha256Hash { get; set; }
}