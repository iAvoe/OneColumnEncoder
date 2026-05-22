using Microsoft.Win32;
using OneColumnEncoder.Commands;
using OneColumnEncoder.Stores;
using OneColumnEncoder.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class ScripeSrcScribeModalVM : BaseVM
    {
        private int _selectedTabIndex; // 0: AVS, 1: VPY
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        // TODO: Move text data to modal and translate
        #region Script text
        public string AvsPrefix { get; } = "LWLibavVideoSource(\"视频文件路径\")\r\n# 在下方添加更多滤镜或留空...";
        private string _avsUserInput = "... 用户输入内容（颜色做出差异） ...";
        public string AvsUserInput
        {
            get => _avsUserInput;
            set => SetProperty(ref _avsUserInput, value);
        }
        public string AvsSuffix { get; } = "# ... 编辑结束位置";

        public string VpyPrefix { get; } = "import vapoursynth as vs\r\ncore = vs.core\r\nsrc = core.lsmas.LWLibavSource(source=r\"视频文件路径\")\r\n# 按需在此加入滤镜或留空（沿用 src 变量，或在最后赋值回 src）";
        private string _vpyUserInput = "... 用户输入内容（颜色做出差异） ...";
        public string VpyUserInput
        {
            get => _vpyUserInput;
            set => SetProperty(ref _vpyUserInput, value);
        }
        public string VpySuffix { get; } = "# ... 编辑结束位置\r\nsrc.set_output()";
        #endregion

        public ScribeLangPack Lang { get; } = new ScribeLangPack();
        public ButtonGroupVM ScriptExportButtons { get; set; }
        public ButtonGroupVM FinishScribeButtons { get; set; }

        public ScriptSrcScribeModalVM(Window window)
        {
            _window = window;

            ScriptExportButtons = ButtonGroupVM.CreateThreeButton (
                "复制完整脚本", "复制输入输出段", "另存为文件",
                new ActionCmd(CopyFullScript), new ActionCmd(CopyInOutSection), new ActionCmd(SaveAsFile));

            // Lock buttons if video source is not ready
            ScriptExportButtons.B3_1IsEnabled = isVideoLoaded;
            ScriptExportButtons.B3_2IsEnabled = isVideoLoaded;
            ScriptExportButtons.B3_3IsEnabled = isVideoLoaded;

            FinishScribeButtons = ButtonGroupVM.CreateTwoButton(
                "取消（仅关闭）", "确认（保存并导入所有脚本）",
                new ActionCmd(CloseModal), new ActionCmd(SaveAndImportAll));
        }

        #region Script operations
        private void CopyFullScript()
        {
            Clipboard.SetText(GetCurrentFullScript());
            // 提示通常可以通过你项目里的 OpenUsingModal 或是 MessageBox
            MessageBox.Show("完整脚本已复制到剪贴板！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CopyInOutSection()
        {
            string inOutText = SelectedTabIndex == 0
                ? $"{AvsPrefix}\r\n\r\n{AvsSuffix}"
                : $"{VpyPrefix}\r\n\r\n{VpySuffix}";

            Clipboard.SetText(inOutText);
            MessageBox.Show("基准输入输出段已复制到剪贴板！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveAsFile()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (SelectedTabIndex == 0)
            {
                sfd.Filter = "AviSynth 脚本 (*.avs)|*.avs";
                sfd.FileName = "script.avs";
            }
            else
            {
                sfd.Filter = "VapourSynth 脚本 (*.vpy)|*.vpy";
                sfd.FileName = "script.vpy";
            }

            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, GetCurrentFullScript());
            }
        }

        private void CloseModal()
        {
            _modalNavS.Close(); // 使用你提供的通用状态管理关闭
        }

        private void SaveAndImportAll()
        {
            // TODO: 将 GetCurrentFullScript() 的内容持久化回你的总线数据中
            _modalNavS.Close();
        }

        private string GetCurrentFullScript()
        {
            return SelectedTabIndex == 0
                ? $"{AvsPrefix}\r\n{AvsUserInput}\r\n{AvsSuffix}"
                : $"{VpyPrefix}\r\n{VpyUserInput}\r\n{VpySuffix}";
        }
        #endregion

        // 独立的多语言本地化字段映射
        public class ScribeLangPack
        {
            public string WindowTitle { get; set; } = "生成上游程序脚本";
            public string Title { get; set; } = "生成上游程序脚本";
            public string ScribeDescription1 { get; set; } = "自动根据已导入的视频构建「调用解码器生成 Y4M 流并导出」的脚本，可以将需要的滤镜粘贴进来，也可以将解码输出段落复制给其它的待命脚本。";
            public string ScribeDescription2 { get; set; } = "若按钮锁定，则先回到主界面完成视频文件导入操作。";
            public string NoteText { get; set; } = "注：默认只使用「确认」按钮生成的脚本";
        }
    }
}
