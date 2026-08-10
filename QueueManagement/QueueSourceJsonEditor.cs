using System.IO;
using System.Text.Json.Nodes;

namespace OneColumnEncoder.QueueManagement;

internal static class QueueSourceJsonEditor
{
    public static bool TryApplyFileOrder(string queueJsonPath, IReadOnlyList<string> orderedFilePaths)
    {
        if (string.IsNullOrWhiteSpace(queueJsonPath) || !File.Exists(queueJsonPath)) return false;

        try
        {
            JsonNode? root = JsonNode.Parse(File.ReadAllText(queueJsonPath));
            if (root is not JsonObject rootObject || rootObject["Entries"] is not JsonArray entries)
                return false;

            Dictionary<string, JsonNode> entriesByPath = new(StringComparer.OrdinalIgnoreCase);
            foreach (JsonNode? child in entries)
            {
                if (child is not JsonObject entry
                    || entry["FilePath"]?.GetValue<string>() is not string path
                    || string.IsNullOrWhiteSpace(path))
                    continue;
                entriesByPath[path] = child;
            }

            JsonArray reorderedEntries = [];
            foreach (string filePath in orderedFilePaths)
            {
                if (entriesByPath.TryGetValue(filePath, out JsonNode? entry))
                    reorderedEntries.Add(entry.DeepClone());
            }

            rootObject["Entries"] = reorderedEntries;
            File.WriteAllText(
                queueJsonPath,
                rootObject.ToJsonString(FFProbeJsonFormatting.Options),
                new UTF8Encoding(false));
            return true;
        }
        catch
        {
            return false;
        }
    }
}
