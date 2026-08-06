using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels.Cards;
using OneColumnEncoder.ConcatManagement;
using OneColumnEncoder.Persistence;
using System.IO;

namespace OneColumnEncoder.RepartManagement;

public sealed class VideoSourceRepartState
{
    private readonly ToolItemCardVM? _card;
    private RepartPlanM? _plan;

    public VideoSourceRepartState(IEnumerable<ToolItemCardVM> videoSourceZone)
    {
        _card = videoSourceZone.FirstOrDefault(item =>
            item.Name.Equals(RepartLangProvider.Current.ToolSourceVideoSrcRepart, StringComparison.OrdinalIgnoreCase));
        if (_card != null) _card.UseAutoAddReplaceText = false;
    }

    public bool IsActive => _card?.IsSelected == true;
    public bool IsRepartItem(ToolItemCardVM item) => ReferenceEquals(item, _card);
    public RepartPlanM? CurrentPlan => _plan?.Clone();
    public string[] CurrentFilePaths => _plan?.Sources.Select(source => source.FilePath).ToArray() ?? [];

    public string RegenerateFileList(Guid executionId)
    {
        if (_plan == null) return string.Empty;
        string path = Path.Combine(
            SaveLoadBase<RepartFileListPathPlaceholder>.GetConfigDirectory(),
            $"source_rp_{_plan.PlanId:N}_{executionId:N}.txt");
        return ConcatFileListGenerator.GenerateFileList(CurrentFilePaths, path);
    }

    public void ApplyPlan(RepartPlanM plan)
    {
        _plan = plan.Clone();
        if (_card == null) return;

        string[] names = [.. plan.Sources.Select(source => source.DisplayName)];
        _card.P1TextData = $"{plan.Sources.Count} → {plan.Outputs.Count}";
        _card.P1TooltipText = string.Join(", ", names);
        _card.P2TextData = plan.Sources.Count > 0
            ? Path.GetDirectoryName(plan.Sources[0].FilePath) ?? string.Empty
            : string.Empty;
        RefreshTitle();
    }

    public bool ReorderSources(string[] filePaths)
    {
        if (_plan == null || filePaths.Length != _plan.Sources.Count)
            return false;

        Dictionary<string, RepartSourceM> sourcesByPath = _plan.Sources.ToDictionary(
            source => Path.GetFullPath(source.FilePath),
            StringComparer.OrdinalIgnoreCase);
        if (filePaths.Any(path => !sourcesByPath.ContainsKey(Path.GetFullPath(path)))
            || filePaths.Distinct(StringComparer.OrdinalIgnoreCase).Count() != filePaths.Length)
            return false;

        RepartPlanM reordered = new()
        {
            PlanId = _plan.PlanId,
            FfprobePath = _plan.FfprobePath,
            ReferenceRawJson = _plan.ReferenceRawJson,
            FormatSignature = _plan.FormatSignature,
            FrameRateNumerator = _plan.FrameRateNumerator,
            FrameRateDenominator = _plan.FrameRateDenominator,
            TotalFrames = _plan.TotalFrames,
            Sources = [.. filePaths.Select(path => sourcesByPath[Path.GetFullPath(path)])],
            Outputs = [.. _plan.Outputs],
            Dividers = [.. _plan.Dividers]
        };
        ApplyPlan(reordered);
        return true;
    }

    public void Clear()
    {
        _plan = null;
        if (_card == null) return;
        _card.P1TextData = string.Empty;
        _card.P1TooltipText = null;
        _card.P2TextData = string.Empty;
        RefreshTitle();
    }

    public void RefreshLanguage()
    {
        if (_card == null) return;
        _card.UseAutoAddReplaceText = false;
        RefreshTitle();
    }

    private void RefreshTitle()
    {
        if (_card == null) return;
        _card.Name = RepartLangProvider.Current.ToolSourceVideoSrcRepart;
    }

    private sealed class RepartFileListPathPlaceholder : SaveLoadBase<RepartFileListPathPlaceholder>
    {
        protected override string FilePath => string.Empty;
    }
}
