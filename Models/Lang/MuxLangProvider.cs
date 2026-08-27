namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Localized strings for the external audio/subtitle track editor.
/// </summary>
public sealed class MuxLangProvider(string languageCode) : LangProviderBase(languageCode, Data)
{
    public const string WindowTitle = "Add Audio/Subtitles";

    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["MuxTracks.QueueSources"] = "Queue Sources",
            ["MuxTracks.AudioHeader"] = "Audio Track Adding",
            ["MuxTracks.SubtitleHeader"] = "Subtitle Track Adding",
            ["MuxTracks.AddAudio"] = "Add Audio",
            ["MuxTracks.AddSubtitle"] = "Add Subtitle",
            ["MuxTracks.Sync"] = "Sync (\u00B1ms)",
            ["MuxTracks.Primary"] = "Primary track",
            ["MuxTracks.Empty"] = "No tracks added",
            ["MuxTracks.DurationUnknown"] = "N/A",
            ["MuxTracks.Moved"] = "moved",
            ["MuxTracks.InvalidSync"] = "Sync must be a whole number of milliseconds.",
            ["MuxTracks.FileFilter"] = "Media files|*.aac;*.ac3;*.eac3;*.flac;*.m4a;*.mka;*.mp3;*.ogg;*.opus;*.wav;*.ass;*.srt;*.ssa;*.sub;*.sup|All files (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Add Audio/Subtitles",
        },
    };

    static MuxLangProvider()
    {
        foreach (string code in new[] { "zh-cn", "zh-tw", "fr", "es", "ja", "ru", "de", "ko", "pt-br" })
            Data[code] = new(Data["en"]);

        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "队列源",
            ["MuxTracks.AudioHeader"] = "添加音轨",
            ["MuxTracks.SubtitleHeader"] = "添加字幕轨",
            ["MuxTracks.AddAudio"] = "添加音频",
            ["MuxTracks.AddSubtitle"] = "添加字幕",
            ["MuxTracks.Sync"] = "同步（+-毫秒）",
            ["MuxTracks.Primary"] = "主轨",
            ["MuxTracks.InvalidSync"] = "同步偏移值必须是整数毫秒。",
            ["MuxTracks.FileFilter"] = "媒体文件|*.aac;*.ac3;*.eac3;*.flac;*.m4a;*.mka;*.mp3;*.ogg;*.opus;*.wav;*.ass;*.srt;*.ssa;*.sub;*.sup|All files (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "添加音频/字幕轨道",
        }) Data["zh-cn"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "佇列來源",
            ["MuxTracks.AudioHeader"] = "新增音軌",
            ["MuxTracks.SubtitleHeader"] = "新增字幕軌",
            ["MuxTracks.AddAudio"] = "新增音訊",
            ["MuxTracks.AddSubtitle"] = "新增字幕",
            ["MuxTracks.Sync"] = "同步（+-毫秒）",
            ["MuxTracks.Primary"] = "主要軌",
            ["MuxTracks.InvalidSync"] = "同步偏移值必須是整數毫秒。",
            ["MuxTracks.FileFilter"] = "媒體檔案|*.aac;*.ac3;*.eac3;*.flac;*.m4a;*.mka;*.mp3;*.ogg;*.opus;*.wav;*.ass;*.srt;*.ssa;*.sub;*.sup|All files (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "添加音訊/字幕軌道",
        }) Data["zh-tw"][pair.Key] = pair.Value;

        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Sources de file",
            ["MuxTracks.AudioHeader"] = "Ajout de piste audio",
            ["MuxTracks.SubtitleHeader"] = "Ajout de piste de sous-titres",
            ["MuxTracks.AddAudio"] = "Ajouter audio",
            ["MuxTracks.AddSubtitle"] = "Ajouter sous-titre",
            ["MuxTracks.Sync"] = "Sync (±ms)",
            ["MuxTracks.Primary"] = "Piste principale",
            ["MuxTracks.Empty"] = "Aucune piste ajoutée",
            ["MuxTracks.Moved"] = "déplacé",
            ["MuxTracks.InvalidSync"] = "Le décalage de synchro doit être un entier en millisecondes.",
            ["MuxTracks.FileFilter"] = "Fichiers multimédias|*.aac;*.ac3;*.eac3;*.flac;*.m4a;*.mka;*.mp3;*.ogg;*.opus;*.wav;*.ass;*.srt;*.ssa;*.sub;*.sup|Tous les fichiers (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Ajouter audio/sous-titres",
        }) Data["fr"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Fuentes de cola",
            ["MuxTracks.AudioHeader"] = "Añadir pista de audio",
            ["MuxTracks.SubtitleHeader"] = "Añadir pista de subtítulos",
            ["MuxTracks.AddAudio"] = "Añadir audio",
            ["MuxTracks.AddSubtitle"] = "Añadir subtítulo",
            ["MuxTracks.Sync"] = "Sincronización (±ms)",
            ["MuxTracks.Primary"] = "Pista principal",
            ["MuxTracks.Empty"] = "No hay pistas añadidas",
            ["MuxTracks.Moved"] = "movido",
            ["MuxTracks.InvalidSync"] = "El desplazamiento de sincronización debe ser un número entero de milisegundos.",
            ["MuxTracks.FileFilter"] = "Archivos multimedia|*.aac;*.ac3;*.eac3;*.flac;*.m4a;*.mka;*.mp3;*.ogg;*.opus;*.wav;*.ass;*.srt;*.ssa;*.sub;*.sup|Todos los archivos (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Añadir audio/subtítulos",
        }) Data["es"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "キューのソース",
            ["MuxTracks.AudioHeader"] = "音声トラックの追加",
            ["MuxTracks.SubtitleHeader"] = "字幕トラックの追加",
            ["MuxTracks.AddAudio"] = "音声を追加",
            ["MuxTracks.AddSubtitle"] = "字幕を追加",
            ["MuxTracks.Sync"] = "同期 (±ms)",
            ["MuxTracks.Primary"] = "メイントラック",
            ["MuxTracks.Empty"] = "トラックは追加されていません",
            ["MuxTracks.Moved"] = "移動済み",
            ["MuxTracks.InvalidSync"] = "同期オフセットはミリ秒単位の整数で指定してください。",
            ["MuxTracks.FileFilter"] = "メディア ファイル|*.aac;*.ac3;*.eac3;*.flac;*.m4a;*.mka;*.mp3;*.ogg;*.opus;*.wav;*.ass;*.srt;*.ssa;*.sub;*.sup|すべてのファイル (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "音声/字幕を追加",
        }) Data["ja"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Источники очереди",
            ["MuxTracks.AudioHeader"] = "Добавление аудиодорожки",
            ["MuxTracks.SubtitleHeader"] = "Добавление дорожки субтитров",
            ["MuxTracks.AddAudio"] = "Добавить аудио",
            ["MuxTracks.AddSubtitle"] = "Добавить субтитры",
            ["MuxTracks.Sync"] = "Синхронизация (±мс)",
            ["MuxTracks.Primary"] = "Основная дорожка",
            ["MuxTracks.Empty"] = "Дорожки не добавлены",
            ["MuxTracks.Moved"] = "перемещено",
            ["MuxTracks.InvalidSync"] = "Смещение синхронизации должно быть целым числом миллисекунд.",
            ["MuxTracks.FileFilter"] = "Медиафайлы|*.aac;*.ac3;*.eac3;*.flac;*.m4a;*.mka;*.mp3;*.ogg;*.opus;*.wav;*.ass;*.srt;*.ssa;*.sub;*.sup|Все файлы (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Добавить аудио/субтитры",
        }) Data["ru"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Quellen der Warteschlange",
            ["MuxTracks.AudioHeader"] = "Audiospur hinzufügen",
            ["MuxTracks.SubtitleHeader"] = "Untertitelspur hinzufügen",
            ["MuxTracks.AddAudio"] = "Audio hinzufügen",
            ["MuxTracks.AddSubtitle"] = "Untertitel hinzufügen",
            ["MuxTracks.Sync"] = "Sync (±ms)",
            ["MuxTracks.Primary"] = "Hauptspur",
            ["MuxTracks.Empty"] = "Keine Spuren hinzugefügt",
            ["MuxTracks.Moved"] = "verschoben",
            ["MuxTracks.InvalidSync"] = "Die Sync-Verschiebung muss eine ganze Millisekundenzahl sein.",
            ["MuxTracks.FileFilter"] = "Mediendateien|*.aac;*.ac3;*.eac3;*.flac;*.m4a;*.mka;*.mp3;*.ogg;*.opus;*.wav;*.ass;*.srt;*.ssa;*.sub;*.sup|Alle Dateien (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Audio/Untertitel hinzufügen",
        }) Data["de"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "대기열 소스",
            ["MuxTracks.AudioHeader"] = "오디오 트랙 추가",
            ["MuxTracks.SubtitleHeader"] = "자막 트랙 추가",
            ["MuxTracks.AddAudio"] = "오디오 추가",
            ["MuxTracks.AddSubtitle"] = "자막 추가",
            ["MuxTracks.Sync"] = "동기화 (±ms)",
            ["MuxTracks.Primary"] = "기본 트랙",
            ["MuxTracks.Empty"] = "추가된 트랙이 없습니다",
            ["MuxTracks.Moved"] = "이동됨",
            ["MuxTracks.InvalidSync"] = "동기화 오프셋은 정수 밀리초여야 합니다.",
            ["MuxTracks.FileFilter"] = "미디어 파일|*.aac;*.ac3;*.eac3;*.flac;*.m4a;*.mka;*.mp3;*.ogg;*.opus;*.wav;*.ass;*.srt;*.ssa;*.sub;*.sup|모든 파일 (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "오디오/자막 추가",
        }) Data["ko"][pair.Key] = pair.Value;
        foreach (var pair in new Dictionary<string, string>
        {
            ["MuxTracks.QueueSources"] = "Fontes da fila",
            ["MuxTracks.AudioHeader"] = "Adição de faixa de áudio",
            ["MuxTracks.SubtitleHeader"] = "Adição de faixa de legenda",
            ["MuxTracks.AddAudio"] = "Adicionar áudio",
            ["MuxTracks.AddSubtitle"] = "Adicionar legenda",
            ["MuxTracks.Sync"] = "Sincronia (±ms)",
            ["MuxTracks.Primary"] = "Faixa principal",
            ["MuxTracks.Empty"] = "Nenhuma faixa adicionada",
            ["MuxTracks.Moved"] = "movido",
            ["MuxTracks.InvalidSync"] = "O deslocamento de sincronização deve ser um número inteiro de milissegundos.",
            ["MuxTracks.FileFilter"] = "Arquivos de mídia|*.aac;*.ac3;*.eac3;*.flac;*.m4a;*.mka;*.mp3;*.ogg;*.opus;*.wav;*.ass;*.srt;*.ssa;*.sub;*.sup|Todos os arquivos (*.*)|*.*",
            ["MuxTracks.WindowButton"] = "Adicionar áudio/legendas",
        }) Data["pt-br"][pair.Key] = pair.Value;
    }

    public static MuxLangProvider Current => new(UILangProvider.Current.LanguageCode);
}
