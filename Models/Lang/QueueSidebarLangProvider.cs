namespace OneColumnEncoder.Models.Lang;

public sealed class QueueSidebarLangProvider : LangProviderBase
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["QueueSidebarCancelAllText"] = "Cancel all",
            ["QueueSidebarRunningHeaderText"] = "Running",
            ["QueueSidebarWaitingHeaderText"] = "Pending",
            ["QueueSidebarUnfinishedHeaderText"] = "Unfinished",
            ["QueueSidebarCompletedHeaderText"] = "Completed",
        },
        ["zh-cn"] = new()
        {
            ["QueueSidebarCancelAllText"] = "取消全部",
            ["QueueSidebarRunningHeaderText"] = "当前运行",
            ["QueueSidebarWaitingHeaderText"] = "待运行",
            ["QueueSidebarUnfinishedHeaderText"] = "未完成",
            ["QueueSidebarCompletedHeaderText"] = "已完成",
        },
        ["zh-tw"] = new()
        {
            ["QueueSidebarCancelAllText"] = "取消全部",
            ["QueueSidebarRunningHeaderText"] = "目前執行",
            ["QueueSidebarWaitingHeaderText"] = "待執行",
            ["QueueSidebarUnfinishedHeaderText"] = "未完成",
            ["QueueSidebarCompletedHeaderText"] = "已完成",
        }
    };

    static QueueSidebarLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["QueueSidebarCancelAllText"] = "Tout annuler",
            ["QueueSidebarRunningHeaderText"] = "En cours",
            ["QueueSidebarWaitingHeaderText"] = "En attente",
            ["QueueSidebarUnfinishedHeaderText"] = "Inachevé",
            ["QueueSidebarCompletedHeaderText"] = "Terminé",
        };
        Data["es"] = new(Data["en"])
        {
            ["QueueSidebarCancelAllText"] = "Cancelar todo",
            ["QueueSidebarRunningHeaderText"] = "En ejecución",
            ["QueueSidebarWaitingHeaderText"] = "Pendiente",
            ["QueueSidebarUnfinishedHeaderText"] = "Sin finalizar",
            ["QueueSidebarCompletedHeaderText"] = "Completado",
        };
        Data["ja"] = new(Data["en"])
        {
            ["QueueSidebarCancelAllText"] = "すべてキャンセル",
            ["QueueSidebarRunningHeaderText"] = "実行中",
            ["QueueSidebarWaitingHeaderText"] = "待機中",
            ["QueueSidebarUnfinishedHeaderText"] = "未完了",
            ["QueueSidebarCompletedHeaderText"] = "完了",
        };
        Data["ru"] = new(Data["en"])
        {
            ["QueueSidebarCancelAllText"] = "Отменить всё",
            ["QueueSidebarRunningHeaderText"] = "Выполняется",
            ["QueueSidebarWaitingHeaderText"] = "В ожидании",
            ["QueueSidebarUnfinishedHeaderText"] = "Незавершённые",
["QueueSidebarCompletedHeaderText"] = "Завершённые",
        };
        Data["de"] = new(Data["en"])
        {
            ["QueueSidebarCancelAllText"] = "Alle abbrechen",
            ["QueueSidebarRunningHeaderText"] = "Läuft",
            ["QueueSidebarWaitingHeaderText"] = "Ausstehend",
            ["QueueSidebarUnfinishedHeaderText"] = "Unvollständig",
            ["QueueSidebarCompletedHeaderText"] = "Abgeschlossen",
        };
        Data["ko"] = new(Data["en"])
        {
            ["QueueSidebarCancelAllText"] = "모두 취소",
            ["QueueSidebarRunningHeaderText"] = "실행 중",
            ["QueueSidebarWaitingHeaderText"] = "대기 중",
            ["QueueSidebarUnfinishedHeaderText"] = "미완료",
            ["QueueSidebarCompletedHeaderText"] = "완료",
        };
        Data["pt-br"] = new(Data["en"])
        {
            ["QueueSidebarCancelAllText"] = "Cancelar tudo",
            ["QueueSidebarRunningHeaderText"] = "Em execução",
            ["QueueSidebarWaitingHeaderText"] = "Pendente",
            ["QueueSidebarUnfinishedHeaderText"] = "Inacabado",
            ["QueueSidebarCompletedHeaderText"] = "Concluído",
        };
    }

    public static QueueSidebarLangProvider Current => new(UILangProvider.Current.LanguageCode);

    public string QueueSidebarCancelAllText { get; }
    public string QueueSidebarRunningHeaderText { get; }
    public string QueueSidebarWaitingHeaderText { get; }
    public string QueueSidebarUnfinishedHeaderText { get; }
    public string QueueSidebarCompletedHeaderText { get; }
    public static string QueueItemRemoveText => "🗙";
    public static string QueueItemMoveUpText => "↑↑";
    public static string QueueItemMoveDownText => "↓↓";

    public QueueSidebarLangProvider(string languageCode) : base(languageCode, Data)
    {
        QueueSidebarCancelAllText = this["QueueSidebarCancelAllText"];
        QueueSidebarRunningHeaderText = this["QueueSidebarRunningHeaderText"];
        QueueSidebarWaitingHeaderText = this["QueueSidebarWaitingHeaderText"];
        QueueSidebarUnfinishedHeaderText = this["QueueSidebarUnfinishedHeaderText"];
        QueueSidebarCompletedHeaderText = this["QueueSidebarCompletedHeaderText"];
    }
}
