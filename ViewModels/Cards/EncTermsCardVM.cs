using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using System;

namespace OneColumnEncoder.ViewModels.Cards
{
    public class EncTermsCardVM : ValidationCardBaseVM
    {
        private const int OffGridChecklistIdx = 0;
        private const int DiskSpaceChecklistIdx = 1;

        private const int WritePermissionChecklistIdx = 0;
        private const int OverwriteChecklistIdx = 1;
        private const int LsmashChecklistIdx = 2;

        private const double DiskSpaceSafetyMultiplier = 1.5;
        private const long FallbackMinDiskBytes = 1L * 1024 * 1024 * 1024;

        public Func<string>? GetOutputDirectoryFunc { get; set; }
        public Func<string>? GetOutputFilePathFunc { get; set; }
        public Func<bool>? IsAvs2yuvSelectedFunc { get; set; }
        public Func<string>? GetAviSynthDllPathFunc { get; set; }
        public Func<string>? GetSourceVideoFilePathFunc { get; set; }

        public EncTermsCardVM()
        {
            FillCollection(Checklist1, ChecklistProviderM.GetEncodeChecklist1());
            FillCollection(Checklist2, ChecklistProviderM.GetEncodeChecklist2());
        }

        public void RunAllChecks()
        {
            RunChecklist1Checks();
            RunChecklist2Checks();
        }

        #region Checklist1: Hardware checks

        private void RunChecklist1Checks()
        {
            SetChecklist1(OffGridChecklistIdx,
                EncTermsCheckH.IsOnAcPower()
                    ? StatusType.Success
                    : StatusType.Warning);

            string? outputDir = GetOutputDirectoryFunc?.Invoke();
            string? sourcePath = GetSourceVideoFilePathFunc?.Invoke();
            SetChecklist1(DiskSpaceChecklistIdx, EvaluateDiskSpace(outputDir, sourcePath));
        }

        private static StatusType EvaluateDiskSpace(string? outputDir, string? sourcePath)
        {
            long availBytes = EncTermsCheckH.GetAvailableDiskSpaceBytes(outputDir);
            if (availBytes < 0) return StatusType.Waiting;

            long requiredBytes;
            long sourceSize = EncTermsCheckH.GetSourceVideoFileSize(sourcePath);

            if (sourceSize > 0)
                requiredBytes = (long)(sourceSize * DiskSpaceSafetyMultiplier);
            else
                requiredBytes = FallbackMinDiskBytes;

            return availBytes >= requiredBytes
                ? StatusType.Success
                : StatusType.Error;
        }

        #endregion

        #region Checklist2: Software checks

        private void RunChecklist2Checks()
        {
            string? outputDir = GetOutputDirectoryFunc?.Invoke();
            SetChecklist2(WritePermissionChecklistIdx,
                EncTermsCheckH.HasWritePermission(outputDir)
                    ? StatusType.Success
                    : StatusType.Error);

            string? outputPath = GetOutputFilePathFunc?.Invoke();
            SetChecklist2(OverwriteChecklistIdx,
                EncTermsCheckH.OutputFileExists(outputPath)
                    ? StatusType.Warning
                    : StatusType.Success);

            bool isAvs2yuv = IsAvs2yuvSelectedFunc?.Invoke() ?? false;
            string? avisynthPath = GetAviSynthDllPathFunc?.Invoke();
            SetChecklist2(LsmashChecklistIdx,
                !isAvs2yuv
                    ? StatusType.Success
                    : EncTermsCheckH.HasLsmashPlugin(avisynthPath)
                        ? StatusType.Success
                        : StatusType.Error);
        }

        #endregion

        public void RefreshLanguage()
        {
            RefreshChecklist(Checklist1, ChecklistProviderM.GetEncodeChecklist1());
            RefreshChecklist(Checklist2, ChecklistProviderM.GetEncodeChecklist2());
        }

        #region Private Checklist Setters

        private void SetChecklist1(int index, StatusType status)
        {
            if (index >= 0 && index < Checklist1.Count)
                Checklist1[index].Status = status;
        }

        private void SetChecklist2(int index, StatusType status)
        {
            if (index >= 0 && index < Checklist2.Count)
                Checklist2[index].Status = status;
        }

        #endregion
    }
}
