using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Models
{
    public enum SettingControlType
    {
        TextBox,
        CheckBox,
        PasswordBox, // For SMTP password
        Dropdown, // Language selection
    }
    public record SettingItemDefinitionM(
        string GroupName,
        string Label,
        SettingControlType ControlType,
        string PropertyName);
}