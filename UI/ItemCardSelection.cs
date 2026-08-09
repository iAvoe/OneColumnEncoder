using OneColumnEncoder.ToolManagement;

namespace OneColumnEncoder.UI;

public static class ItemCardSelection
{
    public static void ToggleOnly(ObservableCollection<ToolItemCardVM> zone, ToolItemCardVM targetCard)
    {
        foreach (ToolItemCardVM card in zone)
        {
            if (card != targetCard) card.IsSelected = false;
        }
        targetCard.IsSelected = !targetCard.IsSelected;
    }

    public static void SelectOnly(ObservableCollection<ToolItemCardVM> zone, ToolItemCardVM targetCard)
    {
        foreach (ToolItemCardVM card in zone)
            card.IsSelected = card == targetCard;
    }

    public static void UnselectAll(ObservableCollection<ToolItemCardVM> zone)
    {
        foreach (ToolItemCardVM card in zone)
            card.IsSelected = false;
    }

    public static void HandleItemCardClick(
        ToolItemCardVM clickedTool,
        ObservableCollection<ToolItemCardVM> upstreamsZone,
        ObservableCollection<ToolItemCardVM> encodersZone,
        ObservableCollection<ToolItemCardVM> analyticsZone,
        ObservableCollection<ToolItemCardVM> dependenciesZone,
        ObservableCollection<ToolItemCardVM> videoSrcImportZone,
        ObservableCollection<ToolItemCardVM> scriptSrcImportZone,
        ToolsImportCardVM toolsImportCard,
        Action refreshSelectedSourceStatusAfterSourceSelection,
        Action updateEncStartButtonsState,
        Action refreshSelectedSourceStatus,
        bool hasFfprobe)
    {
        if (upstreamsZone.Contains(clickedTool))
        {
            ToggleOnly(upstreamsZone, clickedTool);
            RefreshToolPickedStatus(toolsImportCard, ToolZone.Upstream, upstreamsZone);
        }
        else if (encodersZone.Contains(clickedTool))
        {
            ToggleOnly(encodersZone, clickedTool);
            RefreshToolPickedStatus(toolsImportCard, ToolZone.Encoder, encodersZone);
        }
        else if (analyticsZone.Contains(clickedTool))
        {
            SelectOnly(analyticsZone, clickedTool);
            RefreshToolPickedStatus(toolsImportCard, ToolZone.Analytics, analyticsZone);
        }
        else if (dependenciesZone.Contains(clickedTool))
        {
            ToggleOnly(dependenciesZone, clickedTool);
        }
        else if (videoSrcImportZone.Contains(clickedTool))
        {
            if (string.IsNullOrEmpty(clickedTool.P2TextData)) return;

            SelectOnly(videoSrcImportZone, clickedTool);
            refreshSelectedSourceStatusAfterSourceSelection();
        }
        else if (scriptSrcImportZone.Contains(clickedTool))
        {
            if (string.IsNullOrEmpty(clickedTool.P2TextData)) return;

            SelectOnly(scriptSrcImportZone, clickedTool);
            refreshSelectedSourceStatus();
        }

        ToolCompatibility.RefreshDependencySelectionState(
            upstreamsZone, dependenciesZone, updateEncStartButtonsState);
        ToolCompatibility.RefreshSrcSelectState(
            upstreamsZone, scriptSrcImportZone, refreshSelectedSourceStatus);
        ToolCompatibility.RefreshVideoSrcSelectState(
            upstreamsZone, videoSrcImportZone, hasFfprobe);
    }

    public static bool ApplyDefaultSelection(ObservableCollection<ToolItemCardVM> zone)
    {
        if (zone.Count == 1)
        {
            zone[0].IsSelected = true;
            return true;
        }
        else if (zone.Count > 1)
            UnselectAll(zone);

        return false;
    }

    public static void RefreshToolPickedStatus(
        ToolsImportCardVM toolsImportCard,
        ToolZone toolZone,
        ObservableCollection<ToolItemCardVM> zone)
    {
        toolsImportCard.SetToolPickedStatus(toolZone, zone.Any(t => t.IsSelected));
    }
}
