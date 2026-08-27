namespace OneColumnEncoder.Models;

/// <summary>
/// Tool zone grouping.
/// </summary>
public enum ToolZone { Upstream, Encoder, Analytics, Dependencies }

// For file size fingerprint values, see AppDataM.cs→Importables

/// <summary>
/// Display metadata for one tool entry.
/// </summary>
public record ToolDefinitionM(
    string DisplayName,
    string R1Text,
    string R2Text,
    string P1Name,
    string? P2Name = null,
    ToolZone? Zone = null,
    string? ExeName = null,
    string? Key = null
);
