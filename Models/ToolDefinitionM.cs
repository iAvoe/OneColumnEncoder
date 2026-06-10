namespace OneColumnEncoder.Models;

public enum ToolZone { Upstream, Encoder, Analytics, Dependencies }

// For file size fingerprint values, see AppDataM.cs→Importables

// For SrcImportZone / EncSettingsZone items that have unique per-item labels
public record ToolDefinitionM(
    string DisplayName,
    string R1Text,
    string R2Text,
    string P1Name,
    string? P2Name = null,
    ToolZone? Zone = null,
    string? ExeName = null
);