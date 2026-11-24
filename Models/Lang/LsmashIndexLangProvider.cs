namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Localized strings for the L-SMASH index prefetch progress window.
/// </summary>
public class LsmashIndexLangProvider(string languageCode) : LangProviderBase(languageCode, Data)
{
    // Window titles stay in English for support and web-search consistency.
    public const string WindowTitle = "L-SMASH Indexing";

    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["LsmashIndex.CountFormat"] = "Indexed {0}/{1}",
            ["LsmashIndex.Preparing"] = "Preparing index…",
            ["LsmashIndex.CurrentFile"] = "Indexing {0}",
            ["LsmashIndex.Done"] = "Indexing complete",
        },
        ["zh-cn"] = new()
        {
            ["LsmashIndex.CountFormat"] = "已完成 {0}/{1}",
            ["LsmashIndex.Preparing"] = "正在准备索引…",
            ["LsmashIndex.CurrentFile"] = "正在建立索引：{0}",
            ["LsmashIndex.Done"] = "索引建立完成",
        },
        ["zh-tw"] = new()
        {
            ["LsmashIndex.CountFormat"] = "已完成 {0}/{1}",
            ["LsmashIndex.Preparing"] = "正在準備索引…",
            ["LsmashIndex.CurrentFile"] = "正在建立索引：{0}",
            ["LsmashIndex.Done"] = "索引建立完成",
        },
        ["fr"] = new()
        {
            ["LsmashIndex.CountFormat"] = "Indexés {0}/{1}",
            ["LsmashIndex.Preparing"] = "Préparation de l'index…",
            ["LsmashIndex.CurrentFile"] = "Indexation de {0}",
            ["LsmashIndex.Done"] = "Indexation terminée",
        },
        ["es"] = new()
        {
            ["LsmashIndex.CountFormat"] = "Indexados {0}/{1}",
            ["LsmashIndex.Preparing"] = "Preparando índice…",
            ["LsmashIndex.CurrentFile"] = "Indexando {0}",
            ["LsmashIndex.Done"] = "Indexación completada",
        },
        ["ja"] = new()
        {
            ["LsmashIndex.CountFormat"] = "インデックス済み {0}/{1}",
            ["LsmashIndex.Preparing"] = "インデックスを準備中…",
            ["LsmashIndex.CurrentFile"] = "インデックス作成中: {0}",
            ["LsmashIndex.Done"] = "インデックス作成完了",
        },
        ["ru"] = new()
        {
            ["LsmashIndex.CountFormat"] = "Проиндексировано {0}/{1}",
            ["LsmashIndex.Preparing"] = "Подготовка индекса…",
            ["LsmashIndex.CurrentFile"] = "Индексация {0}",
            ["LsmashIndex.Done"] = "Индексация завершена",
        },
        ["de"] = new()
        {
            ["LsmashIndex.CountFormat"] = "Indiziert {0}/{1}",
            ["LsmashIndex.Preparing"] = "Index wird vorbereitet…",
            ["LsmashIndex.CurrentFile"] = "Indiziere {0}",
            ["LsmashIndex.Done"] = "Indizierung abgeschlossen",
        },
        ["ko"] = new()
        {
            ["LsmashIndex.CountFormat"] = "인덱싱 완료 {0}/{1}",
            ["LsmashIndex.Preparing"] = "인덱스 준비 중…",
            ["LsmashIndex.CurrentFile"] = "{0} 인덱싱 중",
            ["LsmashIndex.Done"] = "인덱싱 완료",
        },
        ["pt-br"] = new()
        {
            ["LsmashIndex.CountFormat"] = "Indexados {0}/{1}",
            ["LsmashIndex.Preparing"] = "Preparando índice…",
            ["LsmashIndex.CurrentFile"] = "Indexando {0}",
            ["LsmashIndex.Done"] = "Indexação concluída",
        },
    };

    public static LsmashIndexLangProvider Current => new(UILangProvider.Current.LanguageCode);
}
