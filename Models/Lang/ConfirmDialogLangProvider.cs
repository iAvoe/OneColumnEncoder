namespace OneColumnEncoder.Models.Lang;

public class ConfirmDialogLangProvider
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["ConfirmDialog.Cancel"] = "Cancel",
            ["ConfirmDialog.Confirm"] = "Confirm",
            ["ConfirmDialog.CopyText"] = "Copy Message",
            ["ConfirmDialog.CopyHint"] = "Right-click on text to copy",
            ["ConfirmDialog.WarningPrefix"] = "Warning: ",
            ["ConfirmDialog.ErrorPrefix"] = "Error: ",
            ["ConfirmDialog.DebugPrefix"] = "Debug: ",
            ["ConfirmDialog.InfoPrefix"] = "Info: ",
            ["ConfirmDialog.SuccessPrefix"] = "Success: ",
        },
        ["zh-cn"] = new()
        {
            ["ConfirmDialog.Cancel"] = "取消",
            ["ConfirmDialog.Confirm"] = "确认",
            ["ConfirmDialog.CopyText"] = "复制文本",
            ["ConfirmDialog.CopyHint"] = "右键单击文本以复制",
            ["ConfirmDialog.WarningPrefix"] = "警告：",
            ["ConfirmDialog.ErrorPrefix"] = "错误：",
            ["ConfirmDialog.DebugPrefix"] = "调试：",
            ["ConfirmDialog.InfoPrefix"] = "信息：",
            ["ConfirmDialog.SuccessPrefix"] = "成功：",
        },
        ["zh-tw"] = new()
        {
            ["ConfirmDialog.Cancel"] = "取消",
            ["ConfirmDialog.Confirm"] = "確認",
            ["ConfirmDialog.CopyText"] = "複製文字",
            ["ConfirmDialog.CopyHint"] = "右鍵點擊文字以複製",
            ["ConfirmDialog.WarningPrefix"] = "警告：",
            ["ConfirmDialog.ErrorPrefix"] = "錯誤：",
            ["ConfirmDialog.DebugPrefix"] = "除錯：",
            ["ConfirmDialog.InfoPrefix"] = "資訊：",
            ["ConfirmDialog.SuccessPrefix"] = "成功：",
        },
    };

    static ConfirmDialogLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["ConfirmDialog.Cancel"] = "Annuler",
            ["ConfirmDialog.Confirm"] = "Confirmer",
            ["ConfirmDialog.CopyText"] = "Copier message",
            ["ConfirmDialog.CopyHint"] = "Clic droit sur le texte pour copier",
            ["ConfirmDialog.WarningPrefix"] = "Avertissement : ",
            ["ConfirmDialog.ErrorPrefix"] = "Erreur : ",
            ["ConfirmDialog.DebugPrefix"] = "Debug : ",
            ["ConfirmDialog.InfoPrefix"] = "Info : ",
            ["ConfirmDialog.SuccessPrefix"] = "Succès : ",
        };
        Data["es"] = new(Data["en"])
        {
            ["ConfirmDialog.Cancel"] = "Cancelar",
            ["ConfirmDialog.Confirm"] = "Confirmar",
            ["ConfirmDialog.CopyText"] = "Copiar mensaje",
            ["ConfirmDialog.CopyHint"] = "Clic derecho sobre texto para copiar",
            ["ConfirmDialog.WarningPrefix"] = "Aviso: ",
            ["ConfirmDialog.ErrorPrefix"] = "Error: ",
            ["ConfirmDialog.InfoPrefix"] = "Info: ",
            ["ConfirmDialog.SuccessPrefix"] = "Correcto: ",
            ["ConfirmDialog.DebugPrefix"] = "Depuración: ",
        };
        Data["ja"] = new(Data["en"])
        {
            ["ConfirmDialog.Cancel"] = "キャンセル",
            ["ConfirmDialog.Confirm"] = "確認",
            ["ConfirmDialog.CopyText"] = "メッセージコピー",
            ["ConfirmDialog.WarningPrefix"] = "警告: ",
            ["ConfirmDialog.ErrorPrefix"] = "エラー: ",
            ["ConfirmDialog.InfoPrefix"] = "情報: ",
            ["ConfirmDialog.SuccessPrefix"] = "成功: ",
            ["ConfirmDialog.CopyHint"] = "テキストを右クリックしてコピー",
            ["ConfirmDialog.DebugPrefix"] = "デバッグ: ",
        };
        Data["ru"] = new(Data["en"])
        {
            ["ConfirmDialog.Cancel"] = "Отмена",
            ["ConfirmDialog.Confirm"] = "Подтвердить",
            ["ConfirmDialog.CopyText"] = "Копировать сообщение",
            ["ConfirmDialog.WarningPrefix"] = "Предупреждение: ",
            ["ConfirmDialog.ErrorPrefix"] = "Ошибка: ",
            ["ConfirmDialog.InfoPrefix"] = "Инфо: ",
            ["ConfirmDialog.SuccessPrefix"] = "Успех: ",
            ["ConfirmDialog.CopyHint"] = "Щёлкните текст правой кнопкой, чтобы скопировать",
            ["ConfirmDialog.DebugPrefix"] = "Отладка: ",
        };
    }

    private readonly Dictionary<string, string> _d;

    public static ConfirmDialogLangProvider Current => new(UILangProvider.Current.LanguageCode);
    public string LanguageCode { get; }
    public string this[string key] => _d.TryGetValue(key, out var value) ? value : key;

    public ConfirmDialogLangProvider(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];
    }
}
