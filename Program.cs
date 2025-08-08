
using FileIntegrityWatcher.Services;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true).AddEnvironmentVariables();

IConfiguration config = builder.Build();

string watchDir = Environment.GetEnvironmentVariable("WATCH_DIR") ?? config["WatchDirectory"] ?? "./watched";
string logFile = Environment.GetEnvironmentVariable("LOG_FILE") ?? config["LogFile"] ?? "./logs/audit-log.json";

var watcher = new IntegrityWatcher(watchDir, logFile);
watcher.Start();

Console.WriteLine("Type 'log' to show audit log or press Enter to exit.");
while (true)
{
    var input = Console.ReadLine();
    if (input?.ToLower() == "log")
    {
        watcher.PrintAuditLogTable();
    }
    else
    {
        break;
    }
}

await Task.Delay(-1);