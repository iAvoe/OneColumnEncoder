namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Localized strings for the queue editor.
/// </summary>
public sealed class QueueEditorLangProvider(string languageCode) : LangProviderBase(languageCode, Data)
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["QueueEditor.Title"] = "Edit Queue",
            ["QueueEditor.SortBySize"] = "Sort by size",
            ["QueueEditor.SortByFilename"] = "Sort by filename",
            ["Hint.DoubleClickSortReverse"] = "Double click sort button to reverse",
        },
        ["zh-cn"] = new()
        {
            ["QueueEditor.Title"] = "调整队列",
            ["QueueEditor.SortBySize"] = "按大小排序",
            ["QueueEditor.SortByFilename"] = "按文件名排序",
            ["Hint.DoubleClickSortReverse"] = "双击排序按钮以取反",
        },
        ["zh-tw"] = new()
        {
            ["QueueEditor.Title"] = "調整隊列",
            ["QueueEditor.SortBySize"] = "按大小排序",
            ["QueueEditor.SortByFilename"] = "按檔名排序",
            ["Hint.DoubleClickSortReverse"] = "雙擊排序按鈕以取反",
        },
        ["fr"] = new()
        {
            ["QueueEditor.Title"] = "Modifier la file",
            ["QueueEditor.SortBySize"] = "Trier par taille",
            ["QueueEditor.SortByFilename"] = "Trier par nom",
            ["Hint.DoubleClickSortReverse"] = "Double-cliquez sur tri pour inverser",
        },
        ["es"] = new()
        {
            ["QueueEditor.Title"] = "Editar cola",
            ["QueueEditor.SortBySize"] = "Ordenar por tamaño",
            ["QueueEditor.SortByFilename"] = "Ordenar por nombre",
            ["Hint.DoubleClickSortReverse"] = "Doble clic en ordenar para invertir",
        },
        ["ja"] = new()
        {
            ["QueueEditor.Title"] = "キューを編集",
            ["QueueEditor.SortBySize"] = "サイズで並べ替え",
            ["QueueEditor.SortByFilename"] = "ファイル名で並べ替え",
            ["Hint.DoubleClickSortReverse"] = "ソートボタンをダブルクリックして順序を反転",
        },
        ["ru"] = new()
        {
            ["QueueEditor.Title"] = "Редактировать очередь",
            ["QueueEditor.SortBySize"] = "Сорт. по размеру",
            ["QueueEditor.SortByFilename"] = "Сорт. по имени",
            ["Hint.DoubleClickSortReverse"] = "Дважды кликните сортировку для обратного порядка",
        },
        ["de"] = new()
        {
            ["QueueEditor.Title"] = "Warteschlange bearbeiten",
            ["QueueEditor.SortBySize"] = "Nach Größe sortieren",
            ["QueueEditor.SortByFilename"] = "Nach Dateiname sortieren",
            ["Hint.DoubleClickSortReverse"] = "Doppelklick Sortieren zum Umkehren",
        },
        ["ko"] = new()
        {
            ["QueueEditor.Title"] = "큐 편집",
            ["QueueEditor.SortBySize"] = "크기순 정렬",
            ["QueueEditor.SortByFilename"] = "파일명순 정렬",
            ["Hint.DoubleClickSortReverse"] = "정렬 버튼을 더블 클릭하여 순서를 반전하세요",
        },
        ["pt-br"] = new()
        {
            ["QueueEditor.Title"] = "Editar fila",
            ["QueueEditor.SortBySize"] = "Ordenar por tamanho",
            ["QueueEditor.SortByFilename"] = "Ordenar por nome",
            ["Hint.DoubleClickSortReverse"] = "Duplo clique em ordenar para inverter",
        },
    };

    public static QueueEditorLangProvider Current => new(UILangProvider.Current.LanguageCode);
}
