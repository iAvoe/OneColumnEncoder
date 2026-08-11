namespace OneColumnEncoder.Models;

/// <summary>
/// One checklist entry and its initial status.
/// </summary>
public record ChecklistItemDefinitionM(string Text, StatusType InitialStatus = StatusType.Waiting);
