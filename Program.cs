
using FileIntegrityWatcher.Services;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();

string watchDir = config["WatchDirectory"] ?? "./watched";
string logFile = config["LogFile"] ?? "./logs/audit-log.json";

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