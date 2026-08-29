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
            ["MuxTracks.Language"] = "Lang:",
            ["MuxTracks.Primary"] = "Primary track",
            ["MuxTracks.Forced"] = "Forced",
            ["MuxTracks.Empty"] = "No tracks added",
            ["MuxTracks.InvalidSync"] = "Sync must be a whole number of milliseconds.",
            ["MuxTracks.FileFilter"] = "Subtitle files|*.ass;*.srt;*.ssa;*.vtt|All files (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Add Subtitles",
            ["Hint.CannotDeleteSrcSubs"] = "Source subtitle exclusion feature is unplanned",
            ["MuxTracks.CannotMuxSubtitle"] = "Cannot mux this subtitle.",
            ["MuxTracks.MissingFfmpeg"] = "ffmpeg is required before subtitles can be muxed.",
            ["MuxTracks.MissingFfprobe"] = "ffprobe is required before subtitles can be muxed.",
            ["MuxTracks.DuplicateSourcePaths"] = "The selected source list contains duplicate paths.",
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
            ["MuxTracks.Language"] = "语言：",
            ["MuxTracks.Primary"] = "主轨道",
            ["MuxTracks.Forced"] = "强制显示",
            ["MuxTracks.Empty"] = "未添加轨道",
            ["MuxTracks.InvalidSync"] = "同步偏移值必须是整数毫秒。",
            ["MuxTracks.FileFilter"] = "字幕文件|*.ass;*.srt;*.ssa;*.vtt|All files (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "添加字幕轨道",
            ["Hint.CannotDeleteSrcSubs"] = "源字幕排除功能暂无实现计划",
            ["MuxTracks.CannotMuxSubtitle"] = "无法封装此字幕。",
            ["MuxTracks.MissingFfmpeg"] = "需要 ffmpeg 才能封装字幕。",
            ["MuxTracks.MissingFfprobe"] = "需要 ffprobe 才能检查字幕轨道。",
            ["MuxTracks.DuplicateSourcePaths"] = "所选源列表包含重复路径，无法封装此字幕。",
        }) Data["zh-cn"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "隊列源",
            ["MuxTracks.SubtitleHeader"] = "字幕",
            ["MuxTracks.AddSubtitle"] = "添加字幕",
            ["MuxTracks.Sync"] = "同步（±毫秒）",
            ["MuxTracks.Language"] = "語言：",
            ["MuxTracks.Primary"] = "主軌道",
            ["MuxTracks.Forced"] = "強制顯示",
            ["MuxTracks.Empty"] = "未添加軌道",
            ["MuxTracks.InvalidSync"] = "同步偏移值必須是整數毫秒。",
            ["MuxTracks.FileFilter"] = "字幕文件|*.ass;*.srt;*.ssa;*.vtt|All files (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "添加字幕軌道",
            ["Hint.CannotDeleteSrcSubs"] = "源字幕排除功能暫無實現計劃",
            ["MuxTracks.CannotMuxSubtitle"] = "無法封裝此字幕。",
            ["MuxTracks.MissingFfmpeg"] = "需要 ffmpeg 才能封裝字幕。",
            ["MuxTracks.MissingFfprobe"] = "需要 ffprobe 才能檢查字幕軌道。",
            ["MuxTracks.DuplicateSourcePaths"] = "所選源列表包含重複路徑，無法封裝此字幕。",
        }) Data["zh-tw"][pair.Key] = pair.Value;

        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Sources de file",
            ["MuxTracks.SubtitleHeader"] = "Ajout de piste de sous-titres",
            ["MuxTracks.AddSubtitle"] = "Ajouter sous-titre",
            ["MuxTracks.Sync"] = "Sync (±ms)",
            ["MuxTracks.Language"] = "Langue:",
            ["MuxTracks.Primary"] = "Piste principale",
            ["MuxTracks.Empty"] = "Aucune piste ajoutée",
            ["MuxTracks.InvalidSync"] = "Le décalage de synchro doit être un entier en millisecondes.",
            ["MuxTracks.FileFilter"] = "Fichiers de sous-titres|*.ass;*.srt;*.ssa;*.vtt|Tous les fichiers (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Ajouter sous-titres",
            ["Hint.CannotDeleteSrcSubs"] = "La fonctionnalité d’exclusion des sous-titres source n’est pas prévue pour le moment",
            ["MuxTracks.CannotMuxSubtitle"] = "Impossible de muxer ce sous-titre.",
            ["MuxTracks.MissingFfmpeg"] = "ffmpeg est requis pour muxer les sous-titres.",
            ["MuxTracks.MissingFfprobe"] = "ffprobe est requis pour inspecter les pistes de sous-titres.",
            ["MuxTracks.DuplicateSourcePaths"] = "La liste de sources sélectionnée contient des chemins en double.",
        }) Data["fr"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Fuentes de cola",
            ["MuxTracks.SubtitleHeader"] = "Subtítulos",
            ["MuxTracks.AddSubtitle"] = "Añadir subtítulo",
            ["MuxTracks.Sync"] = "Sincronización (±ms)",
            ["MuxTracks.Language"] = "Idioma:",
            ["MuxTracks.Primary"] = "Pista principal",
            ["MuxTracks.Empty"] = "No hay pistas añadidas",
            ["MuxTracks.InvalidSync"] = "El desplazamiento de sincronización debe ser un número entero de milisegundos.",
            ["MuxTracks.FileFilter"] = "Archivos de subtítulos|*.ass;*.srt;*.ssa;*.vtt|Todos los archivos (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Añadir subtítulos",
            ["Hint.CannotDeleteSrcSubs"] = "La función para excluir subtítulos de origen no está prevista por ahora",
            ["MuxTracks.CannotMuxSubtitle"] = "No se puede muxear este subtítulo.",
            ["MuxTracks.MissingFfmpeg"] = "Se requiere ffmpeg para muxear subtítulos.",
            ["MuxTracks.MissingFfprobe"] = "Se requiere ffprobe para inspeccionar las pistas de subtítulos.",
            ["MuxTracks.DuplicateSourcePaths"] = "La lista de origen seleccionada contiene rutas duplicadas.",
        }) Data["es"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "キューのソース",
            ["MuxTracks.SubtitleHeader"] = "字幕",
            ["MuxTracks.AddSubtitle"] = "字幕を追加",
            ["MuxTracks.Sync"] = "同期（±ms）",
            ["MuxTracks.Language"] = "言語：",
            ["MuxTracks.Primary"] = "メイントラック",
            ["MuxTracks.Empty"] = "トラックは追加されていません",
            ["MuxTracks.InvalidSync"] = "同期オフセットはミリ秒単位の整数で指定してください。",
            ["MuxTracks.FileFilter"] = "字幕ファイル|*.ass;*.srt;*.ssa;*.vtt|すべてのファイル (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "字幕を追加",
            ["Hint.CannotDeleteSrcSubs"] = "ソース字幕の除外機能は現在実装予定がありません",
            ["MuxTracks.CannotMuxSubtitle"] = "この字幕は mux できません。",
            ["MuxTracks.MissingFfmpeg"] = "字幕を mux するには ffmpeg が必要です。",
            ["MuxTracks.MissingFfprobe"] = "字幕トラックを確認するには ffprobe が必要です。",
            ["MuxTracks.DuplicateSourcePaths"] = "選択されたソース一覧に重複したパスがあります。",
        }) Data["ja"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Источники очереди",
            ["MuxTracks.SubtitleHeader"] = "Субтитров",
            ["MuxTracks.AddSubtitle"] = "Добавить субтитры",
            ["MuxTracks.Sync"] = "Синхронизация (±мс)",
            ["MuxTracks.Language"] = "Язык:",
            ["MuxTracks.Primary"] = "Основная дорожка",
            ["MuxTracks.Empty"] = "Дорожки не добавлены",
            ["MuxTracks.InvalidSync"] = "Смещение синхронизации должно быть целым числом миллисекунд.",
            ["MuxTracks.FileFilter"] = "Файлы субтитров|*.ass;*.srt;*.ssa;*.vtt|Все файлы (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Добавить субтитры",
            ["Hint.CannotDeleteSrcSubs"] = "Функция исключения исходных субтитров пока не планируется",
            ["MuxTracks.CannotMuxSubtitle"] = "Невозможно mux-ить этот субтитр.",
            ["MuxTracks.MissingFfmpeg"] = "Для mux субтитров требуется ffmpeg.",
            ["MuxTracks.MissingFfprobe"] = "Для проверки дорожек субтитров требуется ffprobe.",
            ["MuxTracks.DuplicateSourcePaths"] = "В выбранном списке источников есть дублирующиеся пути.",
        }) Data["ru"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Quellen der Warteschlange",
            ["MuxTracks.SubtitleHeader"] = "Untertitelspur",
            ["MuxTracks.AddSubtitle"] = "Untertitel hinzufügen",
            ["MuxTracks.Sync"] = "Sync (±ms)",
            ["MuxTracks.Language"] = "Sprache:",
            ["MuxTracks.Primary"] = "Hauptspur",
            ["MuxTracks.Empty"] = "Keine Spuren hinzugefügt",
            ["MuxTracks.InvalidSync"] = "Die Sync-Verschiebung muss eine ganze Millisekundenzahl sein.",
            ["MuxTracks.FileFilter"] = "Untertiteldateien|*.ass;*.srt;*.ssa;*.vtt|Alle Dateien (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Untertitel hinzufügen",
            ["Hint.CannotDeleteSrcSubs"] = "Eine Funktion zum Ausschließen von Quelluntertiteln ist derzeit nicht geplant",
            ["MuxTracks.CannotMuxSubtitle"] = "Dieser Untertitel kann nicht gemuxt werden.",
            ["MuxTracks.MissingFfmpeg"] = "ffmpeg wird benötigt, um Untertitel zu muxen.",
            ["MuxTracks.MissingFfprobe"] = "ffprobe wird benötigt, um Untertitelspuren zu prüfen.",
            ["MuxTracks.DuplicateSourcePaths"] = "Die ausgewählte Quellenliste enthält doppelte Pfade.",
        }) Data["de"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "대기열 소스",
            ["MuxTracks.SubtitleHeader"] = "자막 트랙",
            ["MuxTracks.AddSubtitle"] = "자막 추가",
            ["MuxTracks.Sync"] = "동기화 (±ms)",
            ["MuxTracks.Language"] = "언어：",
            ["MuxTracks.Primary"] = "기본 트랙",
            ["MuxTracks.Empty"] = "추가된 트랙이 없습니다",
            ["MuxTracks.InvalidSync"] = "동기화 오프셋은 정수 밀리초여야 합니다.",
            ["MuxTracks.FileFilter"] = "자막 파일|*.ass;*.srt;*.ssa;*.vtt|모든 파일 (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "자막 추가",
            ["Hint.CannotDeleteSrcSubs"] = "소스 자막 제외 기능은 현재 구현 계획이 없습니다",
            ["MuxTracks.CannotMuxSubtitle"] = "이 자막은 mux 할 수 없습니다.",
            ["MuxTracks.MissingFfmpeg"] = "자막을 mux 하려면 ffmpeg가 필요합니다.",
            ["MuxTracks.MissingFfprobe"] = "자막 트랙을 확인하려면 ffprobe가 필요합니다.",
            ["MuxTracks.DuplicateSourcePaths"] = "선택한 소스 목록에 중복된 경로가 있습니다.",
        }) Data["ko"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Fontes da fila",
            ["MuxTracks.SubtitleHeader"] = "Faixa de legenda",
            ["MuxTracks.AddSubtitle"] = "Adicionar legenda",
            ["MuxTracks.Sync"] = "Sincronia (±ms)",
            ["MuxTracks.Language"] = "Idioma:",
            ["MuxTracks.Primary"] = "Faixa principal",
            ["MuxTracks.Empty"] = "Nenhuma faixa adicionada",
            ["MuxTracks.InvalidSync"] = "O deslocamento de sincronização deve ser um número inteiro de milissegundos.",
            ["MuxTracks.FileFilter"] = "Arquivos de legenda|*.ass;*.srt;*.ssa;*.vtt|Todos os arquivos (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Adicionar legendas",
            ["Hint.CannotDeleteSrcSubs"] = "O recurso de exclusão de legendas de origem não está planejado no momento",
            ["MuxTracks.CannotMuxSubtitle"] = "Não é possível muxar esta legenda.",
            ["MuxTracks.MissingFfmpeg"] = "ffmpeg é necessário para muxar legendas.",
            ["MuxTracks.MissingFfprobe"] = "ffprobe é necessário para inspecionar as faixas de legenda.",
            ["MuxTracks.DuplicateSourcePaths"] = "A lista de origens selecionada contém caminhos duplicados.",
        }) Data["pt-br"][pair.Key] = pair.Value;
    }

    public static MuxLangProvider Current => new(UILangProvider.Current.LanguageCode);
}
