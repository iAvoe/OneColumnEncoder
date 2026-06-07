using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System;
using System.Linq;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenSampleClipCmd(ModalNavS modalNavS, Func<EncodingPipelineRequest?> buildRequest, VideoAnalysisM srcVideoAnalysis, AppConfM appConfM) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly Func<EncodingPipelineRequest?> _buildRequest = buildRequest;
        private readonly VideoAnalysisM _srcVideoAnalysis = srcVideoAnalysis;
        private readonly AppConfM _appConfM = appConfM;

        public override void Execute(object? parameter)
        {
            SampleClipModal? existingWindow = Application.Current.Windows
                .OfType<SampleClipModal>()
                .FirstOrDefault();

            if (existingWindow != null)
            {
                existingWindow.Activate();
                return;
            }

            if (_modalNavS.IsOpen)
                _modalNavS.Close();

            SampleClipModal window = new();
            SampleClipVM vm = new(_modalNavS, window.Close, _buildRequest, _srcVideoAnalysis, _appConfM);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.Show();
        }
    }
}
