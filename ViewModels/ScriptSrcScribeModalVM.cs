using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using OneColumnEncoder.Helpers;

namespace OneColumnEncoder.ViewModels
{
    public class ScriptSrcScribeModalVM : BaseVM
    {
        private readonly ModalNavS _modalNavS;
        private readonly Func<string> _getSourcePath;
        private readonly Action _closeAction;
        public CloseModalCmd CloseCmd { get; }
        // 0: AVS, 1: VPY
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        #region Script text
        public static string AvsPrefix => UILangProviderM.Current["SrcScribe.AvsPrefix"];
        public static string AvsPrefix2 => UILangProviderM.Current["SrcScribe.AvsPrefix2"];
        private string _avsUserInput = "";
        public string AvsUserInput
        {
            get => _avsUserInput;
            set => SetProperty(ref _avsUserInput, value);
        }
        public static string AvsSuffix => UILangProviderM.Current["SrcScribe.AvsSuffix"];

        public static string VpyPrefix => UILangProviderM.Current["SrcScribe.VpyPrefix"];
        private string _vpyUserInput = "";
        public string VpyUserInput
        {
            get => _vpyUserInput;
            set => SetProperty(ref _vpyUserInput, value);
        }
        public static string VpyPrefix2 => UILangProviderM.Current["SrcScribe.VpyPrefix2"];
        public static string VpySuffix => UILangProviderM.Current["SrcScribe.VpySuffix"];
        #endregion

        #region UILang properties
        public static string WindowTitle => UILangProviderM.Current["SrcScribe.WindowTitle"];
        public static string ScribeDescription1 => UILangProviderM.Current["SrcScribe.Description1"];
        public static string ScribeDescription2 => UILangProviderM.Current["SrcScribe.Description2"];
        public static string NoteText => UILangProviderM.Current["SrcScribe.NoteText"];
        public static string TabAvs => UILangProviderM.Current["SrcScribe.TabAvs"];
        public static string TabVpy => UILangProviderM.Current["SrcScribe.TabVpy"];
        #endregion

        public ButtonGroupVM ScriptExportButtons { get; private set; } = null!;
        public ButtonGroupVM FinishScribeButtons { get; private set; } = null!;

        public ScriptSrcScribeModalVM(ModalNavS modalNavS, Action closeAction, Func<string> getSourcePath)
        {
            _modalNavS = modalNavS;
            _closeAction = closeAction;
            CloseCmd = new CloseModalCmd(modalNavS, closeAction);
            _getSourcePath = getSourcePath;
            BuildButtonGroups();
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        private void BuildButtonGroups()
        {
            ScriptExportButtons = ButtonGroupVM.CreateThreeButton(
                UILangProviderM.Current["SrcScribe.CopyFull"],
                UILangProviderM.Current["SrcScribe.CopyInOut"],
                UILangProviderM.Current["SrcScribe.SaveAsFile"],
                new ActionCmd(_ => CopyFullScript()),
                new ActionCmd(_ => CopyInOutSection()),
                new ActionCmd(_ => SaveAsFile()));

            FinishScribeButtons = ButtonGroupVM.CreateTwoButton(
                UILangProviderM.Current["SrcScribe.Cancel"],
                UILangProviderM.Current["SrcScribe.Confirm"],
                CloseCmd,
                new ActionCmd(_ => SaveAndImportAll()));
        }

        #region Script operations
        private void CopyFullScript()
        {
            Clipboard.SetText(GetCurrentFullScript());
            new OpenInfoOrDbgModalCmd(_modalNavS,
                UILangProviderM.Current["SrcScribe.WindowTitle"],
                UILangProviderM.Current["SrcScribe.CopiedFull"]).Execute(null);
        }

        private void CopyInOutSection()
        {
            string inOutText = SelectedTabIndex == 0
                ? $"LWLibavVideoSource(\"{_getSourcePath()}\")\r\n{AvsPrefix2}\r\n\r\n{AvsSuffix}"
                : $"import vapoursynth as vs\r\ncore = vs.core\r\nsrc = core.lsmas.LWLibavSource(source=r\"{_getSourcePath()}\")\r\n{VpyPrefix2}\r\n\r\n{VpySuffix}";

            Clipboard.SetText(inOutText);
            new OpenInfoOrDbgModalCmd(_modalNavS,
                UILangProviderM.Current["SrcScribe.WindowTitle"],
                UILangProviderM.Current["SrcScribe.CopiedSection"]).Execute(null);
        }

        private void SaveAsFile()
        {
            _ = SelectedTabIndex == 0
                ? ScriptTemplateH.BuildAvsExportScript(
                    _getSourcePath(), AvsPrefix, AvsPrefix2, AvsSuffix, AvsUserInput)
                : ScriptTemplateH.BuildVpyExportScript(
                    _getSourcePath(), VpyPrefix, VpyPrefix2, VpySuffix, VpyUserInput);

            _ = new SaveFileDialog()
            {
                Filter = SelectedTabIndex == 0
                    ? UILangProviderM.Current["SrcScribe.FilterAvs"]
                    : UILangProviderM.Current["SrcScribe.FilterVpy"],
                FileName = SelectedTabIndex == 0 ? "script.avs" : "script.vpy"
            };
        }

        private void SaveAndImportAll()
        {
            _closeAction();
            _modalNavS.Close();
        }

        private string GetCurrentFullScript()
        {
            return SelectedTabIndex == 0
                ? $"LWLibavVideoSource(\"{_getSourcePath()}\")\r\n{AvsPrefix}\r\n{AvsPrefix2}\r\n\r\n{AvsSuffix}"
                : $"import vapoursynth as vs\r\ncore = vs.core\r\nsrc = core.lsmas.LWLibavSource(source=r\"{_getSourcePath()}\")\r\n{VpyPrefix}\r\n{VpyPrefix2}\r\n\r\n{VpySuffix}";
        }
        #endregion

        #region Language switching
        private void OnLanguageChanged()
        {
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(ScribeDescription1));
            OnPropertyChanged(nameof(ScribeDescription2));
            OnPropertyChanged(nameof(NoteText));
            OnPropertyChanged(nameof(TabAvs));
            OnPropertyChanged(nameof(TabVpy));
            OnPropertyChanged(nameof(AvsPrefix));
            OnPropertyChanged(nameof(AvsSuffix));
            OnPropertyChanged(nameof(VpyPrefix));
            OnPropertyChanged(nameof(VpySuffix));

            BuildButtonGroups();
            OnPropertyChanged(nameof(ScriptExportButtons));
            OnPropertyChanged(nameof(FinishScribeButtons));
        }
        #endregion

        public override void Dispose()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            base.Dispose();
        }
    }
}
