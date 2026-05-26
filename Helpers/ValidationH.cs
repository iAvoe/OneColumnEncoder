using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace OneColumnEncoder.Helpers
{
    public partial class ValidationH
    {
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        [GeneratedRegex(@"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\..*)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex ReservedFilenames();

        public static bool IsValidLength(string filename, int max = 50)
            => filename.Length > 0 && filename.Length <= max;

        public static bool IsNotReservedName(string filename)
            => !ReservedFilenames().IsMatch(filename);

        public static bool HasNoInvalidChars(string filename)
            => filename.Length >= 0
                && filename.IndexOfAny(InvalidFileNameChars) < 0
                && !filename.Any(char.IsControl)
                && !filename.Contains('&')
                && !filename.EndsWith('.')
                && !filename.EndsWith(' ');

        // No CJK text over BMP, Emoji or other extended characters
        public static bool HasNoExtendedChars(string filename)
        {
            foreach (Rune r in filename.EnumerateRunes())
            {
                if (r.Value > 0xFFFF) return false;
            }
            return true;
        }

        public static bool HasSpaces(string filename)
            => filename.Contains(' ');

        public static bool HasUnicodeCombiningMarks(string fileName)
            => !fileName.Any(IsUnicodeCombiningMark);

        private static bool IsUnicodeCombiningMark(char c)
            => CharUnicodeInfo.GetUnicodeCategory(c) is UnicodeCategory.NonSpacingMark
                or UnicodeCategory.SpacingCombiningMark
                or UnicodeCategory.EnclosingMark;
    }
}
