using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace OneColumnEncoder.Helpers
{
    public class ValidationHelper
    {
        public static bool IsModernFtpSafe(string fileName)
        {
            foreach (char c in fileName)
            {
                if (char.IsControl(c)) return false;
                // emoji extension
                if (char.IsSurrogate(c)) return false;
                UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(c);
                // Combined
                if (cat == UnicodeCategory.NonSpacingMark) return false;
            }

            return true;
        }
    }
}
