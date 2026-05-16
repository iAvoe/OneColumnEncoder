using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Stores
{
    public class ChecklistProviderS
    {
        public static List<ChecklistItemDefinitionM> GetToolsChecklist() =>
        [
            new("One upsream program available", StatusType.Error),
            new("One downstream program available", StatusType.Error),
            new("One analysis program available", StatusType.Error),
        ];

        public static List<ChecklistItemDefinitionM> GetSourceChecklist1() =>
        [
            new("Metadata and SEI data are readable"),
            new("Progressive video frame / not interlated (SVT-AV1 req.)"),
            new("Bit-depth is less than 12 (8 or 10, SVT-AV1 req.)"),
        ];

        public static List<ChecklistItemDefinitionM> GetSourceChecklist2() =>
        [
            new("Framerate is constant / not variable"),
            new("Square pixel aspect ratio / 1:1 sar"),
            new("Color matrix matadata is normal"),
            new("Transfer characteristics matadata is normal"),
            new("Color primaries metadata is normal"),
            new("No chroma subsampling or being ←/↖ (SVT-AV1 req.)"),
        ];

        public static List<ChecklistItemDefinitionM> GetEncodeChecklist1() =>
        [
            new("Using power adapter instead of battery"),
            new("Sufficient RAM availability"),
            new("Sufficient disk space availability"),
            new("Avoiding slow disk connection (USB2, Bluetooth, etc.)"),
            new("Avoiding disk thrashing (R&W on the same HDD)"),
        ];

        public static List<ChecklistItemDefinitionM> GetEncodeChecklist2() =>
        [
            new("Output filename is valid for OS"),
            new("Output filename maybe valid for FTP (Pseudo-UTF-8)"),
            new("Write permission in output folder"),
            new("Output doesn't overwrite existing file (or delete manually)"),
        ];
    }
}