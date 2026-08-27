namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Localized strings for the external subtitle track editor.
/// </summary>
public sealed class MuxLangProvider(string languageCode) : LangProviderBase(languageCode, Data)
{
    public const string WindowTitle = "Add Subtitles";
    public const string DurationUnknown = "N/A";

    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["MuxTracks.QueueSources"] = "Queue Sources",
            ["MuxTracks.SubtitleHeader"] = "Subtitle Tracks",
            ["MuxTracks.AddSubtitle"] = "Add Subtitle",
            ["MuxTracks.Sync"] = "Sync (±ms)",
            ["MuxTracks.Primary"] = "Primary track",
            ["MuxTracks.Forced"] = "Forced",
            ["MuxTracks.Empty"] = "No tracks added",
            ["MuxTracks.InvalidSync"] = "Sync must be a whole number of milliseconds.",
            ["MuxTracks.FileFilter"] = "Subtitle files|*.ass;*.srt;*.ssa;*.sub;*.sup|All files (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Add Subtitles",
            ["Hint.CannotDeleteSrcSubs"] = "Source subtitle exclusion feature is unplanned",
        },
    };

    static MuxLangProvider()
    {
        foreach (string code in new[] { "zh-cn", "zh-tw", "fr", "es", "ja", "ru", "de", "ko", "pt-br" })
            Data[code] = new(Data["en"]);

        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "队列源",
            ["MuxTracks.SubtitleHeader"] = "字幕",
            ["MuxTracks.AddSubtitle"] = "添加字幕",
            ["MuxTracks.Sync"] = "同步（±毫秒）",
            ["MuxTracks.Primary"] = "主轨道",
            ["MuxTracks.Forced"] = "强制显示",
            ["MuxTracks.Empty"] = "未添加轨道",
            ["MuxTracks.InvalidSync"] = "同步偏移值必须是整数毫秒。",
            ["MuxTracks.FileFilter"] = "字幕文件|*.ass;*.srt;*.ssa;*.sub;*.sup|All files (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "添加字幕轨道",
            ["Hint.CannotDeleteSrcSubs"] = "源字幕排除功能暂无实现计划",
        }) Data["zh-cn"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "隊列源",
            ["MuxTracks.SubtitleHeader"] = "字幕",
            ["MuxTracks.AddSubtitle"] = "添加字幕",
            ["MuxTracks.Sync"] = "同步（±毫秒）",
            ["MuxTracks.Primary"] = "主軌道",
            ["MuxTracks.Forced"] = "強制顯示",
            ["MuxTracks.Empty"] = "未添加軌道",
            ["MuxTracks.InvalidSync"] = "同步偏移值必須是整數毫秒。",
            ["MuxTracks.FileFilter"] = "字幕文件|*.ass;*.srt;*.ssa;*.sub;*.sup|All files (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "添加字幕軌道",
            ["Hint.CannotDeleteSrcSubs"] = "源字幕排除功能暫無實現計劃",
        }) Data["zh-tw"][pair.Key] = pair.Value;

        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Sources de file",
            ["MuxTracks.SubtitleHeader"] = "Ajout de piste de sous-titres",
            ["MuxTracks.AddSubtitle"] = "Ajouter sous-titre",
            ["MuxTracks.Sync"] = "Sync (±ms)",
            ["MuxTracks.Primary"] = "Piste principale",
            ["MuxTracks.Empty"] = "Aucune piste ajoutée",
            ["MuxTracks.InvalidSync"] = "Le décalage de synchro doit être un entier en millisecondes.",
            ["MuxTracks.FileFilter"] = "Fichiers de sous-titres|*.ass;*.srt;*.ssa;*.sub;*.sup|Tous les fichiers (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Ajouter sous-titres",
            ["Hint.CannotDeleteSrcSubs"] = "La fonctionnalité d’exclusion des sous-titres source n’est pas prévue pour le moment",
        }) Data["fr"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Fuentes de cola",
            ["MuxTracks.SubtitleHeader"] = "Subtítulos",
            ["MuxTracks.AddSubtitle"] = "Añadir subtítulo",
            ["MuxTracks.Sync"] = "Sincronización (±ms)",
            ["MuxTracks.Primary"] = "Pista principal",
            ["MuxTracks.Empty"] = "No hay pistas añadidas",
            ["MuxTracks.InvalidSync"] = "El desplazamiento de sincronización debe ser un número entero de milisegundos.",
            ["MuxTracks.FileFilter"] = "Archivos de subtítulos|*.ass;*.srt;*.ssa;*.sub;*.sup|Todos los archivos (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Añadir subtítulos",
            ["Hint.CannotDeleteSrcSubs"] = "La función para excluir subtítulos de origen no está prevista por ahora",
        }) Data["es"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "キューのソース",
            ["MuxTracks.SubtitleHeader"] = "字幕",
            ["MuxTracks.AddSubtitle"] = "字幕を追加",
            ["MuxTracks.Sync"] = "同期 (±ms)",
            ["MuxTracks.Primary"] = "メイントラック",
            ["MuxTracks.Empty"] = "トラックは追加されていません",
            ["MuxTracks.InvalidSync"] = "同期オフセットはミリ秒単位の整数で指定してください。",
            ["MuxTracks.FileFilter"] = "字幕ファイル|*.ass;*.srt;*.ssa;*.sub;*.sup|すべてのファイル (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "字幕を追加",
            ["Hint.CannotDeleteSrcSubs"] = "ソース字幕の除外機能は現在実装予定がありません",
        }) Data["ja"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Источники очереди",
            ["MuxTracks.SubtitleHeader"] = "Субтитров",
            ["MuxTracks.AddSubtitle"] = "Добавить субтитры",
            ["MuxTracks.Sync"] = "Синхронизация (±мс)",
            ["MuxTracks.Primary"] = "Основная дорожка",
            ["MuxTracks.Empty"] = "Дорожки не добавлены",
            ["MuxTracks.InvalidSync"] = "Смещение синхронизации должно быть целым числом миллисекунд.",
            ["MuxTracks.FileFilter"] = "Файлы субтитров|*.ass;*.srt;*.ssa;*.sub;*.sup|Все файлы (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Добавить субтитры",
            ["Hint.CannotDeleteSrcSubs"] = "Функция исключения исходных субтитров пока не планируется",
        }) Data["ru"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Quellen der Warteschlange",
            ["MuxTracks.SubtitleHeader"] = "Untertitelspur",
            ["MuxTracks.AddSubtitle"] = "Untertitel hinzufügen",
            ["MuxTracks.Sync"] = "Sync (±ms)",
            ["MuxTracks.Primary"] = "Hauptspur",
            ["MuxTracks.Empty"] = "Keine Spuren hinzugefügt",
            ["MuxTracks.InvalidSync"] = "Die Sync-Verschiebung muss eine ganze Millisekundenzahl sein.",
            ["MuxTracks.FileFilter"] = "Untertiteldateien|*.ass;*.srt;*.ssa;*.sub;*.sup|Alle Dateien (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Untertitel hinzufügen",
            ["Hint.CannotDeleteSrcSubs"] = "Eine Funktion zum Ausschließen von Quelluntertiteln ist derzeit nicht geplant",
        }) Data["de"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "대기열 소스",
            ["MuxTracks.SubtitleHeader"] = "자막 트랙",
            ["MuxTracks.AddSubtitle"] = "자막 추가",
            ["MuxTracks.Sync"] = "동기화 (±ms)",
            ["MuxTracks.Primary"] = "기본 트랙",
            ["MuxTracks.Empty"] = "추가된 트랙이 없습니다",
            ["MuxTracks.InvalidSync"] = "동기화 오프셋은 정수 밀리초여야 합니다.",
            ["MuxTracks.FileFilter"] = "자막 파일|*.ass;*.srt;*.ssa;*.sub;*.sup|모든 파일 (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "자막 추가",
            ["Hint.CannotDeleteSrcSubs"] = "소스 자막 제외 기능은 현재 구현 계획이 없습니다",
        }) Data["ko"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Fontes da fila",
            ["MuxTracks.SubtitleHeader"] = "Faixa de legenda",
            ["MuxTracks.AddSubtitle"] = "Adicionar legenda",
            ["MuxTracks.Sync"] = "Sincronia (±ms)",
            ["MuxTracks.Primary"] = "Faixa principal",
            ["MuxTracks.Empty"] = "Nenhuma faixa adicionada",
            ["MuxTracks.InvalidSync"] = "O deslocamento de sincronização deve ser um número inteiro de milissegundos.",
            ["MuxTracks.FileFilter"] = "Arquivos de legenda|*.ass;*.srt;*.ssa;*.sub;*.sup|Todos os arquivos (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Adicionar legendas",
            ["Hint.CannotDeleteSrcSubs"] = "O recurso de exclusão de legendas de origem não está planejado no momento",
        }) Data["pt-br"][pair.Key] = pair.Value;
    }

    public static MuxLangProvider Current => new(UILangProvider.Current.LanguageCode);
}
