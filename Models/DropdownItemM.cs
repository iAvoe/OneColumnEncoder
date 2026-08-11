namespace OneColumnEncoder.Models;

/// <summary>
/// Dropdown menu item with separator and placeholder flags.
/// </summary>
public class DropdownItemM(string title, bool isSeparator = false, bool isPlaceholder = false)
{
    public string Title { get; set; } = title;
    public bool IsSeparator { get; set; } = isSeparator;
    public bool IsPlaceholder { get; set; } = isPlaceholder;
    public object? Tag { get; set; }
}
