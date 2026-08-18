namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Localized strings for the VapourSynth preview dialog.
/// </summary>
public class VpyPreviewLangProvider(string languageCode) : LangProviderBase(languageCode, Data)
{
    public const string WindowTitle = "VapourSynth Preview";
    public const string DebugWindowTitle = "VapourSynth Preview frame data";

    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["VpyPreview.Ready"] = "Ready",
            ["VpyPreview.ExtractingSource"] = "Extracting frame from output 0 (original)...",
            ["VpyPreview.ExtractingFiltered"] = "Extracting frame from output 1 (filtered)...",
            ["VpyPreview.FrameRendered"] = "Frame {0} rendered",
            ["VpyPreview.Cancelled"] = "Cancelled",
            ["VpyPreview.ScriptError"] = "Script error: {0}",
            ["VpyPreview.PreviewFrameFileMissing"] = "Preview frame file missing",
            ["VpyPreview.LogVspipeOutput"] = "vspipe output {0}",
            ["VpyPreview.LogVspipeExitCode"] = "vspipe exit code {0}",
            ["VpyPreview.LogErrorPrefix"] = "Error: ",
            ["VpyPreview.NoneText"] = "<none>",
            ["VpyPreview.DebugSourceVideo"] = "Source video: {0}",
            ["VpyPreview.DebugVspipePath"] = "vspipe path: {0}",
            ["VpyPreview.DebugVspipeY4mArg"] = "vspipe y4m arg: {0}",
            ["VpyPreview.DebugTotalFrames"] = "TotalFrames: {0}",
            ["VpyPreview.DebugMaxPositionSeconds"] = "MaxPositionSeconds: {0}",
            ["VpyPreview.DebugPreviewScript"] = "Preview script:",
        },
        ["zh-cn"] = new()
        {
            ["VpyPreview.Ready"] = "就绪",
            ["VpyPreview.ExtractingSource"] = "正在从输出 0（原始）提取帧...",
            ["VpyPreview.ExtractingFiltered"] = "正在从输出 1（滤镜后）提取帧...",
            ["VpyPreview.FrameRendered"] = "已渲染第 {0} 帧",
            ["VpyPreview.Cancelled"] = "已取消",
            ["VpyPreview.ScriptError"] = "脚本错误：{0}",
            ["VpyPreview.PreviewFrameFileMissing"] = "预览帧文件缺失",
            ["VpyPreview.LogVspipeOutput"] = "vspipe 输出 {0}",
            ["VpyPreview.LogVspipeExitCode"] = "vspipe 退出码 {0}",
            ["VpyPreview.LogErrorPrefix"] = "错误：",
            ["VpyPreview.NoneText"] = "<无>",
            ["VpyPreview.DebugSourceVideo"] = "源视频：{0}",
            ["VpyPreview.DebugVspipePath"] = "vspipe 路径：{0}",
            ["VpyPreview.DebugVspipeY4mArg"] = "vspipe y4m 参数：{0}",
            ["VpyPreview.DebugTotalFrames"] = "总帧数：{0}",
            ["VpyPreview.DebugMaxPositionSeconds"] = "最大秒数：{0}",
            ["VpyPreview.DebugPreviewScript"] = "预览脚本：",
        },
        ["zh-tw"] = new()
        {
            ["VpyPreview.Ready"] = "就緒",
            ["VpyPreview.ExtractingSource"] = "正在從輸出 0（原始）提取幀...",
            ["VpyPreview.ExtractingFiltered"] = "正在從輸出 1（濾鏡後）提取幀...",
            ["VpyPreview.FrameRendered"] = "已渲染第 {0} 幀",
            ["VpyPreview.Cancelled"] = "已取消",
            ["VpyPreview.ScriptError"] = "腳本錯誤：{0}",
            ["VpyPreview.PreviewFrameFileMissing"] = "預覽幀檔案缺失",
            ["VpyPreview.LogVspipeOutput"] = "vspipe 輸出 {0}",
            ["VpyPreview.LogVspipeExitCode"] = "vspipe 結束代碼 {0}",
            ["VpyPreview.LogErrorPrefix"] = "錯誤：",
            ["VpyPreview.NoneText"] = "<無>",
            ["VpyPreview.DebugSourceVideo"] = "來源影片：{0}",
            ["VpyPreview.DebugVspipePath"] = "vspipe 路徑：{0}",
            ["VpyPreview.DebugVspipeY4mArg"] = "vspipe y4m 參數：{0}",
            ["VpyPreview.DebugTotalFrames"] = "總幀數：{0}",
            ["VpyPreview.DebugMaxPositionSeconds"] = "最大秒數：{0}",
            ["VpyPreview.DebugPreviewScript"] = "預覽腳本：",
        },
    };

    static VpyPreviewLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["VpyPreview.Ready"] = "Prêt",
            ["VpyPreview.ExtractingSource"] = "Extraction de l'image de sortie 0 (originale)...",
            ["VpyPreview.ExtractingFiltered"] = "Extraction de l'image de sortie 1 (filtrée)...",
            ["VpyPreview.FrameRendered"] = "Image {0} rendue",
            ["VpyPreview.Cancelled"] = "Annulé",
            ["VpyPreview.ScriptError"] = "Erreur de script : {0}",
            ["VpyPreview.PreviewFrameFileMissing"] = "Fichier d'image d'aperçu manquant",
            ["VpyPreview.LogVspipeOutput"] = "sortie vspipe {0}",
            ["VpyPreview.LogVspipeExitCode"] = "code de sortie vspipe {0}",
            ["VpyPreview.LogErrorPrefix"] = "Erreur : ",
            ["VpyPreview.NoneText"] = "<aucun>",
            ["VpyPreview.DebugSourceVideo"] = "Vidéo source : {0}",
            ["VpyPreview.DebugVspipePath"] = "Chemin vspipe : {0}",
            ["VpyPreview.DebugVspipeY4mArg"] = "Argument y4m vspipe : {0}",
            ["VpyPreview.DebugTotalFrames"] = "TotalFrames : {0}",
            ["VpyPreview.DebugMaxPositionSeconds"] = "MaxPositionSeconds : {0}",
            ["VpyPreview.DebugPreviewScript"] = "Script d'aperçu :",
        };
        Data["es"] = new(Data["en"])
        {
            ["VpyPreview.Ready"] = "Listo",
            ["VpyPreview.ExtractingSource"] = "Extrayendo fotograma de la salida 0 (original)...",
            ["VpyPreview.ExtractingFiltered"] = "Extrayendo fotograma de la salida 1 (filtrada)...",
            ["VpyPreview.FrameRendered"] = "Fotograma {0} renderizado",
            ["VpyPreview.Cancelled"] = "Cancelado",
            ["VpyPreview.ScriptError"] = "Error de script: {0}",
            ["VpyPreview.PreviewFrameFileMissing"] = "Falta el archivo de fotograma de vista previa",
            ["VpyPreview.LogVspipeOutput"] = "salida vspipe {0}",
            ["VpyPreview.LogVspipeExitCode"] = "código de salida vspipe {0}",
            ["VpyPreview.LogErrorPrefix"] = "Error: ",
            ["VpyPreview.NoneText"] = "<ninguno>",
            ["VpyPreview.DebugSourceVideo"] = "Vídeo fuente: {0}",
            ["VpyPreview.DebugVspipePath"] = "Ruta de vspipe: {0}",
            ["VpyPreview.DebugVspipeY4mArg"] = "Argumento y4m de vspipe: {0}",
            ["VpyPreview.DebugTotalFrames"] = "TotalFrames: {0}",
            ["VpyPreview.DebugMaxPositionSeconds"] = "MaxPositionSeconds: {0}",
            ["VpyPreview.DebugPreviewScript"] = "Script de vista previa:",
        };
        Data["ja"] = new(Data["en"])
        {
            ["VpyPreview.Ready"] = "準備完了",
            ["VpyPreview.ExtractingSource"] = "出力 0（元）のフレームを抽出しています...",
            ["VpyPreview.ExtractingFiltered"] = "出力 1（フィルタ後）のフレームを抽出しています...",
            ["VpyPreview.FrameRendered"] = "{0} フレームを描画しました",
            ["VpyPreview.Cancelled"] = "キャンセルしました",
            ["VpyPreview.ScriptError"] = "スクリプトエラー: {0}",
            ["VpyPreview.PreviewFrameFileMissing"] = "プレビュー用フレームファイルがありません",
            ["VpyPreview.LogVspipeOutput"] = "vspipe 出力 {0}",
            ["VpyPreview.LogVspipeExitCode"] = "vspipe 終了コード {0}",
            ["VpyPreview.LogErrorPrefix"] = "エラー: ",
            ["VpyPreview.NoneText"] = "<なし>",
            ["VpyPreview.DebugSourceVideo"] = "ソース動画: {0}",
            ["VpyPreview.DebugVspipePath"] = "vspipe パス: {0}",
            ["VpyPreview.DebugVspipeY4mArg"] = "vspipe y4m 引数: {0}",
            ["VpyPreview.DebugTotalFrames"] = "TotalFrames: {0}",
            ["VpyPreview.DebugMaxPositionSeconds"] = "MaxPositionSeconds: {0}",
            ["VpyPreview.DebugPreviewScript"] = "プレビュースクリプト:",
        };
        Data["ru"] = new(Data["en"])
        {
            ["VpyPreview.Ready"] = "Готово",
            ["VpyPreview.ExtractingSource"] = "Извлечение кадра из выхода 0 (оригинал)...",
            ["VpyPreview.ExtractingFiltered"] = "Извлечение кадра из выхода 1 (с фильтром)...",
            ["VpyPreview.FrameRendered"] = "Кадр {0} отрендерен",
            ["VpyPreview.Cancelled"] = "Отменено",
            ["VpyPreview.ScriptError"] = "Ошибка скрипта: {0}",
            ["VpyPreview.PreviewFrameFileMissing"] = "Файл кадра предпросмотра не найден",
            ["VpyPreview.LogVspipeOutput"] = "вывод vspipe {0}",
            ["VpyPreview.LogVspipeExitCode"] = "код выхода vspipe {0}",
            ["VpyPreview.LogErrorPrefix"] = "Ошибка: ",
            ["VpyPreview.NoneText"] = "<нет>",
            ["VpyPreview.DebugSourceVideo"] = "Исходное видео: {0}",
            ["VpyPreview.DebugVspipePath"] = "Путь vspipe: {0}",
            ["VpyPreview.DebugVspipeY4mArg"] = "Аргумент vspipe y4m: {0}",
            ["VpyPreview.DebugTotalFrames"] = "TotalFrames: {0}",
            ["VpyPreview.DebugMaxPositionSeconds"] = "MaxPositionSeconds: {0}",
            ["VpyPreview.DebugPreviewScript"] = "Скрипт предпросмотра:",
        };
        Data["de"] = new(Data["en"])
        {
            ["VpyPreview.Ready"] = "Bereit",
            ["VpyPreview.ExtractingSource"] = "Bild aus Ausgabe 0 (Original) wird extrahiert...",
            ["VpyPreview.ExtractingFiltered"] = "Bild aus Ausgabe 1 (gefiltert) wird extrahiert...",
            ["VpyPreview.FrameRendered"] = "Frame {0} gerendert",
            ["VpyPreview.Cancelled"] = "Abgebrochen",
            ["VpyPreview.ScriptError"] = "Skriptfehler: {0}",
            ["VpyPreview.PreviewFrameFileMissing"] = "Vorschau-Frame-Datei fehlt",
            ["VpyPreview.LogVspipeOutput"] = "vspipe-Ausgabe {0}",
            ["VpyPreview.LogVspipeExitCode"] = "vspipe-Exitcode {0}",
            ["VpyPreview.LogErrorPrefix"] = "Fehler: ",
            ["VpyPreview.NoneText"] = "<keine>",
            ["VpyPreview.DebugSourceVideo"] = "Quelldatei: {0}",
            ["VpyPreview.DebugVspipePath"] = "vspipe-Pfad: {0}",
            ["VpyPreview.DebugVspipeY4mArg"] = "vspipe-y4m-Argument: {0}",
            ["VpyPreview.DebugTotalFrames"] = "TotalFrames: {0}",
            ["VpyPreview.DebugMaxPositionSeconds"] = "MaxPositionSeconds: {0}",
            ["VpyPreview.DebugPreviewScript"] = "Vorschau-Skript:",
        };
        Data["ko"] = new(Data["en"])
        {
            ["VpyPreview.Ready"] = "준비됨",
            ["VpyPreview.ExtractingSource"] = "출력 0(원본)에서 프레임을 추출하는 중...",
            ["VpyPreview.ExtractingFiltered"] = "출력 1(필터됨)에서 프레임을 추출하는 중...",
            ["VpyPreview.FrameRendered"] = "프레임 {0} 렌더링 완료",
            ["VpyPreview.Cancelled"] = "취소됨",
            ["VpyPreview.ScriptError"] = "스크립트 오류: {0}",
            ["VpyPreview.PreviewFrameFileMissing"] = "미리보기 프레임 파일이 없습니다",
            ["VpyPreview.LogVspipeOutput"] = "vspipe 출력 {0}",
            ["VpyPreview.LogVspipeExitCode"] = "vspipe 종료 코드 {0}",
            ["VpyPreview.LogErrorPrefix"] = "오류: ",
            ["VpyPreview.NoneText"] = "<없음>",
            ["VpyPreview.DebugSourceVideo"] = "원본 비디오: {0}",
            ["VpyPreview.DebugVspipePath"] = "vspipe 경로: {0}",
            ["VpyPreview.DebugVspipeY4mArg"] = "vspipe y4m 인수: {0}",
            ["VpyPreview.DebugTotalFrames"] = "TotalFrames: {0}",
            ["VpyPreview.DebugMaxPositionSeconds"] = "MaxPositionSeconds: {0}",
            ["VpyPreview.DebugPreviewScript"] = "미리보기 스크립트:",
        };
        Data["pt-br"] = new(Data["en"])
        {
            ["VpyPreview.Ready"] = "Pronto",
            ["VpyPreview.ExtractingSource"] = "Extraindo quadro da saída 0 (original)...",
            ["VpyPreview.ExtractingFiltered"] = "Extraindo quadro da saída 1 (filtrada)...",
            ["VpyPreview.FrameRendered"] = "Quadro {0} renderizado",
            ["VpyPreview.Cancelled"] = "Cancelado",
            ["VpyPreview.ScriptError"] = "Erro de script: {0}",
            ["VpyPreview.PreviewFrameFileMissing"] = "Arquivo de quadro da prévia ausente",
            ["VpyPreview.LogVspipeOutput"] = "saída do vspipe {0}",
            ["VpyPreview.LogVspipeExitCode"] = "código de saída do vspipe {0}",
            ["VpyPreview.LogErrorPrefix"] = "Erro: ",
            ["VpyPreview.NoneText"] = "<nenhum>",
            ["VpyPreview.DebugSourceVideo"] = "Vídeo de origem: {0}",
            ["VpyPreview.DebugVspipePath"] = "Caminho do vspipe: {0}",
            ["VpyPreview.DebugVspipeY4mArg"] = "Argumento y4m do vspipe: {0}",
            ["VpyPreview.DebugTotalFrames"] = "TotalFrames: {0}",
            ["VpyPreview.DebugMaxPositionSeconds"] = "MaxPositionSeconds: {0}",
            ["VpyPreview.DebugPreviewScript"] = "Script de prévia:",
        };
    }

    public static VpyPreviewLangProvider Current => new(UILangProvider.Current.LanguageCode);

    public string StatusReady => this["VpyPreview.Ready"];
    public string StatusExtractingSource => this["VpyPreview.ExtractingSource"];
    public string StatusExtractingFiltered => this["VpyPreview.ExtractingFiltered"];
    public string StatusFrameRendered => this["VpyPreview.FrameRendered"];
    public string StatusCancelled => this["VpyPreview.Cancelled"];
    public string StatusScriptError => this["VpyPreview.ScriptError"];
    public string PreviewFrameFileMissing => this["VpyPreview.PreviewFrameFileMissing"];
    public string LogVspipeOutput => this["VpyPreview.LogVspipeOutput"];
    public string LogVspipeExitCode => this["VpyPreview.LogVspipeExitCode"];
    public string LogErrorPrefix => this["VpyPreview.LogErrorPrefix"];
    public string NoneText => this["VpyPreview.NoneText"];
    public string DebugSourceVideo => this["VpyPreview.DebugSourceVideo"];
    public string DebugVspipePath => this["VpyPreview.DebugVspipePath"];
    public string DebugVspipeY4mArg => this["VpyPreview.DebugVspipeY4mArg"];
    public string DebugTotalFrames => this["VpyPreview.DebugTotalFrames"];
    public string DebugMaxPositionSeconds => this["VpyPreview.DebugMaxPositionSeconds"];
    public string DebugPreviewScript => this["VpyPreview.DebugPreviewScript"];
}
