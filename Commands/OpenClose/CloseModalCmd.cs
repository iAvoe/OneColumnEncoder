namespace OneColumnEncoder.Commands.OpenClose
{
    public class CloseModalCmd(Action closeAction) : BaseCmd
    {
        public override void Execute(object? parameter)
        {
            closeAction();
        }
    }
}
