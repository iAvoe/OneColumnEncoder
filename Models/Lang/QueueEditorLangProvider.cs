namespace OneColumnEncoder.Models.Lang;

public sealed class QueueEditorLangProvider : LangProviderBase
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["QueueEditor.Title"] = "Edit Queue",
        },
        ["zh-cn"] = new()
        {
            ["QueueEditor.Title"] = "调整队列",
        },
        ["zh-tw"] = new()
        {
            ["QueueEditor.Title"] = "調整隊列",
        },
        ["fr"] = new()
        {
            ["QueueEditor.Title"] = "Modifier la file",
        },
        ["es"] = new()
        {
            ["QueueEditor.Title"] = "Editar cola",
        },
        ["ja"] = new()
        {
            ["QueueEditor.Title"] = "キューを編集",
        },
        ["ru"] = new()
        {
            ["QueueEditor.Title"] = "Редактировать очередь",
        },
        ["de"] = new()
        {
            ["QueueEditor.Title"] = "Warteschlange bearbeiten",
        },
        ["ko"] = new()
        {
            ["QueueEditor.Title"] = "큐 편집",
        },
    };

    public static QueueEditorLangProvider Current => new(UILangProvider.Current.LanguageCode);

    public QueueEditorLangProvider(string languageCode) : base(languageCode, Data)
    {
    }
}
