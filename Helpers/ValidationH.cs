using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OneColumnEncoder.Helpers
{
    public partial class ValidationH
    {
        [GeneratedRegex(@"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\..*)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex ReservedFilenames();

        public static bool IsValidLength(string filename, int max = 50)
            => filename.Length <= max;

        public static bool IsNotReservedName(string filename)
            => !ReservedFilenames().IsMatch(filename);

        public static bool HasNoInvalidChars(string filename)
            => filename.Length > 0
                && filename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
                && !filename.Contains('&')
                && !filename.EndsWith('.')
                && !filename.EndsWith(' ');

        public static bool HasNoExtendedChars(string filename)
            => !filename.Any(char.IsSurrogate) && filename.All(c => c <= 0x7f);

        public static bool HasSpaces(string filename)
            => filename.Contains(' ');

        public static bool IsModernFtpSafe(string fileName)
        {
            foreach (char c in fileName)
            {
                if (char.IsControl(c)) return false;
                if (char.IsSurrogate(c)) return false;
                UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat == UnicodeCategory.NonSpacingMark) return false;
            }

            return true;
        }
    }
}