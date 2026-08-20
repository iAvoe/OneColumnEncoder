namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Localized strings for the encoding monitor.
/// </summary>
public class EncodingMonitorModalLangProvider : LangProviderBase
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["ProgressTitle"] = "Progress",
            ["ProgressReportTitle"] = "Progress stream",
            ["MemoryTitle"] = "RAM use",

            ["StderrTitle"] = "Process log",
            ["DragLogReportHint"] = "Drag window edge to resize the log area; drag the log divider to adjust width",
            ["CurrentSizeLabel"] = "Current size / GB",
            ["EstimatedSizeLabel"] = "Estimated total / GB",
            ["WrittenFramesLabel"] = "Frames written",
            ["SampleIntervalLabel"] = "Sampling interval",
            ["StartedAtLabel"] = "Started At",
            ["ElapsedLabel"] = "Elapsed",
            ["RemainingLabel"] = "Remaining",
            ["CompleteAtLabel"] = "ETA",
            ["ArgsLabel"] = "Other preset name",
            ["SmallNoteText"] = "This program does not support progress save-load; interrupting will discard task progress",
            ["EnableMuxText"] = "Multiplex after encoding (the 2nd part of commandline, ffmpeg required)",
            ["RichTextModeText"] = "Parse rich text",
            ["DistributionUpstreamLabel"] = "Upstream program",
            ["DistributionDownstreamLabel"] = "Downstream program",
            ["DistributionCacheLabel"] = "System cache",
            ["DistributionAvailableLabel"] = "Available Space",
            ["MemoryRangeLegendTitle"] = "Range legend",
            ["SampleIntervalTickLabels"] = "Stop|60S|120S|180S|240",
            ["SampleIntervalZeroText"] = "Stop",
            ["RotateLogFontSizeText"] = "Rotate log fontsize",
            ["CopyUpstreamLogText"] = "Copy upstream log",
            ["CopyDownstreamLogText"] = "Copy downstream log",
            ["SaveLogsText"] = "Save logs",
            ["OpenTxtText"] = "Open TXT",

            ["OpenOutputDirectoryText"] = "Open output folder",
            ["ViewEncodingCommandText"] = "Review params",
            ["InterruptUpstreamText"] = "Interrupt upstream",
            ["InterruptEncoderText"] = "Interrupt encoder",
            ["CloseAfterDoneText"] = "Close",
            ["EncodingCommandTitle"] = "Encoding Command",
            ["PhysicalMemoryTopText"] = "Physical memory",
            ["PhysicalMemoryBottomText"] = "Total XX GB",
            ["CommittedMemoryTopText"] = "Committed memory",
            ["CommittedMemoryBottomText"] = "Limit XX GB",
            ["WorkingSetPeakTopText"] = "Working set peak",
            ["WorkingSetPeakBottomText"] = "Current XX GB",
            ["PageFileTopText"] = "Page file",
            ["PageFileBottomText"] = "Total XX GB",
            ["PageFaultTopText"] = "Page faults",
            ["PageFaultBottomText"] = "Hard & soft",
            ["RAMStressTopText"] = "RAM stress",
            ["RAMStressMediumText"] = "Mid",
            ["RAMStressHighText"] = "High",
            ["BlockTooltipFormat"] = "Range block {0}",
            ["PipeErrorPrefix"] = "Pipe error: ",
            ["ReadyToStartText"] = "Ready to start",
            ["EncodingText"] = "Encoding",
            ["AudioEncodingText"] = "Encoding audio",
            ["MuxingText"] = "Muxing",
            ["InterruptedText"] = "Interrupted",
            ["FailedText"] = "Encoding failed",
            ["CompletedText"] = "Encoding completed",
            ["ResetUsageStatusText"] = "Usage values reset",
            ["InterruptingUpstreamText"] = "Interrupting upstream",
            ["InterruptingEncoderText"] = "Interrupting encoder",
            ["ModeText"] = "mode",

            ["StopQueueConfirmTitle"] = "Stop queue",
            ["StopQueueConfirmMessage"] = "The current job has been interrupted. Stop the entire queue?",
        },
        ["zh-cn"] = new()
        {
            ["ProgressTitle"] = "进度",
            ["ProgressReportTitle"] = "进度流",
            ["MemoryTitle"] = "内存占用",

            ["StderrTitle"] = "进程日志",
            ["DragLogReportHint"] = "拖拽窗口边缘以调整日志显示面积；拖拽日志分界线以调整宽度",
            ["CurrentSizeLabel"] = "当前大小/GB",
            ["EstimatedSizeLabel"] = "预计总大小/GB",
            ["WrittenFramesLabel"] = "已写入帧数",
            ["SampleIntervalLabel"] = "采样间隔",
            ["StartedAtLabel"] = "开始时间（24h）",
            ["ElapsedLabel"] = "已用时",
            ["RemainingLabel"] = "预计剩余",
            ["CompleteAtLabel"] = "预计完成（24h）",
            ["ArgsLabel"] = "其他参数预设名",
            ["SmallNoteText"] = "本程序不支持进度存取；中断将丢弃任务进度",
            ["EnableMuxText"] = "压制完成后封装视频流（先前命令行的第二部分，需导入 ffmpeg）",
            ["RichTextModeText"] = "富文本解析",
            ["DistributionUpstreamLabel"] = "上游程序",
            ["DistributionDownstreamLabel"] = "下游程序",
            ["DistributionCacheLabel"] = "系统缓存",
            ["DistributionAvailableLabel"] = "可用空间",
            ["MemoryRangeLegendTitle"] = "范围图例",
            ["SampleIntervalTickLabels"] = "停|60秒|120秒|180秒|240秒",
            ["SampleIntervalZeroText"] = "停",
            ["RotateLogFontSizeText"] = "轮换日志字号",
            ["CopyUpstreamLogText"] = "复制上游日志",
            ["CopyDownstreamLogText"] = "复制下游日志",
            ["SaveLogsText"] = "保存日志",
            ["OpenTxtText"] = "打开 TXT",

            ["OpenOutputDirectoryText"] = "打开输出目录",
            ["ViewEncodingCommandText"] = "复查参数",
            ["InterruptUpstreamText"] = "中断上游程序",
            ["InterruptEncoderText"] = "中断编码器",
            ["CloseAfterDoneText"] = "关闭",
            ["EncodingCommandTitle"] = "编码命令",
            ["PhysicalMemoryTopText"] = "物理内存",
            ["PhysicalMemoryBottomText"] = "共 XX GB",
            ["CommittedMemoryTopText"] = "已提交",
            ["CommittedMemoryBottomText"] = "限额 XX GB",
            ["WorkingSetPeakTopText"] = "工作集峰值",
            ["WorkingSetPeakBottomText"] = "当前 XX GB",
            ["PageFileTopText"] = "页文件",
            ["PageFileBottomText"] = "总计 XX GB",
            ["PageFaultTopText"] = "页错误",
            ["PageFaultBottomText"] = "硬+软",
            ["RAMStressTopText"] = "内存压力",
            ["RAMStressMediumText"] = "中",
            ["RAMStressHighText"] = "高",
            ["BlockTooltipFormat"] = "范围块 {0}",
            ["PipeErrorPrefix"] = "管道错误：",
            ["ReadyToStartText"] = "准备启动",
            ["EncodingText"] = "正在压制",
            ["AudioEncodingText"] = "正在压制音频",
            ["MuxingText"] = "正在封装",
            ["InterruptedText"] = "已中断",
            ["FailedText"] = "压制失败",
            ["CompletedText"] = "压制完成",
            ["ResetUsageStatusText"] = "已重置占用值",
            ["InterruptingUpstreamText"] = "正在中断上游程序",
            ["InterruptingEncoderText"] = "正在中断编码器",
            ["ModeText"] = "模式",

            ["StopQueueConfirmTitle"] = "停止队列",
            ["StopQueueConfirmMessage"] = "当前任务已中断。是否停止整个队列？",
        },
        ["zh-tw"] = new()
        {
            ["ProgressTitle"] = "進度",
            ["ProgressReportTitle"] = "進度流",
            ["MemoryTitle"] = "記憶體占用",

            ["StderrTitle"] = "進程日誌",
            ["DragLogReportHint"] = "拖曳視窗邊緣以調整日誌顯示面積；拖曳日誌分界線以調整寬度",
            ["CurrentSizeLabel"] = "目前大小/GB",
            ["EstimatedSizeLabel"] = "預計總大小/GB",
            ["WrittenFramesLabel"] = "已寫入幀數",
            ["SampleIntervalLabel"] = "取樣間隔",
            ["StartedAtLabel"] = "開始時間（24h）",
            ["ElapsedLabel"] = "已用時",
            ["RemainingLabel"] = "預計剩餘",
            ["CompleteAtLabel"] = "預計完成（24h）",
            ["ArgsLabel"] = "其他參數預設名",
            ["SmallNoteText"] = "本程式不支援進度存取；中斷將丟棄任務進度",
            ["EnableMuxText"] = "壓製完成後封裝影片串流（先前命令行的第二部分，需導入 ffmpeg）",
            ["RichTextModeText"] = "富文本解析",
            ["DistributionUpstreamLabel"] = "上游程式",
            ["DistributionDownstreamLabel"] = "下游程式",
            ["DistributionCacheLabel"] = "系統快取",
            ["DistributionAvailableLabel"] = "可用空間",
            ["MemoryRangeLegendTitle"] = "範圍圖例",
            ["SampleIntervalTickLabels"] = "停|60秒|120秒|180秒|240秒",
            ["SampleIntervalZeroText"] = "停",
            ["RotateLogFontSizeText"] = "輪換日誌字型大小",
            ["CopyUpstreamLogText"] = "複製上游日誌",
            ["CopyDownstreamLogText"] = "複製下游日誌",
            ["SaveLogsText"] = "保存日誌",
            ["OpenTxtText"] = "開啟 TXT",

            ["OpenOutputDirectoryText"] = "開啟輸出資料夾",
            ["ViewEncodingCommandText"] = "複查參數",
            ["InterruptUpstreamText"] = "中斷上游程式",
            ["InterruptEncoderText"] = "中斷編碼器",
            ["CloseAfterDoneText"] = "關閉",
            ["EncodingCommandTitle"] = "編碼命令",
            ["PhysicalMemoryTopText"] = "實體記憶體",
            ["PhysicalMemoryBottomText"] = "共 XX GB",
            ["CommittedMemoryTopText"] = "已提交",
            ["CommittedMemoryBottomText"] = "上限 XX GB",
            ["WorkingSetPeakTopText"] = "工作集峰值",
            ["WorkingSetPeakBottomText"] = "目前 XX GB",
            ["PageFileTopText"] = "分頁檔",
            ["PageFileBottomText"] = "總計 XX GB",
            ["PageFaultTopText"] = "分頁錯誤",
            ["PageFaultBottomText"] = "硬+軟",
            ["RAMStressTopText"] = "記憶體壓力",
            ["RAMStressMediumText"] = "中",
            ["RAMStressHighText"] = "高",
            ["BlockTooltipFormat"] = "範圍塊 {0}",
            ["PipeErrorPrefix"] = "管道錯誤：",
            ["ReadyToStartText"] = "準備啟動",
            ["EncodingText"] = "正在壓制",
            ["AudioEncodingText"] = "正在壓制音訊",
            ["MuxingText"] = "正在封裝",
            ["InterruptedText"] = "已中斷",
            ["FailedText"] = "壓制失敗",
            ["CompletedText"] = "壓制完成",
            ["ResetUsageStatusText"] = "已重置佔用值",
            ["InterruptingUpstreamText"] = "正在中斷上游程式",
            ["InterruptingEncoderText"] = "正在中斷編碼器",
            ["ModeText"] = "模式",

            ["StopQueueConfirmTitle"] = "停止隊列",
            ["StopQueueConfirmMessage"] = "當前任務已中斷。是否停止整個隊列？",
        }
    };

    static EncodingMonitorModalLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["ProgressTitle"] = "Progression",
            ["ProgressReportTitle"] = "Flux de progression",
            ["MemoryTitle"] = "RAM util.",
            ["StderrTitle"] = "Journal du processus",
            ["DragLogReportHint"] = "Glissez le bord de la fenêtre pour redimensionner le journal; glissez le séparateur pour la largeur",
            ["CurrentSizeLabel"] = "Taille actuelle / GB",
            ["EstimatedSizeLabel"] = "Total estimé / GB",
            ["WrittenFramesLabel"] = "Images écrites",
            ["SampleIntervalLabel"] = "Intervalle d'échant.",
            ["StartedAtLabel"] = "Démarré à",
            ["ElapsedLabel"] = "Écoulé",
            ["RemainingLabel"] = "Restant",
            ["CompleteAtLabel"] = "ETA",
            ["ArgsLabel"] = "Nom d'autre préréglage",
            ["SmallNoteText"] = "Ce programme ne prend pas en charge la sauvegarde de progression; interrompre supprimera l'avancement",
            ["EnableMuxText"] = "Multiplexer après encodage (2e partie de commande, ffmpeg requis)",
            ["RichTextModeText"] = "Analyse de texte enrichi",
            ["DistributionUpstreamLabel"] = "Programme amont",
            ["DistributionDownstreamLabel"] = "Programme aval",
            ["DistributionCacheLabel"] = "Cache système",
            ["DistributionAvailableLabel"] = "Espace libre",
            ["MemoryRangeLegendTitle"] = "Légende",
            ["SampleIntervalTickLabels"] = "Arrêt|60 s|120 s|180 s|240 s",
            ["SampleIntervalZeroText"] = "Arrêt",
            ["RotateLogFontSizeText"] = "Changer taille police log",
            ["CopyUpstreamLogText"] = "Copier log amont",
            ["CopyDownstreamLogText"] = "Copier log aval",
            ["SaveLogsText"] = "Sauver les logs",
            ["OpenTxtText"] = "Ouvrir TXT",
            ["OpenOutputDirectoryText"] = "Ouvrir dossier sortie",
            ["ViewEncodingCommandText"] = "Examiner param.",
            ["InterruptUpstreamText"] = "Interrompre amont",
            ["InterruptEncoderText"] = "Interrompre encodeur",
            ["CloseAfterDoneText"] = "Fermer",
            ["EncodingCommandTitle"] = "Commande d'encodage",
            ["PhysicalMemoryTopText"] = "Mémoire physique",
            ["PhysicalMemoryBottomText"] = "Total XX GB",
            ["CommittedMemoryTopText"] = "Mémoire validée",
            ["CommittedMemoryBottomText"] = "Limite XX GB",
            ["WorkingSetPeakTopText"] = "Pic ensemble de travail",
            ["WorkingSetPeakBottomText"] = "Actuel XX GB",
            ["PageFileTopText"] = "Fichier d'échange",
            ["PageFileBottomText"] = "Total XX GB",
            ["PageFaultTopText"] = "Défauts page",
            ["PageFaultBottomText"] = "Matér. & Logic.",
            ["RAMStressTopText"] = "Stress RAM",
            ["RAMStressMediumText"] = "Moy.",
            ["RAMStressHighText"] = "Élevé",
            ["BlockTooltipFormat"] = "Bloc plage {0}",
            ["PipeErrorPrefix"] = "Erreur pipe : ",
            ["ReadyToStartText"] = "Prêt",
            ["EncodingText"] = "Encodage",
            ["AudioEncodingText"] = "Encodage audio",
            ["MuxingText"] = "Multiplexage",
            ["InterruptedText"] = "Interrompu",
            ["FailedText"] = "Échec encodage",
            ["CompletedText"] = "Encodage terminé",
            ["ResetUsageStatusText"] = "Valeurs réinitialisées",
            ["InterruptingUpstreamText"] = "Interruption amont",
            ["InterruptingEncoderText"] = "Interruption encodeur",
            ["ModeText"] = "mode",
            ["StopQueueConfirmTitle"] = "Arrêter la file",
            ["StopQueueConfirmMessage"] = "La tâche actuelle a été interrompue. Arrêter toute la file d'attente ?"
        };
        Data["es"] = new(Data["en"])
        {
            ["ProgressTitle"] = "Progreso",
            ["ProgressReportTitle"] = "Flujo de progreso",
            ["MemoryTitle"] = "RAM uso",
            ["StderrTitle"] = "Registro del proceso",
            ["DragLogReportHint"] = "Arrastre el borde de la ventana para redimensionar el log; arrastre el divisor para ajustar ancho",
            ["CurrentSizeLabel"] = "Tamaño actual / GB",
            ["EstimatedSizeLabel"] = "Total estimado / GB",
            ["WrittenFramesLabel"] = "Fotogramas escritos",
            ["SampleIntervalLabel"] = "Intervalo de muestreo",
            ["StartedAtLabel"] = "Inicio",
            ["ElapsedLabel"] = "Transcurrido",
            ["RemainingLabel"] = "Restante",
            ["CompleteAtLabel"] = "ETA",
            ["ArgsLabel"] = "Nombre de otro preajuste",
            ["SmallNoteText"] = "Este programa no soporta guardar/cargar progreso; interrumpir descartará el avance",
            ["EnableMuxText"] = "Multiplexar tras codificar (2a parte de comando; requiere ffmpeg)",
            ["RichTextModeText"] = "Análisis de texto enriquecido",
            ["DistributionUpstreamLabel"] = "Programa aguas arriba",
            ["DistributionDownstreamLabel"] = "Programa aguas abajo",
            ["DistributionCacheLabel"] = "Caché del sistema",
            ["DistributionAvailableLabel"] = "Espacio libre",
            ["MemoryRangeLegendTitle"] = "Leyenda",
            ["SampleIntervalTickLabels"] = "Detener|60 s|120 s|180 s|240 s",
            ["SampleIntervalZeroText"] = "Detener",
            ["RotateLogFontSizeText"] = "Cambiar tamaño del log",
            ["CopyUpstreamLogText"] = "Copiar log aguas arriba",
            ["CopyDownstreamLogText"] = "Copiar log aguas abajo",
            ["SaveLogsText"] = "Guardar logs",
            ["OpenTxtText"] = "Abrir TXT",
            ["OpenOutputDirectoryText"] = "Abrir carpeta de salida",
            ["ViewEncodingCommandText"] = "Revisar parám.",
            ["InterruptUpstreamText"] = "Interrumpir aguas arriba",
            ["InterruptEncoderText"] = "Interrumpir codificador",
            ["CloseAfterDoneText"] = "Cerrar",
            ["EncodingCommandTitle"] = "Comando de codificación",
            ["PhysicalMemoryTopText"] = "Memoria física",
            ["PhysicalMemoryBottomText"] = "Total XX GB",
            ["CommittedMemoryTopText"] = "Memoria comprometida",
            ["CommittedMemoryBottomText"] = "Límite XX GB",
            ["WorkingSetPeakTopText"] = "Pico conjunto de trabajo",
            ["WorkingSetPeakBottomText"] = "Actual XX GB",
            ["PageFileTopText"] = "Archivo de paginación",
            ["PageFileBottomText"] = "Total XX GB",
            ["PageFaultTopText"] = "Fallos de página",
            ["PageFaultBottomText"] = "Duros y blandos",
            ["RAMStressTopText"] = "Estrés RAM",
            ["RAMStressMediumText"] = "Medio",
            ["RAMStressHighText"] = "Alto",
            ["BlockTooltipFormat"] = "Bloque {0}",
            ["PipeErrorPrefix"] = "Error de pipe: ",
            ["ReadyToStartText"] = "Listo",
            ["EncodingText"] = "Codificando",
            ["AudioEncodingText"] = "Codificando audio",
            ["MuxingText"] = "Multiplexando",
            ["InterruptedText"] = "Interrumpido",
            ["FailedText"] = "Codificación fallida",
            ["CompletedText"] = "Codificación completa",
            ["ResetUsageStatusText"] = "Valores reiniciados",
            ["InterruptingUpstreamText"] = "Interrumpiendo upstream",
            ["InterruptingEncoderText"] = "Interrumpiendo codificador",
            ["ModeText"] = "modo",
            ["StopQueueConfirmTitle"] = "Detener cola",
            ["StopQueueConfirmMessage"] = "La tarea actual ha sido interrumpida. ¿Detener toda la cola?"
        };
        Data["ja"] = new(Data["en"])
        {
            ["ProgressTitle"] = "進捗",
            ["ProgressReportTitle"] = "進捗ストリーム",
            ["MemoryTitle"] = "RAM 使用",
            ["StderrTitle"] = "プロセスログ",
            ["DragLogReportHint"] = "ウィンドウの端をドラッグしてログ領域を変更; 区切り線で幅を調整",
            ["CurrentSizeLabel"] = "現在サイズ / GB",
            ["EstimatedSizeLabel"] = "推定合計 / GB",
            ["WrittenFramesLabel"] = "書込フレーム",
            ["SampleIntervalLabel"] = "サンプル間隔",
            ["StartedAtLabel"] = "開始時刻",
            ["ElapsedLabel"] = "経過",
            ["RemainingLabel"] = "残り",
            ["CompleteAtLabel"] = "ETA",
            ["ArgsLabel"] = "その他のプリセット名",
            ["SmallNoteText"] = "このプログラムは進捗の保存・読み込みに対応していません。中断すると進捗は失われます",
            ["EnableMuxText"] = "エンコード後に多重化 (コマンド第2部、ffmpeg 必須)",
            ["RichTextModeText"] = "リッチテキスト解析",
            ["DistributionUpstreamLabel"] = "上流プログラム",
            ["DistributionDownstreamLabel"] = "下流プログラム",
            ["DistributionCacheLabel"] = "システムキャッシュ",
            ["DistributionAvailableLabel"] = "空き容量",
            ["MemoryRangeLegendTitle"] = "範囲凡例",
            ["SampleIntervalTickLabels"] = "停止|60秒|120秒|180秒|240秒",
            ["SampleIntervalZeroText"] = "停止",
            ["RotateLogFontSizeText"] = "ログ文字サイズ変更",
            ["CopyUpstreamLogText"] = "上流ログをコピー",
            ["CopyDownstreamLogText"] = "下流ログをコピー",
            ["SaveLogsText"] = "ログを保存",
            ["OpenTxtText"] = "TXTを開く",
            ["OpenOutputDirectoryText"] = "出力フォルダを開く",
            ["ViewEncodingCommandText"] = "パラメ.再表示",
            ["InterruptUpstreamText"] = "上流を中断",
            ["InterruptEncoderText"] = "エンコーダを中断",
            ["CloseAfterDoneText"] = "閉じる",
            ["EncodingCommandTitle"] = "エンコードコマンド",
            ["PhysicalMemoryTopText"] = "物理メモリ",
            ["PhysicalMemoryBottomText"] = "合計 XX GB",
            ["CommittedMemoryTopText"] = "コミット済み",
            ["CommittedMemoryBottomText"] = "上限 XX GB",
            ["WorkingSetPeakTopText"] = "WS ピーク",
            ["WorkingSetPeakBottomText"] = "現在 XX GB",
            ["PageFileTopText"] = "ページファイル",
            ["PageFileBottomText"] = "合計 XX GB",
            ["PageFaultTopText"] = "ページフォルト",
            ["PageFaultBottomText"] = "ハード+ソフト",
            ["RAMStressTopText"] = "RAM 負荷",
            ["RAMStressMediumText"] = "中",
            ["RAMStressHighText"] = "高",
            ["BlockTooltipFormat"] = "範囲ブロック {0}",
            ["PipeErrorPrefix"] = "パイプエラー: ",
            ["ReadyToStartText"] = "開始準備完了",
            ["EncodingText"] = "エンコード中",
            ["AudioEncodingText"] = "音声エンコード中",
            ["MuxingText"] = "Mux 中",
            ["InterruptedText"] = "中断済み",
            ["FailedText"] = "エンコード失敗",
            ["CompletedText"] = "エンコード完了",
            ["ResetUsageStatusText"] = "使用量をリセット",
            ["InterruptingUpstreamText"] = "上流を中断中",
            ["InterruptingEncoderText"] = "エンコーダを中断中",
            ["ModeText"] = "モード",
            ["StopQueueConfirmTitle"] = "キューを停止",
            ["StopQueueConfirmMessage"] = "現在のジョブが中断されました。キュー全体を停止しますか？"
        };
        Data["ru"] = new(Data["en"])
        {
            ["ProgressTitle"] = "Прогресс",
            ["ProgressReportTitle"] = "Поток прогресса",
            ["MemoryTitle"] = "Исп. ОЗУ",
            ["StderrTitle"] = "Журнал процесса",
            ["DragLogReportHint"] = "Перетащите край окна, чтобы изменить область лога; перетащите разделитель для ширины",
            ["CurrentSizeLabel"] = "Текущий размер / GB",
            ["EstimatedSizeLabel"] = "Оценка всего / GB",
            ["WrittenFramesLabel"] = "Записано кадров",
            ["SampleIntervalLabel"] = "Интервал опроса",
            ["StartedAtLabel"] = "Старт",
            ["ElapsedLabel"] = "Прошло",
            ["RemainingLabel"] = "Осталось",
            ["CompleteAtLabel"] = "ETA",
            ["ArgsLabel"] = "Имя другого набора настроек",
            ["SmallNoteText"] = "Программа не поддерживает сохранение прогресса; прерывание приведёт к потере хода задачи",
            ["EnableMuxText"] = "Mux после кодирования (2-я часть команды, нужен ffmpeg)",
            ["RichTextModeText"] = "Анализ форматированного текста",
            ["DistributionUpstreamLabel"] = "Апстрим",
            ["DistributionDownstreamLabel"] = "Даунстрим",
            ["DistributionCacheLabel"] = "Системный кэш",
            ["DistributionAvailableLabel"] = "Свободно",
            ["MemoryRangeLegendTitle"] = "Легенда",
            ["SampleIntervalTickLabels"] = "Стоп|60 с|120 с|180 с|240 с",
            ["SampleIntervalZeroText"] = "Стоп",
            ["RotateLogFontSizeText"] = "Сменить размер шрифта лога",
            ["CopyUpstreamLogText"] = "Копировать лог апстрима",
            ["CopyDownstreamLogText"] = "Копировать лог даунстрима",
            ["SaveLogsText"] = "Сохранять логи",
            ["OpenTxtText"] = "Открыть TXT",
            ["OpenOutputDirectoryText"] = "Открыть папку вывода",
            ["ViewEncodingCommandText"] = "Провер. парам.",
            ["InterruptUpstreamText"] = "Прервать апстрим",
            ["InterruptEncoderText"] = "Прервать кодер",
            ["CloseAfterDoneText"] = "Закрыть",
            ["EncodingCommandTitle"] = "Команда кодирования",
            ["PhysicalMemoryTopText"] = "Физическая память",
            ["PhysicalMemoryBottomText"] = "Всего XX GB",
            ["CommittedMemoryTopText"] = "Выделено",
            ["CommittedMemoryBottomText"] = "Лимит XX GB",
            ["WorkingSetPeakTopText"] = "Пик рабочего набора",
            ["WorkingSetPeakBottomText"] = "Сейчас XX GB",
            ["PageFileTopText"] = "Файл подкачки",
            ["PageFileBottomText"] = "Всего XX GB",
            ["PageFaultTopText"] = "Ошибки страниц",
            ["PageFaultBottomText"] = "Жёст.+Мягк",
            ["RAMStressTopText"] = "Нагрузка RAM",
            ["RAMStressMediumText"] = "Средн.",
            ["RAMStressHighText"] = "Высокая",
            ["BlockTooltipFormat"] = "Блок диапазона {0}",
            ["PipeErrorPrefix"] = "Ошибка pipe: ",
            ["ReadyToStartText"] = "Готово к старту",
            ["EncodingText"] = "Кодирование",
            ["AudioEncodingText"] = "Кодирование аудио",
            ["MuxingText"] = "Мультиплексирование",
            ["InterruptedText"] = "Прервано",
            ["FailedText"] = "Кодирование не удалось",
            ["CompletedText"] = "Кодирование завершено",
            ["ResetUsageStatusText"] = "Значения сброшены",
            ["InterruptingUpstreamText"] = "Прерывание upstream",
            ["InterruptingEncoderText"] = "Прерывание кодера",
            ["ModeText"] = "режим",
            ["StopQueueConfirmTitle"] = "Остановить очередь",
["StopQueueConfirmMessage"] = "Текущая задача прервана. Остановить всю очередь?"
        };
        Data["de"] = new(Data["en"])
        {
            ["ProgressTitle"] = "Fortschritt",
            ["ProgressReportTitle"] = "Fortschrittsstream",
            ["MemoryTitle"] = "RAM-Nutzung",
            ["StderrTitle"] = "Prozessprotokoll",
            ["DragLogReportHint"] = "Fensterkante ziehen für Größenänderung; Teiler ziehen für Breite",
            ["CurrentSizeLabel"] = "Aktuelle Größe / GB",
            ["EstimatedSizeLabel"] = "Geschätzte Gesamtgröße / GB",
            ["WrittenFramesLabel"] = "Geschriebene Frames",
            ["SampleIntervalLabel"] = "Sampling-Intervall",
            ["StartedAtLabel"] = "Gestartet um",
            ["ElapsedLabel"] = "Verstrichen",
            ["RemainingLabel"] = "Verbleibend",
            ["CompleteAtLabel"] = "ETA",
            ["ArgsLabel"] = "Anderer Preset-Name",
            ["SmallNoteText"] = "Dieses Programm unterstützt kein Fortschrittsspeichern; Unterbrechung verwirfort den Fortschritt",
            ["EnableMuxText"] = "Nach Kodierung multiplexen (2. Teil der Kommandozeile, ffmpeg erforderlich)",
            ["RichTextModeText"] = "Rich-Text parsen",
            ["DistributionUpstreamLabel"] = "Upstream-Programm",
            ["DistributionDownstreamLabel"] = "Downstream-Programm",
            ["DistributionCacheLabel"] = "System-Cache",
            ["DistributionAvailableLabel"] = "Verfügbarer Speicher",
            ["MemoryRangeLegendTitle"] = "Bereichslegende",
            ["SampleIntervalTickLabels"] = "Stopp|60 s|120 s|180 s|240 s",
            ["SampleIntervalZeroText"] = "Stopp",
            ["RotateLogFontSizeText"] = "Log-Schriftgröße wechseln",
            ["CopyUpstreamLogText"] = "Upstream-Log kopieren",
            ["CopyDownstreamLogText"] = "Downstream-Log kopieren",
            ["SaveLogsText"] = "Logs speichern",
            ["OpenTxtText"] = "TXT öffnen",
            ["OpenOutputDirectoryText"] = "Ausgabeordner öffnen",
            ["ViewEncodingCommandText"] = "Param. prüfen",
            ["InterruptUpstreamText"] = "Upstream unterbrechen",
            ["InterruptEncoderText"] = "Encoder unterbrechen",
            ["CloseAfterDoneText"] = "Schließen",
            ["EncodingCommandTitle"] = "Kodierungsbefehl",
            ["PhysicalMemoryTopText"] = "Physischer Speicher",
            ["PhysicalMemoryBottomText"] = "Gesamt XX GB",
            ["CommittedMemoryTopText"] = "Zugesagt",
            ["CommittedMemoryBottomText"] = "Limit XX GB",
            ["WorkingSetPeakTopText"] = "Working-Set-Peak",
            ["WorkingSetPeakBottomText"] = "Aktuell XX GB",
            ["PageFileTopText"] = "Auslagerungsdatei",
            ["PageFileBottomText"] = "Gesamt XX GB",
            ["PageFaultTopText"] = "Page-Fehler",
            ["PageFaultBottomText"] = "Hart+Weich",
            ["RAMStressTopText"] = "RAM-Auslastung",
            ["RAMStressMediumText"] = "Mittel",
            ["RAMStressHighText"] = "Hoch",
            ["BlockTooltipFormat"] = "Bereichsblock {0}",
            ["PipeErrorPrefix"] = "Pipe-Fehler: ",
            ["ReadyToStartText"] = "Bereit zum Start",
            ["EncodingText"] = "Kodierung",
            ["AudioEncodingText"] = "Audio-Kodierung",
            ["MuxingText"] = "Multiplexing",
            ["InterruptedText"] = "Unterbrochen",
            ["FailedText"] = "Kodierung fehlgeschlagen",
            ["CompletedText"] = "Kodierung abgeschlossen",
            ["ResetUsageStatusText"] = "Nutzungswerte zurückgesetzt",
            ["InterruptingUpstreamText"] = "Upstream wird unterbrochen",
            ["InterruptingEncoderText"] = "Encoder wird unterbrochen",
            ["ModeText"] = "Modus",
            ["StopQueueConfirmTitle"] = "Warteschlange stoppen",
            ["StopQueueConfirmMessage"] = "Aktueller Job wurde unterbrochen. Gesamte Warteschlange stoppen?"
        };
        Data["pt-br"] = new(Data["en"])
        {
            ["ProgressTitle"] = "Progresso",
            ["ProgressReportTitle"] = "Stream de progresso",
            ["MemoryTitle"] = "Uso de RAM",
            ["StderrTitle"] = "Log do processo",
            ["DragLogReportHint"] = "Arraste a borda da janela para redimensionar a área de log; arraste o divisor de log para ajustar a largura",
            ["CurrentSizeLabel"] = "Tamanho atual / GB",
            ["EstimatedSizeLabel"] = "Total estimado / GB",
            ["WrittenFramesLabel"] = "Quadros escritos",
            ["SampleIntervalLabel"] = "Intervalo de amostragem",
            ["StartedAtLabel"] = "Início",
            ["ElapsedLabel"] = "Decorrido",
            ["RemainingLabel"] = "Restante",
            ["CompleteAtLabel"] = "ETA",
            ["ArgsLabel"] = "Nome de outra predefinição",
            ["SmallNoteText"] = "Este programa não suporta salvar-carregar progresso; interrir descartará o progresso da tarefa",
            ["EnableMuxText"] = "Multiplexar após codificação (2ª parte da linha de comando, ffmpeg necessário)",
            ["RichTextModeText"] = "Analisar rich text",
            ["DistributionUpstreamLabel"] = "Programa upstream",
            ["DistributionDownstreamLabel"] = "Programa downstream",
            ["DistributionCacheLabel"] = "Cache do sistema",
            ["DistributionAvailableLabel"] = "Espaço disponível",
            ["MemoryRangeLegendTitle"] = "Legenda",
            ["SampleIntervalTickLabels"] = "Parar|60 s|120 s|180 s|240 s",
            ["SampleIntervalZeroText"] = "Parar",
            ["RotateLogFontSizeText"] = "Alternar tamanho da fonte do log",
            ["CopyUpstreamLogText"] = "Copiar log upstream",
            ["CopyDownstreamLogText"] = "Copiar log downstream",
            ["SaveLogsText"] = "Salvar logs",
            ["OpenTxtText"] = "Abrir TXT",
            ["OpenOutputDirectoryText"] = "Abrir pasta de saída",
            ["ViewEncodingCommandText"] = "Revisar param.",
            ["InterruptUpstreamText"] = "Interromper upstream",
            ["InterruptEncoderText"] = "Interromper codificador",
            ["CloseAfterDoneText"] = "Fechar",
            ["EncodingCommandTitle"] = "Comando de codificação",
            ["PhysicalMemoryTopText"] = "Memória física",
            ["PhysicalMemoryBottomText"] = "Total XX GB",
            ["CommittedMemoryTopText"] = "Memória confirmada",
            ["CommittedMemoryBottomText"] = "Limite XX GB",
            ["WorkingSetPeakTopText"] = "Pico do conjunto de trabalho",
            ["WorkingSetPeakBottomText"] = "Atual XX GB",
            ["PageFileTopText"] = "Arquivo de paginação",
            ["PageFileBottomText"] = "Total XX GB",
            ["PageFaultTopText"] = "Faltas de página",
            ["PageFaultBottomText"] = "Ríg+Flex.",
            ["RAMStressTopText"] = "Estresse de RAM",
            ["RAMStressMediumText"] = "Médio",
            ["RAMStressHighText"] = "Alto",
            ["BlockTooltipFormat"] = "Bloco de intervalo {0}",
            ["PipeErrorPrefix"] = "Erro de pipe: ",
            ["ReadyToStartText"] = "Pronto para iniciar",
            ["EncodingText"] = "Codificando",
            ["AudioEncodingText"] = "Codificando áudio",
            ["MuxingText"] = "Multiplexando",
            ["InterruptedText"] = "Interrompido",
            ["FailedText"] = "Falha na codificação",
            ["CompletedText"] = "Codificação concluída",
            ["ResetUsageStatusText"] = "Valores de uso redefinidos",
            ["InterruptingUpstreamText"] = "Interrompendo upstream",
            ["InterruptingEncoderText"] = "Interrompendo codificador",
            ["ModeText"] = "modo",
            ["StopQueueConfirmTitle"] = "Parar fila",
            ["StopQueueConfirmMessage"] = "A tarefa atual foi interrompida. Parar toda a fila?"
        };
    }

    public const string WindowTitle = "1cenc Encoding Monitor";
    public const string WindowTitleSampleMode = "1cenc Encoding Monitor (Sample mode)";
    public string ProgressTitle { get; }
    public string ProgressReportTitle { get; }
    public string MemoryTitle { get; }
    public string StderrTitle { get; }
    public string DragLogReportHint { get; }
    public string CurrentSizeLabel { get; }
    public string EstimatedSizeLabel { get; }
    public string WrittenFramesLabel { get; }
    public string SampleIntervalLabel { get; }
    public string StartedAtLabel { get; }
    public string ElapsedLabel { get; }
    public string RemainingLabel { get; }
    public string CompleteAtLabel { get; }
    public string ArgsLabel { get; }
    public string SmallNoteText { get; }
    public string EnableMuxText { get; }
    public string RichTextModeText { get; }
    public string OpusAudioCommandHintFormat { get; }
    public string DistributionUpstreamLabel { get; }
    public string DistributionDownstreamLabel { get; }
    public string DistributionCacheLabel { get; }
    public string DistributionAvailableLabel { get; }
    public string MemoryRangeLegendTitle { get; }
    public string[] SampleIntervalTickLabels { get; }
    public string SampleIntervalZeroText { get; }
    public string RotateLogFontSizeText { get; }
    public string CopyUpstreamLogText { get; }
    public string CopyDownstreamLogText { get; }
    public string SaveLogsText { get; }
    public string OpenTxtText { get; }
    public string OpenOutputDirectoryText { get; }
    public string ViewEncodingCommandText { get; }
    public string InterruptUpstreamText { get; }
    public string InterruptEncoderText { get; }
    public string CloseAfterDoneText { get; }
    public string EncodingCommandTitle { get; }
    public string PhysicalMemoryTopText { get; }
    public string PhysicalMemoryBottomText { get; }
    public string CommittedMemoryTopText { get; }
    public string CommittedMemoryBottomText { get; }
    public string WorkingSetPeakTopText { get; }
    public string WorkingSetPeakBottomText { get; }
    public string PageFileTopText { get; }
    public string PageFileBottomText { get; }
    public string PageFaultTopText { get; }
    public string PageFaultBottomText { get; }
    public string RAMStressTopText { get; }
    public string RAMStressMediumText { get; }
    public string RAMStressHighText { get; }
    public string BlockTooltipFormat { get; }
    public string PipeErrorPrefix { get; }
    public string ReadyToStartText { get; }
    public string EncodingText { get; } // Used in queue/concat/repart mode
    public string AudioEncodingText { get; } // Used in queue/concat/repart mode
    public string MuxingText { get; } // Used in queue/concat/repart mode
    public string InterruptedText { get; }
    public string FailedText { get; }
    public string CompletedText { get; }
    public string ResetUsageStatusText { get; }
    public string InterruptingUpstreamText { get; }
    public string InterruptingEncoderText { get; }
    public string ModeText { get; }
    public string NotAvailableText { get; }
    public string ABRText { get; }
    public string CRFText { get; }

    public string StopQueueConfirmTitle { get; }
    public string StopQueueConfirmMessage { get; }

    public EncodingMonitorModalLangProvider(string languageCode) : base(languageCode, Data)
    {
        ProgressTitle = this["ProgressTitle"];
        ProgressReportTitle = this["ProgressReportTitle"];
        MemoryTitle = this["MemoryTitle"];
        StderrTitle = this["StderrTitle"];
        DragLogReportHint = this["DragLogReportHint"];
        CurrentSizeLabel = this["CurrentSizeLabel"];
        EstimatedSizeLabel = this["EstimatedSizeLabel"];
        WrittenFramesLabel = this["WrittenFramesLabel"];
        SampleIntervalLabel = this["SampleIntervalLabel"];
        StartedAtLabel = this["StartedAtLabel"];
        ElapsedLabel = this["ElapsedLabel"];
        RemainingLabel = this["RemainingLabel"];
        CompleteAtLabel = this["CompleteAtLabel"];
        ArgsLabel = this["ArgsLabel"];
        SmallNoteText = this["SmallNoteText"];
        EnableMuxText = this["EnableMuxText"];
        RichTextModeText = this["RichTextModeText"];
        OpusAudioCommandHintFormat = "♫Opus 320Kbps: {0}";
        DistributionUpstreamLabel = this["DistributionUpstreamLabel"];
        DistributionDownstreamLabel = this["DistributionDownstreamLabel"];
        DistributionCacheLabel = this["DistributionCacheLabel"];
        DistributionAvailableLabel = this["DistributionAvailableLabel"];
        MemoryRangeLegendTitle = this["MemoryRangeLegendTitle"];
        SampleIntervalTickLabels = this["SampleIntervalTickLabels"].Split('|');
        SampleIntervalZeroText = this["SampleIntervalZeroText"];
        RotateLogFontSizeText = this["RotateLogFontSizeText"];
        CopyUpstreamLogText = this["CopyUpstreamLogText"];
        CopyDownstreamLogText = this["CopyDownstreamLogText"];
        SaveLogsText = this["SaveLogsText"];
        OpenTxtText = this["OpenTxtText"];
        OpenOutputDirectoryText = this["OpenOutputDirectoryText"];
        ViewEncodingCommandText = this["ViewEncodingCommandText"];
        InterruptUpstreamText = this["InterruptUpstreamText"];
        InterruptEncoderText = this["InterruptEncoderText"];
        CloseAfterDoneText = this["CloseAfterDoneText"];
        EncodingCommandTitle = this["EncodingCommandTitle"];
        PhysicalMemoryTopText = this["PhysicalMemoryTopText"];
        PhysicalMemoryBottomText = this["PhysicalMemoryBottomText"];
        CommittedMemoryTopText = this["CommittedMemoryTopText"];
        CommittedMemoryBottomText = this["CommittedMemoryBottomText"];
        WorkingSetPeakTopText = this["WorkingSetPeakTopText"];
        WorkingSetPeakBottomText = this["WorkingSetPeakBottomText"];
        PageFileTopText = this["PageFileTopText"];
        PageFileBottomText = this["PageFileBottomText"];
        PageFaultTopText = this["PageFaultTopText"];
        PageFaultBottomText = this["PageFaultBottomText"];
        RAMStressTopText = this["RAMStressTopText"];
        RAMStressMediumText = this["RAMStressMediumText"];
        RAMStressHighText = this["RAMStressHighText"];
        BlockTooltipFormat = this["BlockTooltipFormat"];
        PipeErrorPrefix = this["PipeErrorPrefix"];
        ReadyToStartText = this["ReadyToStartText"];
        EncodingText = this["EncodingText"];
        AudioEncodingText = this["AudioEncodingText"];
        MuxingText = this["MuxingText"];
        InterruptedText = this["InterruptedText"];
        FailedText = this["FailedText"];
        CompletedText = this["CompletedText"];
        ResetUsageStatusText = this["ResetUsageStatusText"];
        InterruptingUpstreamText = this["InterruptingUpstreamText"];
        InterruptingEncoderText = this["InterruptingEncoderText"];
        ModeText = this["ModeText"];
        NotAvailableText = "N/A";
        ABRText = "ABR";
        CRFText = "CRF";

        StopQueueConfirmTitle = this["StopQueueConfirmTitle"];
        StopQueueConfirmMessage = this["StopQueueConfirmMessage"];
    }
}
