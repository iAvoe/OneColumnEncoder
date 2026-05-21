using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class SelectToolCmd : BaseCmd
    {
        private readonly MainVM _mainVM;

        public SelectToolCmd(MainVM mainVM)
        {
            _mainVM = mainVM;
        }

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
                ResetSelection(_mainVM.AnalyticsZone, clickedTool);
                bool isSelected = _mainVM.AnalyticsZone.Any(t => t.IsSelected);
                _mainVM.ToolsImportCard.SetToolPickedStatus(ToolZone.Analytics, isSelected);
            }
            else if (_mainVM.SrcImportZone != null && _mainVM.SrcImportZone.Contains(clickedTool))
            {
                ResetSelection(_mainVM.SrcImportZone, clickedTool);
                bool isSelected = _mainVM.SrcImportZone.Any(t => t.IsSelected);
                _mainVM.SrcValidationCard.SetSourcePickedStatus(isSelected);
            }
        }

        private void ResetSelection(ObservableCollection<ToolItemVM> zone, ToolItemVM targetCard)
        {
            foreach (ToolItemVM card in zone)
            {
                if (card != targetCard) card.IsSelected = false;
            }
            targetCard.IsSelected = !targetCard.IsSelected;
        }

        // CanExecuteChanged defined in BaseCmd
    }
}
