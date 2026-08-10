namespace OneColumnEncoder.Models.Lang;

public class ConfirmDialogLangProvider : LangProviderBase
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
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
            ["ConfirmDialog.CopyText"] = "Копировать сообщение",
            ["ConfirmDialog.WarningPrefix"] = "Предупреждение: ",
            ["ConfirmDialog.ErrorPrefix"] = "Ошибка: ",
            ["ConfirmDialog.InfoPrefix"] = "Инфо: ",
            ["ConfirmDialog.SuccessPrefix"] = "Успех: ",
            ["ConfirmDialog.CopyHint"] = "Щёлкните текст правой кнопкой, чтобы скопировать",
            ["ConfirmDialog.DebugPrefix"] = "Отладка: ",
        };
        Data["de"] = new(Data["en"])
        {
            ["ConfirmDialog.CopyText"] = "Nachricht kopieren",
            ["ConfirmDialog.CopyHint"] = "Rechtsklick auf Text zum Kopieren",
            ["ConfirmDialog.WarningPrefix"] = "Warnung: ",
            ["ConfirmDialog.ErrorPrefix"] = "Fehler: ",
            ["ConfirmDialog.DebugPrefix"] = "Debug: ",
            ["ConfirmDialog.InfoPrefix"] = "Info: ",
            ["ConfirmDialog.SuccessPrefix"] = "Erfolg: ",
        };
        Data["ko"] = new(Data["en"])
        {
            ["ConfirmDialog.CopyText"] = "메시지 복사",
            ["ConfirmDialog.CopyHint"] = "텍스트를 마우스 오른쪽 버튼으로 클릭하여 복사",
            ["ConfirmDialog.WarningPrefix"] = "경고: ",
            ["ConfirmDialog.ErrorPrefix"] = "오류: ",
            ["ConfirmDialog.DebugPrefix"] = "디버그: ",
            ["ConfirmDialog.InfoPrefix"] = "정보: ",
            ["ConfirmDialog.SuccessPrefix"] = "성공: ",
        };
    }

    public static ConfirmDialogLangProvider Current => new(UILangProvider.Current.LanguageCode);

    public ConfirmDialogLangProvider(string languageCode) : base(languageCode, Data)
    {
    }
}
