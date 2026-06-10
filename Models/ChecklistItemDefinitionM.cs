namespace OneColumnEncoder.Models
{
    public record ChecklistItemDefinitionM(string Text, StatusType InitialStatus = StatusType.Waiting);
}