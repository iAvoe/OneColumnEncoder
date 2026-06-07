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
using OneColumnEncoder.ViewModels.Cards;

namespace OneColumnEncoder.ViewModels
{
    public class ScriptScribeModalVM : BaseVM
    {
        private readonly ModalNavS _modalNavS;
        private readonly Func<string> _getSourcePath;
        private readonly Action _closeAction;
        private readonly ToolItemCardVM _avsItem;
        private readonly ToolItemCardVM _vpyItem;
        private readonly Action<ToolItemCardVM, SourceFileKind, string> _afterImport;
        public CloseModalCmd CloseCmd { get; }
        // 0: AVS, 1: VPY
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        // Avs/VpyPrefix is a placeholder (import directory w/out video source path), so its unusable for output
        // Avs/VpyPrefix2 is a guidance comment to keep
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
        public string WindowTitle => "1cenc Script Generator";
        public static string ScribeDescription1 => UILangProviderM.Current["SrcScribe.Description1"];
        public static string ScribeDescription2 => UILangProviderM.Current["SrcScribe.Description2"];
        public static string NoteText => UILangProviderM.Current["SrcScribe.NoteText"];
        public static string TabAvs => UILangProviderM.Current["SrcScribe.TabAvs"];
        public static string TabVpy => UILangProviderM.Current["SrcScribe.TabVpy"];
        #endregion

        public ButtonGroupVM ScriptExportButtons { get; private set; } = null!;
        public ButtonGroupVM FinishScribeButtons { get; private set; } = null!;

        public ScriptScribeModalVM(
            ModalNavS modalNavS,
            Action closeAction,
            Func<string> getSourcePath,
            ToolItemCardVM avsItem,
            ToolItemCardVM vpyItem,
            Action<ToolItemCardVM, SourceFileKind, string> afterImport)
        {
            _modalNavS = modalNavS;
            _closeAction = closeAction;
            CloseCmd = new CloseModalCmd(closeAction);
            _getSourcePath = getSourcePath;
            _avsItem = avsItem;
            _vpyItem = vpyItem;
            _afterImport = afterImport;
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
            ScriptExportButtons.B3_3Icon = SvgIconProviderH.GameSave;

            FinishScribeButtons = ButtonGroupVM.CreateTwoButton(
                UILangProviderM.Current["SrcScribe.Cancel"],
                UILangProviderM.Current["SrcScribe.Confirm"],
                CloseCmd,
                new ActionCmd(_ => SaveAndImportAll()));
        }

        #region ThreeButtonGroup: copy full, copy in-out, save as file
        private void CopyFullScript()
        {
            Clipboard.SetText(GetCurrentFullScript());
            new OpenInfoModalCmd(
                _modalNavS,
                UILangProviderM.Current["SrcScribe.WindowTitle"],
                UILangProviderM.Current["SrcScribe.CopiedFull"]).Execute(null);
        }
        private void CopyInOutSection()
        {
            string sourcePath = _getSourcePath();
            string inOutText = SelectedTabIndex == 0
                ? ScriptTemplateH.BuildAvsInOutSection(sourcePath, AvsPrefix2, AvsSuffix)
                : ScriptTemplateH.BuildVpyInOutSection(sourcePath, VpyPrefix2, VpySuffix);

            Clipboard.SetText(inOutText);
            new OpenInfoModalCmd(
                _modalNavS,
                UILangProviderM.Current["SrcScribe.WindowTitle"],
                UILangProviderM.Current["SrcScribe.CopiedSection"]).Execute(null);
        }
        private void SaveAsFile()
        {
            string script = SelectedTabIndex == 0
                ? ScriptTemplateH.BuildAvsExportScript(
                    _getSourcePath(), AvsPrefix, AvsPrefix2, AvsSuffix, AvsUserInput)
                : ScriptTemplateH.BuildVpyExportScript(
                    _getSourcePath(), VpyPrefix, VpyPrefix2, VpySuffix, VpyUserInput);

            SaveFileDialog dialog = new()
            {
                Title = UILangProviderM.Current["SrcScribe.SavingWindowTitle"],
                Filter = SelectedTabIndex == 0
                    ? UILangProviderM.Current["SrcScribe.FilterAvs"]
                    : UILangProviderM.Current["SrcScribe.FilterVpy"],
                FileName = SelectedTabIndex == 0 ? "script.avs" : "script.vpy"
            };

            if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;

            if (TryWriteScript(dialog.FileName, script))
                ShowSavedMessage(dialog.FileName);
        }

        private void SaveAndImportAll()
        {
            string sourcePath = _getSourcePath();
            string avsScript = ScriptTemplateH.BuildAvsExportScript(
                sourcePath, AvsPrefix, AvsPrefix2, AvsSuffix, AvsUserInput);
            string vpyScript = ScriptTemplateH.BuildVpyExportScript(
                sourcePath, VpyPrefix, VpyPrefix2, VpySuffix, VpyUserInput);

            SaveFileDialog dialog = new()
            {
                Title = UILangProviderM.Current["SrcScribe.SavingWindowTitle"],
                Filter = UILangProviderM.Current["SrcScribe.FilterAvs"],
                FileName = "script.avs"
            };

            if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;

            string avsPath = dialog.FileName;
            string directory = Path.GetDirectoryName(avsPath) ?? ".";
            string vpyPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(avsPath) + ".vpy");

            if (!TryWriteScripts(avsPath, avsScript, vpyPath, vpyScript)) return;

            ImportScript(_avsItem, SourceFileKind.AviSynthScript, avsPath);
            ImportScript(_vpyItem, SourceFileKind.VapourSynthScript, vpyPath);
            _closeAction();
        }

        private bool TryWriteScript(string path, string script)
        {
            try
            {
                File.WriteAllText(path, script);
                return true;
            }
            catch (Exception ex)
            {
                ShowSaveError(ex);
                return false;
            }
        }

        private bool TryWriteScripts(string avsPath, string avsScript, string vpyPath, string vpyScript)
        {
            try
            {
                File.WriteAllText(avsPath, avsScript);
                File.WriteAllText(vpyPath, vpyScript);
                return true;
            }
            catch (Exception ex)
            {
                ShowSaveError(ex);
                return false;
            }
        }

        private void ImportScript(ToolItemCardVM item, SourceFileKind kind, string path)
        {
            item.P2TextData = path;
            item.P1TextData = SourceFilePickerH.GetPrimaryText(kind, path);
            _afterImport(item, kind, path);
        }

        private void ShowSaveError(Exception ex)
        {
            new OpenErrModalCmd(
                _modalNavS,
                UILangProviderM.Current["SrcScribe.WindowTitle"],
                $"Failed to save scripts: {ex.Message}").Execute(null);
        }

        private void ShowSavedMessage(string path)
        {
            new OpenInfoModalCmd(
                _modalNavS,
                UILangProviderM.Current["SrcScribe.WindowTitle"],
                $"Script saved:\n{path}").Execute(null);
        }

        private string GetCurrentFullScript()
        {
            string sourcePath = _getSourcePath();
            return SelectedTabIndex == 0
                ? ScriptTemplateH.BuildAvsEditorScript(sourcePath, AvsPrefix2, AvsUserInput)
                : ScriptTemplateH.BuildVpyEditorScript(sourcePath, VpyPrefix2, VpySuffix, VpyUserInput);
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
