using System.IO;
using OneColumnEncoder.Models;
using System.Text.RegularExpressions;

namespace OneColumnEncoder.Validation;

public partial class FilenameValidation
{
    private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

    public static bool IsValidLength(string filename, int max = 50)
        => filename.Length > 0 && filename.Length <= max;

    public static bool IsNotReservedName(string filename)
        => !RegexProvider.ReservedFilenamesRegex().IsMatch(filename);

    public static bool HasNoInvalidChars(string filename)
        => filename.Length >= 0
            && filename.IndexOfAny(InvalidFileNameChars) < 0
            && !filename.Any(char.IsControl)
            && !filename.Contains('&')
            && !filename.EndsWith('.')
            && !filename.EndsWith(' ');

    public static string ToCompatibleFileName(string filename, int maxLength = 50)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return "file";

        StringBuilder builder = new(filename.Length);
        foreach (char c in filename)
        {
            if (Array.IndexOf(InvalidFileNameChars, c) >= 0)
                continue;
            if (char.IsControl(c) || c == '&')
                continue;
            builder.Append(c);
        }

        string value = builder
            .ToString()
            .Trim()
            .TrimEnd('.', ' ');

        if (string.IsNullOrWhiteSpace(value))
            value = "file";

        if (!IsNotReservedName(value))
            value = "_" + value;

        if (value.Length > maxLength)
            value = value[..maxLength].TrimEnd('.', ' ');

        if (string.IsNullOrWhiteSpace(value))
            value = "file";

        if (!IsNotReservedName(value))
            value = "_" + value;

        return value;
    }

    // No CJK text over BMP, Emoji or other extended characters
    public static bool HasNoExtendedChars(string filename)
    {
        foreach (Rune r in filename.EnumerateRunes())
            if (r.Value > 0xFFFF) return false;
        return true;
    }

    public static bool HasSpaces(string filename)
        => filename.Contains(' ');

    public static bool HasNoSpecialSpaceVariants(string filename)
        => !filename.Any(c => char.IsWhiteSpace(c) && c != ' ');

    public static bool HasUnicodeCombiningMarks(string fileName)
        => !fileName.Any(IsUnicodeCombiningMark);

    private static bool IsUnicodeCombiningMark(char c)
        => CharUnicodeInfo.GetUnicodeCategory(c) is UnicodeCategory.NonSpacingMark
            or UnicodeCategory.SpacingCombiningMark
            or UnicodeCategory.EnclosingMark;
}
