namespace OneColumnEncoder.Models;

/// <summary>
/// Setting control type.
/// </summary>
public enum SettingControlType
{
    TextBox,
    CheckBox,
    Dropdown, // Language selection
    Font, // Font family selection
}

/// <summary>
/// Definition of one configurable setting field.
/// </summary>
public record SettingItemDefinitionM(
    string GroupName,
    string Label,
    SettingControlType ControlType,
    string PropertyName,
    int? MinValue = null,
    int? MaxValue = null,
    string[]? Options = null,
    Func<string, string>? DisplayNameResolver = null);
