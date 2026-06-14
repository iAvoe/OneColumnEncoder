namespace OneColumnEncoder.Models;

public class SourceFilePickerLangProviderM
{
    private const string VideoExtensions = "*.mkv;*.mp4;*.mov;*.avi;*.m2ts;*.ts;*.webm;*.mxf;*.vob;*.wmv;*.flv;*.f4v;*.asf;*.rm;*.rmvb;*.divx;*.xvid;*.3gp;*.3g2;*.ogv;*.ogg;*.mpg;*.mpeg;*.m1v;*.m2v;*.mp2;*.mpe;*.mpv;*.m4v;*.m4p;*.mp4v;*.dv;*.mts;*.m2t;*.trp;*.tp;*.evo;*.ifo;*.vro;*.bup;*.swf;*.wtv;*.dvr-ms;*.rec;*.yuv;*.y4m;*.hevc;*.h264;*.h265;*.264;*.265;*.vc1;*.avs2;*.avs3;*.ivf;*.drc;*.mj2;*.mjpeg;*.mjpg;*.amv;*.nsv;*.svi;*.viv;*.f4p;*.f4a;*.f4b;*.roq;*.mng;*.gifv;*.qt;*.hdmov;*.mod;*.tod;*.moi;*.pva;*.nsr;*.nut;*.fli;*.flc;*.flic;*.dsm;*.dsv;*.dsa;*.dss;*.ask;*.dat";

    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["Filter.Video"] = $"Video files ({VideoExtensions})|{VideoExtensions}|All files (*.*)|*.*",
            ["Filter.AviSynthScript"] = "AviSynth script files (*.avs)|*.avs",
            ["Filter.VapourSynthScript"] = "VapourSynth script files (*.vpy)|*.vpy",
            ["Filter.SvfiIni"] = "SVFI configuration files (*.ini)|*.ini",
            ["Filter.AllFiles"] = "All files (*.*)|*.*",
            ["NoFileSelectedTitle"] = "No file selected",
            ["MissingSelectionMessage"] = "No file selected. Choose Yes to try again, or No to cancel.",
            ["CustomScriptModeText"] = "Import custom script"
        },
        ["zh-cn"] = new()
        {
            ["Filter.Video"] = $"视频文件 ({VideoExtensions})|{VideoExtensions}|所有文件 (*.*)|*.*",
            ["Filter.AviSynthScript"] = "AviSynth 脚本文件 (*.avs)|*.avs",
            ["Filter.VapourSynthScript"] = "VapourSynth 脚本文件 (*.vpy)|*.vpy",
            ["Filter.SvfiIni"] = "SVFI 配置文件 (*.ini)|*.ini",
            ["Filter.AllFiles"] = "所有文件 (*.*)|*.*",
            ["NoFileSelectedTitle"] = "未选择文件",
            ["MissingSelectionMessage"] = "未选择文件。选择「是」重试，选择「否」取消。",
            ["CustomScriptModeText"] = "导入自定义脚本"
        },
        ["zh-tw"] = new()
        {
            ["Filter.Video"] = $"影片檔案 ({VideoExtensions})|{VideoExtensions}|所有檔案 (*.*)|*.*",
            ["Filter.AviSynthScript"] = "AviSynth 腳本檔案 (*.avs)|*.avs",
            ["Filter.VapourSynthScript"] = "VapourSynth 腳本檔案 (*.vpy)|*.vpy",
            ["Filter.SvfiIni"] = "SVFI 設定檔 (*.ini)|*.ini",
            ["Filter.AllFiles"] = "所有檔案 (*.*)|*.*",
            ["NoFileSelectedTitle"] = "未選擇檔案",
            ["MissingSelectionMessage"] = "未選擇檔案。選擇「是」重試，選擇「否」取消。",
            ["CustomScriptModeText"] = "導入自訂腳本"
        }
    };

    static SourceFilePickerLangProviderM()
    {
        Data["fr"] = new(Data["en"])
        {
            ["Filter.Video"] = $"Fichiers vidéo ({VideoExtensions})|{VideoExtensions}|Tous les fichiers (*.*)|*.*",
            ["Filter.AviSynthScript"] = "Scripts AviSynth (*.avs)|*.avs",
            ["Filter.VapourSynthScript"] = "Scripts VapourSynth (*.vpy)|*.vpy",
            ["Filter.SvfiIni"] = "Fichiers de configuration SVFI (*.ini)|*.ini",
            ["Filter.AllFiles"] = "Tous les fichiers (*.*)|*.*",
            ["NoFileSelectedTitle"] = "Aucun fichier sélectionné",
            ["MissingSelectionMessage"] = "Aucun fichier sélectionné. Oui pour réessayer, Non pour annuler.",
            ["CustomScriptModeText"] = "Importer un script perso"
        };
        Data["es"] = new(Data["en"])
        {
            ["Filter.Video"] = $"Archivos de vídeo ({VideoExtensions})|{VideoExtensions}|Todos los archivos (*.*)|*.*",
            ["Filter.AviSynthScript"] = "Scripts AviSynth (*.avs)|*.avs",
            ["Filter.VapourSynthScript"] = "Scripts VapourSynth (*.vpy)|*.vpy",
            ["Filter.SvfiIni"] = "Configuración SVFI (*.ini)|*.ini",
            ["Filter.AllFiles"] = "Todos los archivos (*.*)|*.*",
            ["NoFileSelectedTitle"] = "No se seleccionó archivo",
            ["MissingSelectionMessage"] = "No se seleccionó archivo. Sí para reintentar; No para cancelar.",
            ["CustomScriptModeText"] = "Importar script propio"
        };
        Data["ja"] = new(Data["en"])
        {
            ["Filter.Video"] = $"動画ファイル ({VideoExtensions})|{VideoExtensions}|すべてのファイル (*.*)|*.*",
            ["Filter.AviSynthScript"] = "AviSynth スクリプト (*.avs)|*.avs",
            ["Filter.VapourSynthScript"] = "VapourSynth スクリプト (*.vpy)|*.vpy",
            ["Filter.SvfiIni"] = "SVFI 設定ファイル (*.ini)|*.ini",
            ["Filter.AllFiles"] = "すべてのファイル (*.*)|*.*",
            ["NoFileSelectedTitle"] = "ファイル未選択",
            ["MissingSelectionMessage"] = "ファイルが選択されていません。「はい」で再試行、「いいえ」でキャンセル。",
            ["CustomScriptModeText"] = "カスタムスクリプトを取込"
        };
        Data["ru"] = new(Data["en"])
        {
            ["Filter.Video"] = $"Видео ({VideoExtensions})|{VideoExtensions}|Все файлы (*.*)|*.*",
            ["Filter.AviSynthScript"] = "Скрипты AviSynth (*.avs)|*.avs",
            ["Filter.VapourSynthScript"] = "Скрипты VapourSynth (*.vpy)|*.vpy",
            ["Filter.SvfiIni"] = "Конфигурации SVFI (*.ini)|*.ini",
            ["Filter.AllFiles"] = "Все файлы (*.*)|*.*",
            ["NoFileSelectedTitle"] = "Файл не выбран",
            ["MissingSelectionMessage"] = "Файл не выбран. Да — повторить, Нет — отменить.",
            ["CustomScriptModeText"] = "Импорт своего скрипта"
        };
    }

    private readonly Dictionary<string, string> _d;

    public string LanguageCode { get; }
    public string this[string key] => _d.TryGetValue(key, out var value) ? value : key;

    public string VideoFilter { get; }
    public string AviSynthScriptFilter { get; }
    public string VapourSynthScriptFilter { get; }
    public string SvfiIniFilter { get; }
    public string AllFilesFilter { get; }
    public string NoFileSelectedTitle { get; }
    public string MissingSelectionMessage { get; }
    public string CustomScriptModeText { get; }

    public SourceFilePickerLangProviderM(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];

        VideoFilter = _d["Filter.Video"];
        AviSynthScriptFilter = _d["Filter.AviSynthScript"];
        VapourSynthScriptFilter = _d["Filter.VapourSynthScript"];
        SvfiIniFilter = _d["Filter.SvfiIni"];
        AllFilesFilter = _d["Filter.AllFiles"];
        NoFileSelectedTitle = _d["NoFileSelectedTitle"];
        MissingSelectionMessage = _d["MissingSelectionMessage"];
        CustomScriptModeText = _d["CustomScriptModeText"];
    }
}
