using OneColumnEncoder.ViewModels;

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
            _mainVM.SelectItemCard(clickedTool);
        }

        // CanExecuteChanged defined in BaseCmd
    }
}
