using System;
using System.Diagnostics;
using System.Linq;

namespace GitExtUtils.FileLogging
{
    public interface ILogFormatter
    {
        string Format(LogEntry entry);
    }

    public class GitCommandLogFormatter : ILogFormatter
    {
        public string Format(LogEntry entry)
        {
            try
            {
                entry.ExtraInfo.TryGetValue(Logger.MetaTypeKeyName, out var metatype);

                if (metatype == "GitExeCommand")
                {
                    var stack = entry.ExtraInfo["CallStack"];
                    var stackFormatted = stack == null ? "" : $" stack: {stack}";

                    return $"{entry.Time:yyyy-MM-dd HH:mm:ss} {entry.Severity,-5} {entry.ExtraInfo["Duration"],9} {entry.ExtraInfo["ProcessId"],6} {entry.ExtraInfo["IsOnMainThread"],2} {entry.ExtraInfo["ExitCode"],4} {entry.Message} {entry.ExtraInfo["Arguments"]} {entry.ExtraInfo["WorkingDir"]}{stackFormatted} metatype: {metatype}";
                }
                else
                {
                    var extra = entry.ExtraInfo
                        .Where(x => x.Key != Logger.MetaTypeKeyName)
                        .OrderBy(x => x.Key)
                        .Select(x => $"{x.Key}: {x.Value}");

                    return $"{entry.Time:yyyy-MM-dd HH:mm:ss} {entry.Severity,-5}                          {entry.Message} {string.Join(" ", extra)} metatype: {metatype}";
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
    }
}