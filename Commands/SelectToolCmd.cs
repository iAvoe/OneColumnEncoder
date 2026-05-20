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
            }
            else if (_mainVM.EncodersZone != null && _mainVM.EncodersZone.Contains(clickedTool))
            {
                ResetSelection(_mainVM.EncodersZone, clickedTool);
            }
            else if (_mainVM.AnalyticsZone != null && _mainVM.AnalyticsZone.Contains(clickedTool))
            {
                ResetSelection(_mainVM.AnalyticsZone, clickedTool);
            }
            // add if else for AnalyticsZone, SrcImportZone
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
