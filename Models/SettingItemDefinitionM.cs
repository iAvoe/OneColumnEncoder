namespace OneColumnEncoder.Models;

public enum SettingControlType
{
    TextBox,
    CheckBox,
    Dropdown, // Language selection
    Font, // Font family selection
}
public record SettingItemDefinitionM(
    string GroupName,
    string Label,
    SettingControlType ControlType,
    string PropertyName,
    int? MinValue = null,
    int? MaxValue = null);
