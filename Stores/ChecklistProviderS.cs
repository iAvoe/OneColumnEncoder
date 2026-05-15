using OneColumnEncoder.CommonMethods;
using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Stores
{
    class ChecklistProviderS
    {
        public static List<ChecklistItemDefinitionM> GetToolsChecklist() =>
        [
            new("One upsream program available", StatusType.Error),
            new("One downstream program available", StatusType.Error),
            new("One analysis program available", StatusType.Error),
        ];

        public static List<ChecklistItemDefinitionM> GetSourceChecklist1() =>
        [
            new("Metadata is readable"),
            new("Progressive video frame / not interlated (SVT-AV1 req.)"),
            new("Bit-depth is less than 12 (SVT-AV1 req.)"),
        ];

        public static List<ChecklistItemDefinitionM> GetSourceChecklist2() =>
        [
            new("Framerate is constant / not variable"),
            new("Square pixel aspect ratio / 1:1 sar"),
            new("Color matrix matadata is normal"),
            new("Transfer characteristics matadata is normal"),
            new("Color primaries metadata is normal"),
            new("←/↖ chroma sample location (SVT-AV1 req.)"),
        ];

        public static List<ChecklistItemDefinitionM> GetEncodeChecklist1() =>
        [
            new("Using power adapter or battery level above 30%"),
            new("Sufficient available RAM"),
            new("Sufficient available disk space"),
        ];

        public static List<ChecklistItemDefinitionM> GetEncodeChecklist2() =>
        [
            new("Output filename is valid for OS"),
            new("Output filename is valid for FTP (optional)"),
            new("Output format is compatible with the downstream program"),
            new("Output file does not overwrite existing files"),
        ];
    }
}
