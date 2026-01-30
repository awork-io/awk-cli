using System.Text;

namespace Awk.Config;

internal static class EnvFile
{
    internal static Dictionary<string, string> Load(string path)
    {
        var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path)) return data;

        foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;
            if (trimmed.StartsWith('#')) continue;

            var normalized = trimmed.StartsWith("export ", StringComparison.OrdinalIgnoreCase)
                ? trimmed[7..].Trim()
                : trimmed;

            var idx = normalized.IndexOf('=');
            if (idx <= 0) continue;

            var key = normalized[..idx].Trim();
            var value = normalized[(idx + 1)..].Trim();

            if (value.Length >= 2 && ((value.StartsWith('"') && value.EndsWith('"')) || (value.StartsWith('\'') && value.EndsWith('\''))))
            {
                value = value[1..^1];
            }

            if (string.IsNullOrWhiteSpace(key)) continue;
            data[key] = value;
        }

        return data;
    }
}
