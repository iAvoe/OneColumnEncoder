using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace OneColumnEncoder.ViewModels
{
    public class ScriptSrcScribeModalVM : BaseVM
    {
        private readonly ModalNavS _modalNavS;
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        #region Script text
        public string AvsPrefix => UILangProviderM.Current["SrcScribe.AvsPrefix"];
        private string _avsUserInput = "";
        public string AvsUserInput
        {
            get => _avsUserInput;
            set => SetProperty(ref _avsUserInput, value);
        }
        public string AvsSuffix => UILangProviderM.Current["SrcScribe.AvsSuffix"];

        public string VpyPrefix => UILangProviderM.Current["SrcScribe.VpyPrefix"];
        private string _vpyUserInput = "";
        public string VpyUserInput
        {
            get => _vpyUserInput;
            set => SetProperty(ref _vpyUserInput, value);
        }
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

        public ICommand CloseCmd { get; }
        public ButtonGroupVM ScriptExportButtons { get; private set; } = null!;
        public ButtonGroupVM FinishScribeButtons { get; private set; } = null!;

        public ScriptSrcScribeModalVM(ModalNavS modalNavS, Action closeAction)
        {
            _modalNavS = modalNavS;
            CloseCmd = new CloseModalCmd(modalNavS, closeAction);

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
            MessageBox.Show(UILangProviderM.Current["SrcScribe.CopiedFull"],
                UILangProviderM.Current["SrcScribe.WindowTitle"],
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CopyInOutSection()
        {
            string inOutText = SelectedTabIndex == 0
                ? $"{AvsPrefix}\r\n\r\n{AvsSuffix}"
                : $"{VpyPrefix}\r\n\r\n{VpySuffix}";

            Clipboard.SetText(inOutText);
            MessageBox.Show(UILangProviderM.Current["SrcScribe.CopiedSection"],
                UILangProviderM.Current["SrcScribe.WindowTitle"],
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveAsFile()
        {
            SaveFileDialog sfd = new();
            if (SelectedTabIndex == 0)
            {
                sfd.Filter = UILangProviderM.Current["SrcScribe.FilterAvs"];
                sfd.FileName = "script.avs";
            }
            else
            {
                sfd.Filter = UILangProviderM.Current["SrcScribe.FilterVpy"];
                sfd.FileName = "script.vpy";
            }

            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, GetCurrentFullScript());
            }
        }

        private void SaveAndImportAll()
        {
            _modalNavS.Close();
        }

        private string GetCurrentFullScript()
        {
            return SelectedTabIndex == 0
                ? $"{AvsPrefix}\r\n{AvsUserInput}\r\n{AvsSuffix}"
                : $"{VpyPrefix}\r\n{VpyUserInput}\r\n{VpySuffix}";
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
