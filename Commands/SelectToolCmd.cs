using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OneColumnEncoder.Commands
{
    public class SelectToolCmd(MainVM mainVM) : BaseCmd
    {
        private readonly MainVM _mainVM = mainVM;

        public override bool CanExecute(object? parameter)
        {
            return parameter is ToolItemVM;
        }

        public override void Execute(object? parameter)
        {
            if (parameter is not ToolItemVM clickedTool) return;

            if (_mainVM.UpstreamsZone != null && _mainVM.UpstreamsZone.Contains(clickedTool))
            {
                ResetSelection(_mainVM.UpstreamsZone, clickedTool);
                bool isSelected = _mainVM.UpstreamsZone.Any(t => t.IsSelected);
                _mainVM.ToolsImportCard.SetToolPickedStatus(ToolZone.Upstream, isSelected);
            }
            else if (_mainVM.EncodersZone != null && _mainVM.EncodersZone.Contains(clickedTool))
            {
                ResetSelection(_mainVM.EncodersZone, clickedTool);
                bool isSelected = _mainVM.EncodersZone.Any(t => t.IsSelected);
                _mainVM.ToolsImportCard.SetToolPickedStatus(ToolZone.Encoder, isSelected);
            }
            else if (_mainVM.AnalyticsZone != null && _mainVM.AnalyticsZone.Contains(clickedTool))
            {
                SelectOnly(_mainVM.AnalyticsZone, clickedTool);
                _mainVM.ToolsImportCard.SetToolPickedStatus(ToolZone.Analytics, true);
            }
            else if (_mainVM.DependenciesZone != null && _mainVM.DependenciesZone.Contains(clickedTool))
            {
                ResetSelection(_mainVM.DependenciesZone, clickedTool);
            }
            else if (_mainVM.VideoSrcImportZone != null && _mainVM.VideoSrcImportZone.Contains(clickedTool))
            {
                if (string.IsNullOrEmpty(clickedTool.Path)) return;

                SelectOnly(_mainVM.VideoSrcImportZone, clickedTool);
                UnselectAll(_mainVM.ScriptSrcImportZone);
                _mainVM.RefreshSelectedSourceStatusAfterSourceSelection();
            }
            else if (_mainVM.ScriptSrcImportZone != null && _mainVM.ScriptSrcImportZone.Contains(clickedTool))
            {
                if (string.IsNullOrEmpty(clickedTool.Path)) return;

                ResetSelection(_mainVM.ScriptSrcImportZone, clickedTool);
                UnselectAll(_mainVM.VideoSrcImportZone!);
                _mainVM.RefreshSelectedSourceStatusAfterSourceSelection();
            }

            ToolCompatibilityH.RefreshDependencySelectionState(
                _mainVM.UpstreamsZone!, _mainVM.DependenciesZone!, _mainVM.UpdateEncStartButtonsState);
            ToolCompatibilityH.RefreshSourceSelectionState(
                _mainVM.UpstreamsZone!, _mainVM.ScriptSrcImportZone!, _mainVM.RefreshSelectedSourceStatus);
        }

        private static void ResetSelection(ObservableCollection<ToolItemVM> zone, ToolItemVM targetCard)
        {
            foreach (ToolItemVM card in zone)
            {
                if (card != targetCard) card.IsSelected = false;
            }
            targetCard.IsSelected = !targetCard.IsSelected;
        }

        private static void SelectOnly(ObservableCollection<ToolItemVM> zone, ToolItemVM targetCard)
        {
            foreach (ToolItemVM card in zone)
                card.IsSelected = card == targetCard;
        }

        private static void UnselectAll(ObservableCollection<ToolItemVM> zone)
        {
            foreach (ToolItemVM card in zone)
                card.IsSelected = false;
        }

        // CanExecuteChanged defined in BaseCmd
    }
}
