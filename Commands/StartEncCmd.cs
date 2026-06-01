using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Stores;
using System;

namespace OneColumnEncoder.Commands
{
    public class StartEncCmd(Func<EncodingPipelineRequest?> buildRequest, ModalNavS modalNavS) : BaseCmd
    {
        private readonly Func<EncodingPipelineRequest?> _buildRequest = buildRequest;
        private readonly ModalNavS _modalNavS = modalNavS;

        public override void Execute(object? parameter)
        {
            EncodingPipelineRequest? request = _buildRequest();
            if (request == null) return;

            EncodingPipelineCommand command = EncodingPipelineH.BuildY4mCommand(request);
            new OpenInfoOrDbgModalCmd(
                _modalNavS,
                "Encoding Command",
                command.CommandLine).Execute(null);
        }
    }
}
