namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Localized strings for the external subtitle track editor.
/// </summary>
public sealed class MuxLangProvider(string languageCode) : LangProviderBase(languageCode, Data)
{
    public const string WindowTitle = "Add Subtitles";
    public static string DurationUnknown => NAText; // LangProviderBase.NAText

    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["MuxTracks.QueueSources"] = "Queue Sources",
            ["MuxTracks.SubtitleHeader"] = "Subtitle Tracks",
            ["MuxTracks.AddSubtitle"] = "Add Subtitle",
            ["MuxTracks.Language"] = "Lang",
            ["MuxTracks.Primary"] = "Default track",
            ["MuxTracks.Forced"] = "Forced",
            ["MuxTracks.Empty"] = "No tracks added",
            ["MuxTracks.FileFilter"] = "Subtitle files|*.ass;*.srt;*.ssa;*.vtt|All files (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Add Subtitles",
            ["Hint.CannotDeleteSrcSubs"] = "Source subtitle exclusion feature is unplanned",
            ["Hint.FFmpegSubtitleDefault"] = "When no subtitle is default & muxing multiple subtitles, ffmpeg will mark the 1st as default",
            ["MuxTracks.CannotMuxSubtitle"] = "Cannot mux this subtitle.",
            ["MuxTracks.MissingFfmpeg"] = "ffmpeg is required before subtitles can be muxed.",
            ["MuxTracks.MissingFfprobe"] = "ffprobe is required before subtitles can be muxed.",
            ["MuxTracks.DuplicateSourcePaths"] = "The selected source list contains duplicate paths.",
            ["MuxTracks.NoDefault.SourceLine"] = "Source: {0}",
            ["MuxTracks.NoDefault.NoDefaultMarked"] = "No subtitle is marked default.",
            ["MuxTracks.NoDefault.OrigDefTrackId"] = "Original source default subtitle track ID: {0}",
            ["MuxTracks.NoDefault.FirstSubTrackId"] = "First source subtitle track ID: {0}",
            ["MuxTracks.NoDefault.NoSourceSubs"] = "No source subtitle tracks were found.",
            ["MuxTracks.NoDefault.ClearAndContinue"] = "Clear default markings and continue?",
            ["MuxTracks.NoDefault.ConsiderAdding"] = "Consider adding a default subtitle before continuing?",
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
            ["MuxTracks.Language"] = "语言",
            ["MuxTracks.Primary"] = "默认轨道",
            ["MuxTracks.Forced"] = "强制显示",
            ["MuxTracks.Empty"] = "未添加轨道",
            ["MuxTracks.FileFilter"] = "字幕文件|*.ass;*.srt;*.ssa;*.vtt|All files (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "添加字幕轨道",
            ["Hint.CannotDeleteSrcSubs"] = "源字幕排除功能暂无实现计划",
            ["Hint.FFmpegSubtitleDefault"] = "未标记默认字幕且封装输出多条字幕时，ffmpeg 会标记第一条字幕为默认",
            ["MuxTracks.CannotMuxSubtitle"] = "无法封装此字幕。",
            ["MuxTracks.MissingFfmpeg"] = "需要 ffmpeg 才能封装字幕。",
            ["MuxTracks.MissingFfprobe"] = "需要 ffprobe 才能检查字幕轨道。",
            ["MuxTracks.DuplicateSourcePaths"] = "所选源列表包含重复路径，无法封装此字幕。",
            ["MuxTracks.NoDefault.SourceLine"] = "源：{0}",
            ["MuxTracks.NoDefault.NoDefaultMarked"] = "未标记默认字幕。",
            ["MuxTracks.NoDefault.OrigDefTrackId"] = "原始源默认字幕轨道 ID：{0}",
            ["MuxTracks.NoDefault.FirstSubTrackId"] = "首个源字幕轨道 ID：{0}",
            ["MuxTracks.NoDefault.NoSourceSubs"] = "未找到源字幕轨道。",
            ["MuxTracks.NoDefault.ClearAndContinue"] = "清除默认标记并继续？",
            ["MuxTracks.NoDefault.ConsiderAdding"] = "建议在继续前添加默认字幕？",
        }) Data["zh-cn"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "隊列源",
            ["MuxTracks.SubtitleHeader"] = "字幕",
            ["MuxTracks.AddSubtitle"] = "添加字幕",
            ["MuxTracks.Language"] = "語言",
            ["MuxTracks.Primary"] = "默認軌道",
            ["MuxTracks.Forced"] = "強制顯示",
            ["MuxTracks.Empty"] = "未添加軌道",
            ["MuxTracks.FileFilter"] = "字幕文件|*.ass;*.srt;*.ssa;*.vtt|All files (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "添加字幕軌道",
            ["Hint.CannotDeleteSrcSubs"] = "源字幕排除功能暫無實現計劃",
            ["Hint.FFmpegSubtitleDefault"] = "未標記默認字幕且封裝輸出多條字幕時，ffmpeg 會標記第一條字幕為默認",
            ["MuxTracks.CannotMuxSubtitle"] = "無法封裝此字幕。",
            ["MuxTracks.MissingFfmpeg"] = "需要 ffmpeg 才能封裝字幕。",
            ["MuxTracks.MissingFfprobe"] = "需要 ffprobe 才能檢查字幕軌道。",
            ["MuxTracks.DuplicateSourcePaths"] = "所選源列表包含重複路徑，無法封裝此字幕。",
            ["MuxTracks.NoDefault.SourceLine"] = "源：{0}",
            ["MuxTracks.NoDefault.NoDefaultMarked"] = "未標記預設字幕。",
            ["MuxTracks.NoDefault.OrigDefTrackId"] = "原始源預設字幕軌道 ID：{0}",
            ["MuxTracks.NoDefault.FirstSubTrackId"] = "首個源字幕軌道 ID：{0}",
            ["MuxTracks.NoDefault.NoSourceSubs"] = "未找到源字幕軌道。",
            ["MuxTracks.NoDefault.ClearAndContinue"] = "清除預設標記並繼續？",
            ["MuxTracks.NoDefault.ConsiderAdding"] = "建議在繼續前新增預設字幕？",
        }) Data["zh-tw"][pair.Key] = pair.Value;

        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Sources de file",
            ["MuxTracks.SubtitleHeader"] = "Ajout de piste de sous-titres",
            ["MuxTracks.AddSubtitle"] = "Ajouter sous-titre",
            ["MuxTracks.Language"] = "Langue",
            ["MuxTracks.Primary"] = "Piste par défaut",
            ["MuxTracks.Empty"] = "Aucune piste ajoutée",
            ["MuxTracks.FileFilter"] = "Fichiers de sous-titres|*.ass;*.srt;*.ssa;*.vtt|Tous les fichiers (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Ajouter sous-titres",
            ["Hint.CannotDeleteSrcSubs"] = "La fonctionnalité d’exclusion des sous-titres source n’est pas prévue pour le moment",
            ["Hint.FFmpegSubtitleDefault"] = "Aucun sous-titre par défaut et multiplexage multiple, ffmpeg marque le 1er comme défaut",
            ["MuxTracks.CannotMuxSubtitle"] = "Impossible de muxer ce sous-titre.",
            ["MuxTracks.MissingFfmpeg"] = "ffmpeg est requis pour muxer les sous-titres.",
            ["MuxTracks.MissingFfprobe"] = "ffprobe est requis pour inspecter les pistes de sous-titres.",
            ["MuxTracks.DuplicateSourcePaths"] = "La liste de sources sélectionnée contient des chemins en double.",
            ["MuxTracks.NoDefault.SourceLine"] = "Source : {0}",
            ["MuxTracks.NoDefault.NoDefaultMarked"] = "Aucun sous-titre n'est marqué par défaut.",
            ["MuxTracks.NoDefault.OrigDefTrackId"] = "ID de piste de sous-titre par défaut de la source d'origine : {0}",
            ["MuxTracks.NoDefault.FirstSubTrackId"] = "ID de la première piste de sous-titre source : {0}",
            ["MuxTracks.NoDefault.NoSourceSubs"] = "Aucune piste de sous-titre source trouvée.",
            ["MuxTracks.NoDefault.ClearAndContinue"] = "Effacer les marquages par défaut et continuer ?",
            ["MuxTracks.NoDefault.ConsiderAdding"] = "Envisager d'ajouter un sous-titre par défaut avant de continuer ?",
        }) Data["fr"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Fuentes de cola",
            ["MuxTracks.SubtitleHeader"] = "Subtítulos",
            ["MuxTracks.AddSubtitle"] = "Añadir subtítulo",
            ["MuxTracks.Language"] = "Idioma",
            ["MuxTracks.Primary"] = "Pista predeterminada",
            ["MuxTracks.Empty"] = "No hay pistas añadidas",
            ["MuxTracks.FileFilter"] = "Archivos de subtítulos|*.ass;*.srt;*.ssa;*.vtt|Todos los archivos (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Añadir subtítulos",
            ["Hint.CannotDeleteSrcSubs"] = "La función para excluir subtítulos de origen no está prevista por ahora",
            ["Hint.FFmpegSubtitleDefault"] = "Sin subtítulo predeterminado y multiplexando varios, ffmpeg marca el primero como predeterminado",
            ["MuxTracks.CannotMuxSubtitle"] = "No se puede muxear este subtítulo.",
            ["MuxTracks.MissingFfmpeg"] = "Se requiere ffmpeg para muxear subtítulos.",
            ["MuxTracks.MissingFfprobe"] = "Se requiere ffprobe para inspeccionar las pistas de subtítulos.",
            ["MuxTracks.DuplicateSourcePaths"] = "La lista de origen seleccionada contiene rutas duplicadas.",
            ["MuxTracks.NoDefault.SourceLine"] = "Origen: {0}",
            ["MuxTracks.NoDefault.NoDefaultMarked"] = "Ningún subtítulo está marcado como predeterminado.",
            ["MuxTracks.NoDefault.OrigDefTrackId"] = "ID de pista de subtítulo predeterminado de la fuente original: {0}",
            ["MuxTracks.NoDefault.FirstSubTrackId"] = "ID de la primera pista de subtítulo de la fuente: {0}",
            ["MuxTracks.NoDefault.NoSourceSubs"] = "No se encontraron pistas de subtítulos de la fuente.",
            ["MuxTracks.NoDefault.ClearAndContinue"] = "¿Borrar marcas predeterminadas y continuar?",
            ["MuxTracks.NoDefault.ConsiderAdding"] = "¿Considerar añadir un subtítulo predeterminado antes de continuar?",
        }) Data["es"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "キューのソース",
            ["MuxTracks.SubtitleHeader"] = "字幕",
            ["MuxTracks.AddSubtitle"] = "字幕を追加",
            ["MuxTracks.Language"] = "言語",
            ["MuxTracks.Primary"] = "デフォルトトラック",
            ["MuxTracks.Empty"] = "トラックは追加されていません",
            ["MuxTracks.FileFilter"] = "字幕ファイル|*.ass;*.srt;*.ssa;*.vtt|すべてのファイル (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "字幕を追加",
            ["Hint.CannotDeleteSrcSubs"] = "ソース字幕の除外機能は現在実装予定がありません",
            ["Hint.FFmpegSubtitleDefault"] = "デフォルト字幕なしで複数多重化する場合、ffmpegは最初をデフォルトに",
            ["MuxTracks.CannotMuxSubtitle"] = "この字幕は mux できません。",
            ["MuxTracks.MissingFfmpeg"] = "字幕を mux するには ffmpeg が必要です。",
            ["MuxTracks.MissingFfprobe"] = "字幕トラックを確認するには ffprobe が必要です。",
            ["MuxTracks.DuplicateSourcePaths"] = "選択されたソース一覧に重複したパスがあります。",
            ["MuxTracks.NoDefault.SourceLine"] = "ソース：{0}",
            ["MuxTracks.NoDefault.NoDefaultMarked"] = "デフォルトとしてマークされた字幕がありません。",
            ["MuxTracks.NoDefault.OrigDefTrackId"] = "元のソースのデフォルト字幕トラック ID：{0}",
            ["MuxTracks.NoDefault.FirstSubTrackId"] = "最初のソース字幕トラック ID：{0}",
            ["MuxTracks.NoDefault.NoSourceSubs"] = "ソース字幕トラックが見つかりませんでした。",
            ["MuxTracks.NoDefault.ClearAndContinue"] = "デフォルトのマークを解除して続行しますか？",
            ["MuxTracks.NoDefault.ConsiderAdding"] = "続行前にデフォルト字幕を追加することを検討しますか？",
        }) Data["ja"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Источники очереди",
            ["MuxTracks.SubtitleHeader"] = "Субтитров",
            ["MuxTracks.AddSubtitle"] = "Добавить субтитры",
            ["MuxTracks.Language"] = "Язык",
            ["MuxTracks.Primary"] = "Трек по умолчанию",
            ["MuxTracks.Empty"] = "Дорожки не добавлены",
            ["MuxTracks.FileFilter"] = "Файлы субтитров|*.ass;*.srt;*.ssa;*.vtt|Все файлы (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Добавить субтитры",
            ["Hint.CannotDeleteSrcSubs"] = "Функция исключения исходных субтитров пока не планируется",
            ["Hint.FFmpegSubtitleDefault"] = "Без субтитров по умолчанию и при мультиплексировании нескольких, ffmpeg пометит первый как умолчанию",
            ["MuxTracks.CannotMuxSubtitle"] = "Невозможно mux-ить этот субтитр.",
            ["MuxTracks.MissingFfmpeg"] = "Для mux субтитров требуется ffmpeg.",
            ["MuxTracks.MissingFfprobe"] = "Для проверки дорожек субтитров требуется ffprobe.",
            ["MuxTracks.DuplicateSourcePaths"] = "В выбранном списке источников есть дублирующиеся пути.",
            ["MuxTracks.NoDefault.SourceLine"] = "Источник: {0}",
            ["MuxTracks.NoDefault.NoDefaultMarked"] = "Ни один субтитр не помечен как основной.",
            ["MuxTracks.NoDefault.OrigDefTrackId"] = "ID дорожки субтитров по умолчанию исходного источника: {0}",
            ["MuxTracks.NoDefault.FirstSubTrackId"] = "ID первой дорожки субтитров источника: {0}",
            ["MuxTracks.NoDefault.NoSourceSubs"] = "Дорожки субтитров источника не найдены.",
            ["MuxTracks.NoDefault.ClearAndContinue"] = "Снять отметки по умолчанию и продолжить?",
            ["MuxTracks.NoDefault.ConsiderAdding"] = "Рассмотреть добавление субтитров по умолчанию перед продолжением?",
        }) Data["ru"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Quellen der Warteschlange",
            ["MuxTracks.SubtitleHeader"] = "Untertitelspur",
            ["MuxTracks.AddSubtitle"] = "Untertitel hinzufügen",
            ["MuxTracks.Language"] = "Sprache",
            ["MuxTracks.Primary"] = "Standardspur",
            ["MuxTracks.Empty"] = "Keine Spuren hinzugefügt",
            ["MuxTracks.FileFilter"] = "Untertiteldateien|*.ass;*.srt;*.ssa;*.vtt|Alle Dateien (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Untertitel hinzufügen",
            ["Hint.CannotDeleteSrcSubs"] = "Eine Funktion zum Ausschließen von Quelluntertiteln ist derzeit nicht geplant",
            ["Hint.FFmpegSubtitleDefault"] = "Kein Standard-Untertitel und mehrere gemuxt, ffmpeg markiert den ersten als Standard",
            ["MuxTracks.CannotMuxSubtitle"] = "Dieser Untertitel kann nicht gemuxt werden.",
            ["MuxTracks.MissingFfmpeg"] = "ffmpeg wird benötigt, um Untertitel zu muxen.",
            ["MuxTracks.MissingFfprobe"] = "ffprobe wird benötigt, um Untertitelspuren zu prüfen.",
            ["MuxTracks.DuplicateSourcePaths"] = "Die ausgewählte Quellenliste enthält doppelte Pfade.",
            ["MuxTracks.NoDefault.SourceLine"] = "Quelle: {0}",
            ["MuxTracks.NoDefault.NoDefaultMarked"] = "Kein Untertitel als Standard markiert.",
            ["MuxTracks.NoDefault.OrigDefTrackId"] = "Standard-Untertitelspur-ID der Originalquelle: {0}",
            ["MuxTracks.NoDefault.FirstSubTrackId"] = "Erste Quell-Untertitelspur-ID: {0}",
            ["MuxTracks.NoDefault.NoSourceSubs"] = "Keine Quell-Untertitelspuren gefunden.",
            ["MuxTracks.NoDefault.ClearAndContinue"] = "Standardmarkierungen entfernen und fortfahren?",
            ["MuxTracks.NoDefault.ConsiderAdding"] = "Vor dem Fortfahren einen Standard-Untertitel hinzufügen?",
        }) Data["de"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "대기열 소스",
            ["MuxTracks.SubtitleHeader"] = "자막 트랙",
            ["MuxTracks.AddSubtitle"] = "자막 추가",
            ["MuxTracks.Language"] = "언어",
            ["MuxTracks.Primary"] = "기본 트랙",
            ["MuxTracks.Empty"] = "추가된 트랙이 없습니다",
            ["MuxTracks.FileFilter"] = "자막 파일|*.ass;*.srt;*.ssa;*.vtt|모든 파일 (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "자막 추가",
            ["Hint.CannotDeleteSrcSubs"] = "소스 자막 제외 기능은 현재 구현 계획이 없습니다",
            ["Hint.FFmpegSubtitleDefault"] = "기본 자막 없이 여러 자막을 멀티플렉싱 시, ffmpeg는 첫 번째를 기본으로 지정",
            ["MuxTracks.CannotMuxSubtitle"] = "이 자막은 mux 할 수 없습니다.",
            ["MuxTracks.MissingFfmpeg"] = "자막을 mux 하려면 ffmpeg가 필요합니다.",
            ["MuxTracks.MissingFfprobe"] = "자막 트랙을 확인하려면 ffprobe가 필요합니다.",
            ["MuxTracks.DuplicateSourcePaths"] = "선택한 소스 목록에 중복된 경로가 있습니다.",
            ["MuxTracks.NoDefault.SourceLine"] = "소스: {0}",
            ["MuxTracks.NoDefault.NoDefaultMarked"] = "기본으로 표시된 자막이 없습니다.",
            ["MuxTracks.NoDefault.OrigDefTrackId"] = "원본 소스 기본 자막 트랙 ID: {0}",
            ["MuxTracks.NoDefault.FirstSubTrackId"] = "첫 번째 소스 자막 트랙 ID: {0}",
            ["MuxTracks.NoDefault.NoSourceSubs"] = "소스 자막 트랙을 찾을 수 없습니다.",
            ["MuxTracks.NoDefault.ClearAndContinue"] = "기본 표시를 지우고 계속하시겠습니까?",
            ["MuxTracks.NoDefault.ConsiderAdding"] = "계속하기 전에 기본 자막을 추가하는 것을 고려하시겠습니까?",
        }) Data["ko"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Fontes da fila",
            ["MuxTracks.SubtitleHeader"] = "Faixa de legenda",
            ["MuxTracks.AddSubtitle"] = "Adicionar legenda",
            ["MuxTracks.Language"] = "Idioma",
            ["MuxTracks.Primary"] = "Faixa padrão",
            ["MuxTracks.Empty"] = "Nenhuma faixa adicionada",
            ["MuxTracks.FileFilter"] = "Arquivos de legenda|*.ass;*.srt;*.ssa;*.vtt|Todos os arquivos (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Adicionar legendas",
            ["Hint.CannotDeleteSrcSubs"] = "O recurso de exclusão de legendas de origem não está planejado no momento",
            ["Hint.FFmpegSubtitleDefault"] = "Sem legenda padrão e multiplexando várias, ffmpeg marca a primeira como padrão",
            ["MuxTracks.CannotMuxSubtitle"] = "Não é possível muxar esta legenda.",
            ["MuxTracks.MissingFfmpeg"] = "ffmpeg é necessário para muxar legendas.",
            ["MuxTracks.MissingFfprobe"] = "ffprobe é necessário para inspecionar as faixas de legenda.",
            ["MuxTracks.DuplicateSourcePaths"] = "A lista de origens selecionada contém caminhos duplicados.",
            ["MuxTracks.NoDefault.SourceLine"] = "Origem: {0}",
            ["MuxTracks.NoDefault.NoDefaultMarked"] = "Nenhuma legenda está marcada como padrão.",
            ["MuxTracks.NoDefault.OrigDefTrackId"] = "ID da faixa de legenda padrão da fonte original: {0}",
            ["MuxTracks.NoDefault.FirstSubTrackId"] = "ID da primeira faixa de legenda da fonte: {0}",
            ["MuxTracks.NoDefault.NoSourceSubs"] = "Nenhuma faixa de legenda da fonte encontrada.",
            ["MuxTracks.NoDefault.ClearAndContinue"] = "Limpar marcações padrão e continuar?",
            ["MuxTracks.NoDefault.ConsiderAdding"] = "Considerar adicionar uma legenda padrão antes de continuar?",
        }) Data["pt-br"][pair.Key] = pair.Value;
    }

    public static MuxLangProvider Current => new(UILangProvider.Current.LanguageCode);
}
