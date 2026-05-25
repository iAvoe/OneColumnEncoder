using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.ViewModels.Cards;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace OneColumnEncoder.Helpers
{
    public static class ItemCardSelectionH
    {
        public static void ToggleOnly(ObservableCollection<ToolItemVM> zone, ToolItemVM targetCard)
        {
            foreach (ToolItemVM card in zone)
            {
                if (card != targetCard) card.IsSelected = false;
            }
            targetCard.IsSelected = !targetCard.IsSelected;
        }

        public static void SelectOnly(ObservableCollection<ToolItemVM> zone, ToolItemVM targetCard)
        {
            foreach (ToolItemVM card in zone)
                card.IsSelected = card == targetCard;
        }

        public static void UnselectAll(ObservableCollection<ToolItemVM> zone)
        {
            foreach (ToolItemVM card in zone)
                card.IsSelected = false;
        }

        public static void HandleItemCardClick(
            ToolItemVM clickedTool,
            ObservableCollection<ToolItemVM> upstreamsZone,
            ObservableCollection<ToolItemVM> encodersZone,
            ObservableCollection<ToolItemVM> analyticsZone,
            ObservableCollection<ToolItemVM> dependenciesZone,
            ObservableCollection<ToolItemVM> videoSrcImportZone,
            ObservableCollection<ToolItemVM> scriptSrcImportZone,
            ToolsImportCardVM toolsImportCard,
            Action refreshSelectedSourceStatusAfterSourceSelection,
            Action updateEncStartButtonsState,
            Action refreshSelectedSourceStatus)
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
                if (string.IsNullOrEmpty(clickedTool.Path)) return;

                SelectOnly(videoSrcImportZone, clickedTool);
                UnselectAll(scriptSrcImportZone);
                refreshSelectedSourceStatusAfterSourceSelection();
            }
            else if (scriptSrcImportZone.Contains(clickedTool))
            {
                if (string.IsNullOrEmpty(clickedTool.Path)) return;

                ToggleOnly(scriptSrcImportZone, clickedTool);
                UnselectAll(videoSrcImportZone);
                refreshSelectedSourceStatusAfterSourceSelection();
            }

            ToolCompatibilityH.RefreshDependencySelectionState(
                upstreamsZone, dependenciesZone, updateEncStartButtonsState);
            ToolCompatibilityH.RefreshSourceSelectionState(
                upstreamsZone, scriptSrcImportZone, refreshSelectedSourceStatus);
        }

        public static void ApplyDefaultSelection(ObservableCollection<ToolItemVM> zone)
        {
            if (zone.Count == 1)
                zone[0].IsSelected = true;
            else if (zone.Count > 1)
                UnselectAll(zone);
        }

        public static void RefreshToolPickedStatus(
            ToolsImportCardVM toolsImportCard,
            ToolZone toolZone,
            ObservableCollection<ToolItemVM> zone)
        {
            toolsImportCard.SetToolPickedStatus(toolZone, zone.Any(t => t.IsSelected));
        }
    }
}
