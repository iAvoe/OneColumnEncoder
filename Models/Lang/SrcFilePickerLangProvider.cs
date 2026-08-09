namespace OneColumnEncoder.Models.Lang;

public class SrcFilePickerLangProvider : LangProviderBase
{
    public const string VideoExtensions = "*.mkv;*.mp4;*.mov;*.avi;*.m2ts;*.ts;*.webm;*.mxf;*.vob;*.wmv;*.flv;*.f4v;*.asf;*.rm;*.rmvb;*.divx;*.xvid;*.3gp;*.3g2;*.ogv;*.ogg;*.mpg;*.mpeg;*.m1v;*.m2v;*.mp2;*.mpe;*.mpv;*.m4v;*.m4p;*.mp4v;*.dv;*.mts;*.m2t;*.trp;*.tp;*.evo;*.ifo;*.vro;*.bup;*.swf;*.wtv;*.dvr-ms;*.rec;*.yuv;*.y4m;*.hevc;*.h264;*.h265;*.264;*.265;*.vc1;*.avs2;*.avs3;*.ivf;*.drc;*.mj2;*.mjpeg;*.mjpg;*.amv;*.nsv;*.svi;*.viv;*.f4p;*.f4a;*.f4b;*.roq;*.mng;*.gifv;*.qt;*.hdmov;*.mod;*.tod;*.moi;*.pva;*.nsr;*.nut;*.fli;*.flc;*.flic;*.dsm;*.dsv;*.dsa;*.dss;*.ask;*.dat";

    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["Filter.Video"] = $"Video files ({VideoExtensions})|{VideoExtensions}|All files (*.*)|*.*",
            ["Filter.AviSynthScript"] = "AviSynth script files (*.avs)|*.avs",
            ["Filter.VapourSynthScript"] = "VapourSynth script files (*.vpy)|*.vpy",
            ["Filter.SvfiIni"] = "SVFI configuration files (*.ini)|*.ini",
            ["Filter.AllFiles"] = "All files (*.*)|*.*",
            ["CustomScriptModeText"] = "Import custom script"
        },
        ["zh-cn"] = new()
        {
            ["Filter.Video"] = $"视频文件 ({VideoExtensions})|{VideoExtensions}|所有文件 (*.*)|*.*",
            ["Filter.AviSynthScript"] = "AviSynth 脚本文件 (*.avs)|*.avs",
            ["Filter.VapourSynthScript"] = "VapourSynth 脚本文件 (*.vpy)|*.vpy",
            ["Filter.SvfiIni"] = "SVFI 配置文件 (*.ini)|*.ini",
            ["Filter.AllFiles"] = "所有文件 (*.*)|*.*",
            ["CustomScriptModeText"] = "导入自定义脚本"
        },
        ["zh-tw"] = new()
        {
            ["Filter.Video"] = $"影片檔案 ({VideoExtensions})|{VideoExtensions}|所有檔案 (*.*)|*.*",
            ["Filter.AviSynthScript"] = "AviSynth 腳本檔案 (*.avs)|*.avs",
            ["Filter.VapourSynthScript"] = "VapourSynth 腳本檔案 (*.vpy)|*.vpy",
            ["Filter.SvfiIni"] = "SVFI 設定檔 (*.ini)|*.ini",
            ["Filter.AllFiles"] = "所有檔案 (*.*)|*.*",
            ["CustomScriptModeText"] = "導入自訂腳本"
        }
    };

    static SrcFilePickerLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["Filter.Video"] = $"Fichiers vidéo ({VideoExtensions})|{VideoExtensions}|Tous les fichiers (*.*)|*.*",
            ["Filter.AviSynthScript"] = "Scripts AviSynth (*.avs)|*.avs",
            ["Filter.VapourSynthScript"] = "Scripts VapourSynth (*.vpy)|*.vpy",
            ["Filter.SvfiIni"] = "Fichiers de configuration SVFI (*.ini)|*.ini",
            ["Filter.AllFiles"] = "Tous les fichiers (*.*)|*.*",
            ["CustomScriptModeText"] = "Importer un script perso"
        };
        Data["es"] = new(Data["en"])
        {
            ["Filter.Video"] = $"Archivos de vídeo ({VideoExtensions})|{VideoExtensions}|Todos los archivos (*.*)|*.*",
            ["Filter.AviSynthScript"] = "Scripts AviSynth (*.avs)|*.avs",
            ["Filter.VapourSynthScript"] = "Scripts VapourSynth (*.vpy)|*.vpy",
            ["Filter.SvfiIni"] = "Configuración SVFI (*.ini)|*.ini",
            ["Filter.AllFiles"] = "Todos los archivos (*.*)|*.*",
            ["CustomScriptModeText"] = "Importar script propio"
        };
        Data["ja"] = new(Data["en"])
        {
            ["Filter.Video"] = $"動画ファイル ({VideoExtensions})|{VideoExtensions}|すべてのファイル (*.*)|*.*",
            ["Filter.AviSynthScript"] = "AviSynth スクリプト (*.avs)|*.avs",
            ["Filter.VapourSynthScript"] = "VapourSynth スクリプト (*.vpy)|*.vpy",
            ["Filter.SvfiIni"] = "SVFI 設定ファイル (*.ini)|*.ini",
            ["Filter.AllFiles"] = "すべてのファイル (*.*)|*.*",
            ["CustomScriptModeText"] = "カスタムスクリプトを取込"
        };
        Data["ru"] = new(Data["en"])
        {
            ["Filter.Video"] = $"Видео ({VideoExtensions})|{VideoExtensions}|Все файлы (*.*)|*.*",
            ["Filter.AviSynthScript"] = "Скрипты AviSynth (*.avs)|*.avs",
            ["Filter.VapourSynthScript"] = "Скрипты VapourSynth (*.vpy)|*.vpy",
            ["Filter.SvfiIni"] = "Конфигурации SVFI (*.ini)|*.ini",
            ["Filter.AllFiles"] = "Все файлы (*.*)|*.*",
            ["CustomScriptModeText"] = "Импорт своего скрипта"
        };
    }

    public string VideoFilter { get; }
    public string AviSynthScriptFilter { get; }
    public string VapourSynthScriptFilter { get; }
    public string SvfiIniFilter { get; }
    public string AllFilesFilter { get; }
    public string CustomScriptModeText { get; }

    public SrcFilePickerLangProvider(string languageCode) : base(languageCode, Data)
    {
        VideoFilter = this["Filter.Video"];
        AviSynthScriptFilter = this["Filter.AviSynthScript"];
        VapourSynthScriptFilter = this["Filter.VapourSynthScript"];
        SvfiIniFilter = this["Filter.SvfiIni"];
        AllFilesFilter = this["Filter.AllFiles"];
        CustomScriptModeText = this["CustomScriptModeText"];
    }
}
