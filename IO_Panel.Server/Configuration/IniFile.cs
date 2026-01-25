using System.Collections.Generic;
using System.IO;

namespace IO_Panel.Server.Configuration;

public static class IniFile
{
    public static Dictionary<string, string> ReadKeyValues(string path)
    {
        var result = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(path))
        {
            return result;
        }

        string? section = null;

        foreach (var raw in File.ReadLines(path))
        {
            var line = raw.Trim();

            if (line.Length == 0 || line.StartsWith(";", System.StringComparison.Ordinal) || line.StartsWith("#", System.StringComparison.Ordinal))
            {
                continue;
            }

            if (line.StartsWith("[", System.StringComparison.Ordinal) && line.EndsWith("]", System.StringComparison.Ordinal))
            {
                section = line[1..^1].Trim();
                continue;
            }

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex <= 0)
            {
                continue;
            }

            var key = line[..equalsIndex].Trim();
            var value = line[(equalsIndex + 1)..].Trim();

            if (key.Length == 0)
            {
                continue;
            }

            var fullKey = string.IsNullOrWhiteSpace(section)
                ? key
                : $"{section}:{key}";

            result[fullKey] = value;
        }

        return result;
    }
}