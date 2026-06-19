namespace OneColumnEncoder.Models;

public class EncodingMonitorModalLangProviderM
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
            ["SmallNoteText"] = "This program does not support progress quantization; interrupting will discard task progress.",
            ["EnableMuxText"] = "Multiplex after encoding (the 2nd part of commandline, ffmpeg required)",
            ["MuxTimebaseHint"] = "Muxed output inherits timebase from source, so quality metrics can align frames correctly, w/out manually calculating GCD of fractions",
            ["DistributionUpstreamLabel"] = "Upstream program",
            ["DistributionDownstreamLabel"] = "Downstream program",
            ["DistributionCacheLabel"] = "System cache",
            ["DistributionAvailableLabel"] = "Available Space",
            ["MemoryRangeLegendTitle"] = "Range legend",
            ["SampleIntervalTickLabels"] = "0 (Real Time)|60S|120S|180S|240",
            ["ContinueMonitoringText"] = "Continue monitoring",
            ["FreezeContinueText"] = "Freeze / Continue",
            ["UpdateUsageText"] = "Update now",
            ["RotateLogFontSizeText"] = "Rotate log fontsize",
            ["SaveUpstreamStderrText"] = "Save upstream log",
            ["SaveDownstreamStderrText"] = "Save downstream log",

            ["OpenOutputDirectoryText"] = "Open output folder",
            ["ViewEncodingCommandText"] = "Revisit commandline",
            ["InterruptUpstreamText"] = "Interrupt upstream",
            ["InterruptEncoderText"] = "Interrupt encoder",
            ["CloseAfterDoneText"] = "Close",
            ["EncodingCommandTitle"] = "Encoding Command",
            ["PhysicalMemoryTopText"] = "Physical memory",
            ["PhysicalMemoryBottomText"] = "Total XX GB",
["CommittedMemoryTopText"] = "Закоммичено",
            ["CommittedMemoryBottomText"] = "Limit XX GB",
            ["WorkingSetPeakTopText"] = "Working set peak",
            ["WorkingSetPeakBottomText"] = "Current XX GB",
["PageFileTopText"] = "Файл подкачки",
            ["PageFileBottomText"] = "Total XX GB",
["PageFaultTopText"] = "Ошибки страниц",
["PageFaultBottomText"] = "Жёсткие и мягкие",
            ["RAMStressTopText"] = "RAM stress",
            ["RAMStressMediumText"] = "Mid",
            ["RAMStressHighText"] = "High",
            ["BlockTooltipFormat"] = "Range block {0}",
            ["PipeErrorPrefix"] = "Pipe error: ",
            ["ReadyToStartText"] = "Ready to start",
            ["EncodingText"] = "Encoding",
["MuxingText"] = "Муксирование",
            ["InterruptedText"] = "Interrupted",
            ["FailedText"] = "Encoding failed",
            ["CompletedText"] = "Encoding completed",
            ["ResetUsageStatusText"] = "Usage values reset",
            ["InterruptingUpstreamText"] = "Interrupting upstream",
            ["InterruptingEncoderText"] = "Interrupting encoder",
            ["ModeText"] = "mode",
            ["NotAvailableText"] = "N/A",
            ["ABRText"] = "ABR",
            ["CRFText"] = "CRF",

            ["QueueSidebarTitle"] = "Queue",
            ["QueueSidebarStartBatchText"] = "Start batch",
            ["QueueSidebarCancelAllText"] = "Cancel all",
            ["QueueSidebarCollapseText"] = "Collapse",
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
            ["SmallNoteText"] = "本程序不支持进度量化；中断将丢弃任务进度。",
            ["EnableMuxText"] = "压制完成后封装视频流（先前命令行的第二部分，需导入 ffmpeg）",
            ["MuxTimebaseHint"] = "自动封装会使用与源视频相同的时间基，以便画质指标跑分时正确对齐帧，而无需计算两个分数的最大公约数。",
            ["DistributionUpstreamLabel"] = "上游程序",
            ["DistributionDownstreamLabel"] = "下游程序",
            ["DistributionCacheLabel"] = "系统缓存",
            ["DistributionAvailableLabel"] = "可用空间",
            ["MemoryRangeLegendTitle"] = "范围图例",
            ["SampleIntervalTickLabels"] = "0（实时）|60秒|120秒|180秒|240秒",
            ["ContinueMonitoringText"] = "继续监测",
            ["FreezeContinueText"] = "冻结 / 继续监测",
            ["UpdateUsageText"] = "立即检查",
            ["RotateLogFontSizeText"] = "轮换日志字号",
            ["SaveUpstreamStderrText"] = "保存上游日志",
            ["SaveDownstreamStderrText"] = "保存下游日志",

            ["OpenOutputDirectoryText"] = "打开输出目录",
            ["ViewEncodingCommandText"] = "查看编码参数",
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
            ["MuxingText"] = "正在封装",
            ["InterruptedText"] = "已中断",
            ["FailedText"] = "压制失败",
            ["CompletedText"] = "压制完成",
            ["ResetUsageStatusText"] = "已重置占用值",
            ["InterruptingUpstreamText"] = "正在中断上游程序",
            ["InterruptingEncoderText"] = "正在中断编码器",
            ["ModeText"] = "模式",
            ["NotAvailableText"] = "N/A",
            ["ABRText"] = "ABR",
            ["CRFText"] = "CRF",
        },
        ["zh-tw"] = new()
        {
            ["ProgressTitle"] = "進度",
            ["ProgressReportTitle"] = "進度流",
            ["MemoryTitle"] = "記憶占用",

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
            ["SmallNoteText"] = "本程式不支援進度量化；中斷將丟棄任務進度。",
            ["EnableMuxText"] = "壓製完成後封裝影片串流（先前命令行的第二部分，需導入 ffmpeg）",
            ["MuxTimebaseHint"] = "自動封裝會使用與源影片相同的時間基，以便畫質指標跑分時正確對齊幀，而無需計算兩個分數的最大公約數。",
            ["DistributionUpstreamLabel"] = "上游程式",
            ["DistributionDownstreamLabel"] = "下游程式",
            ["DistributionCacheLabel"] = "系統快取",
            ["DistributionAvailableLabel"] = "可用空間",
            ["MemoryRangeLegendTitle"] = "範圍圖例",
            ["SampleIntervalTickLabels"] = "0（即時）|60秒|120秒|180秒|240秒",
            ["ContinueMonitoringText"] = "繼續監測",
            ["FreezeContinueText"] = "凍結 / 繼續監測",
            ["UpdateUsageText"] = "立即檢查",
            ["RotateLogFontSizeText"] = "輪換日誌字型大小",
            ["SaveUpstreamStderrText"] = "保存上游日誌",
            ["SaveDownstreamStderrText"] = "保存下游日誌",

            ["OpenOutputDirectoryText"] = "開啟輸出資料夾",
            ["ViewEncodingCommandText"] = "檢視編碼參數",
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
            ["MuxingText"] = "正在封裝",
            ["InterruptedText"] = "已中斷",
            ["FailedText"] = "壓制失敗",
            ["CompletedText"] = "壓制完成",
            ["ResetUsageStatusText"] = "已重置占用值",
            ["InterruptingUpstreamText"] = "正在中斷上游程式",
            ["InterruptingEncoderText"] = "正在中斷編碼器",
            ["ModeText"] = "模式",
            ["NotAvailableText"] = "N/A",
            ["ABRText"] = "ABR",
            ["CRFText"] = "CRF",
        }
    };

    static EncodingMonitorModalLangProviderM()
    {
        Data["fr"] = new(Data["en"])
        {
            ["ProgressTitle"] = "Progression",
            ["ProgressReportTitle"] = "Flux de progression",
            ["MemoryTitle"] = "RAM utilisée",
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
            ["SmallNoteText"] = "Ce programme ne quantifie pas la progression; interrompre supprimera l'avancement.",
            ["EnableMuxText"] = "Multiplexer après encodage (2e partie de commande, ffmpeg requis)",
            ["MuxTimebaseHint"] = "La sortie muxée hérite du timebase source; les métriques qualité s'alignent sans calcul manuel de PGCD.",
            ["DistributionUpstreamLabel"] = "Programme amont",
            ["DistributionDownstreamLabel"] = "Programme aval",
            ["DistributionCacheLabel"] = "Cache système",
            ["DistributionAvailableLabel"] = "Espace libre",
            ["MemoryRangeLegendTitle"] = "Légende",
            ["SampleIntervalTickLabels"] = "0 (temps réel)|60 s|120 s|180 s|240 s",
            ["ContinueMonitoringText"] = "Continuer le suivi",
            ["FreezeContinueText"] = "Figer / Continuer",
            ["UpdateUsageText"] = "Mettre à jour",
            ["RotateLogFontSizeText"] = "Changer taille police log",
            ["SaveUpstreamStderrText"] = "Sauver log amont",
            ["SaveDownstreamStderrText"] = "Sauver log aval",
            ["OpenOutputDirectoryText"] = "Ouvrir dossier sortie",
            ["ViewEncodingCommandText"] = "Revoir la commande",
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
            ["PageFaultBottomText"] = "Matérielles et logicielles",
            ["RAMStressTopText"] = "Stress RAM",
            ["RAMStressMediumText"] = "Moy.",
            ["RAMStressHighText"] = "Élevé",
            ["BlockTooltipFormat"] = "Bloc plage {0}",
            ["PipeErrorPrefix"] = "Erreur pipe : ",
            ["ReadyToStartText"] = "Prêt",
            ["EncodingText"] = "Encodage",
            ["MuxingText"] = "Multiplexage",
            ["InterruptedText"] = "Interrompu",
            ["FailedText"] = "Échec encodage",
            ["CompletedText"] = "Encodage terminé",
            ["ResetUsageStatusText"] = "Valeurs réinitialisées",
            ["InterruptingUpstreamText"] = "Interruption amont",
            ["InterruptingEncoderText"] = "Interruption encodeur",
            ["ModeText"] = "mode",
            ["NotAvailableText"] = "N/A"
        };
        Data["es"] = new(Data["en"])
        {
            ["ProgressTitle"] = "Progreso",
            ["ProgressReportTitle"] = "Flujo de progreso",
            ["MemoryTitle"] = "Uso de RAM",
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
            ["SmallNoteText"] = "Este programa no cuantifica progreso; interrumpir descarta el avance.",
            ["EnableMuxText"] = "Multiplexar tras codificar (2a parte de comando; requiere ffmpeg)",
            ["MuxTimebaseHint"] = "La salida mux hereda el timebase de la fuente para alinear métricas sin calcular MCD.",
["DistributionUpstreamLabel"] = "Programa aguas arriba",
            ["DistributionDownstreamLabel"] = "Programa aguas abajo",
            ["DistributionCacheLabel"] = "Caché del sistema",
            ["DistributionAvailableLabel"] = "Espacio libre",
            ["MemoryRangeLegendTitle"] = "Leyenda",
            ["SampleIntervalTickLabels"] = "0 (tiempo real)|60 s|120 s|180 s|240 s",
            ["ContinueMonitoringText"] = "Seguir monitorizando",
            ["FreezeContinueText"] = "Congelar / seguir",
            ["UpdateUsageText"] = "Actualizar",
            ["RotateLogFontSizeText"] = "Cambiar tamaño del log",
            ["SaveUpstreamStderrText"] = "Guardar log aguas arriba",
            ["SaveDownstreamStderrText"] = "Guardar log aguas abajo",
            ["OpenOutputDirectoryText"] = "Abrir carpeta de salida",
            ["ViewEncodingCommandText"] = "Ver comando",
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
            ["MuxingText"] = "Multiplexando",
            ["InterruptedText"] = "Interrumpido",
            ["FailedText"] = "Codificación fallida",
            ["CompletedText"] = "Codificación completa",
            ["ResetUsageStatusText"] = "Valores reiniciados",
            ["InterruptingUpstreamText"] = "Interrumpiendo upstream",
            ["InterruptingEncoderText"] = "Interrumpiendo codificador",
            ["ModeText"] = "modo",
            ["NotAvailableText"] = "N/D"
        };
        Data["ja"] = new(Data["en"])
        {
            ["ProgressTitle"] = "進捗",
            ["ProgressReportTitle"] = "進捗ストリーム",
            ["MemoryTitle"] = "RAM 使用量",
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
            ["SmallNoteText"] = "このプログラムは進捗量子化非対応です。中断すると進捗は破棄されます。",
            ["EnableMuxText"] = "エンコード後に多重化 (コマンド第2部、ffmpeg 必須)",
            ["MuxTimebaseHint"] = "Mux 出力はソース timebase を継承し、品質指標のフレーム照合を容易にします。",
            ["DistributionUpstreamLabel"] = "上流プログラム",
            ["DistributionDownstreamLabel"] = "下流プログラム",
            ["DistributionCacheLabel"] = "システムキャッシュ",
            ["DistributionAvailableLabel"] = "空き容量",
            ["MemoryRangeLegendTitle"] = "範囲凡例",
            ["SampleIntervalTickLabels"] = "0 (リアルタイム)|60秒|120秒|180秒|240秒",
            ["ContinueMonitoringText"] = "監視を続行",
            ["FreezeContinueText"] = "停止 / 続行",
            ["UpdateUsageText"] = "今すぐ更新",
            ["RotateLogFontSizeText"] = "ログ文字サイズ変更",
            ["SaveUpstreamStderrText"] = "上流ログ保存",
            ["SaveDownstreamStderrText"] = "下流ログ保存",
            ["OpenOutputDirectoryText"] = "出力フォルダを開く",
            ["ViewEncodingCommandText"] = "コマンド再表示",
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
            ["PageFaultBottomText"] = "ハードとソフト",
            ["RAMStressTopText"] = "RAM 負荷",
            ["RAMStressMediumText"] = "中",
            ["RAMStressHighText"] = "高",
            ["BlockTooltipFormat"] = "範囲ブロック {0}",
            ["PipeErrorPrefix"] = "パイプエラー: ",
            ["ReadyToStartText"] = "開始準備完了",
            ["EncodingText"] = "エンコード中",
            ["MuxingText"] = "Mux 中",
            ["InterruptedText"] = "中断済み",
            ["FailedText"] = "エンコード失敗",
            ["CompletedText"] = "エンコード完了",
            ["ResetUsageStatusText"] = "使用量をリセット",
            ["InterruptingUpstreamText"] = "上流を中断中",
            ["InterruptingEncoderText"] = "エンコーダを中断中",
            ["ModeText"] = "モード",
            ["NotAvailableText"] = "N/A"
        };
        Data["ru"] = new(Data["en"])
        {
            ["ProgressTitle"] = "Прогресс",
            ["ProgressReportTitle"] = "Поток прогресса",
            ["MemoryTitle"] = "RAM",
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
            ["SmallNoteText"] = "Прогресс не квантуется; прерывание отбросит ход задачи.",
            ["EnableMuxText"] = "Mux после кодирования (2-я часть команды, нужен ffmpeg)",
            ["MuxTimebaseHint"] = "Mux-вывод наследует timebase источника, чтобы метрики качества точно совпадали по кадрам.",
["DistributionUpstreamLabel"] = "Апстрим",
            ["DistributionDownstreamLabel"] = "Даунстрим",
            ["DistributionCacheLabel"] = "Системный кэш",
            ["DistributionAvailableLabel"] = "Свободно",
            ["MemoryRangeLegendTitle"] = "Легенда",
            ["SampleIntervalTickLabels"] = "0 (реал. время)|60 с|120 с|180 с|240 с",
            ["ContinueMonitoringText"] = "Продолжить мониторинг",
            ["FreezeContinueText"] = "Пауза / продолжить",
            ["UpdateUsageText"] = "Обновить",
            ["RotateLogFontSizeText"] = "Сменить размер шрифта лога",
            ["SaveUpstreamStderrText"] = "Сохранить лог апстрима",
            ["SaveDownstreamStderrText"] = "Сохранить лог даунстрима",
            ["OpenOutputDirectoryText"] = "Открыть папку вывода",
            ["ViewEncodingCommandText"] = "Показать команду",
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
            ["PageFaultBottomText"] = "Жёсткие и мягкие",
            ["RAMStressTopText"] = "Нагрузка RAM",
            ["RAMStressMediumText"] = "Средн.",
            ["RAMStressHighText"] = "Высокая",
            ["BlockTooltipFormat"] = "Блок диапазона {0}",
            ["PipeErrorPrefix"] = "Ошибка pipe: ",
            ["ReadyToStartText"] = "Готово к старту",
            ["EncodingText"] = "Кодирование",
            ["MuxingText"] = "Мультиплексирование",
            ["InterruptedText"] = "Прервано",
            ["FailedText"] = "Кодирование не удалось",
            ["CompletedText"] = "Кодирование завершено",
            ["ResetUsageStatusText"] = "Значения сброшены",
            ["InterruptingUpstreamText"] = "Прерывание upstream",
            ["InterruptingEncoderText"] = "Прерывание кодера",
            ["ModeText"] = "режим",
            ["NotAvailableText"] = "N/A"
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
    public string MuxTimebaseHint { get; }
    public string DistributionUpstreamLabel { get; }
    public string DistributionDownstreamLabel { get; }
    public string DistributionCacheLabel { get; }
    public string DistributionAvailableLabel { get; }
    public string MemoryRangeLegendTitle { get; }
    public string[] SampleIntervalTickLabels { get; }
    public string ContinueMonitoringText { get; }
    public string FreezeContinueText { get; }
    public string UpdateUsageText { get; }
    public string RotateLogFontSizeText { get; }
    public string SaveUpstreamStderrText { get; }
    public string SaveDownstreamStderrText { get; }
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
    public string EncodingText { get; }
    public string MuxingText { get; }
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

    public string QueueSidebarTitle { get; }
    public string QueueSidebarStartBatchText { get; }
    public string QueueSidebarCancelAllText { get; }
    public string QueueSidebarCollapseText { get; }

    public string LanguageCode { get; }
    private readonly Dictionary<string, string> _d;

    public string this[string key] =>
        _d.TryGetValue(key, out string? value)
            ? value
            : Data["en"].TryGetValue(key, out string? fallback)
                ? fallback
                : key;

    public EncodingMonitorModalLangProviderM(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];

        ProgressTitle = _d["ProgressTitle"];
        ProgressReportTitle = _d["ProgressReportTitle"];
        MemoryTitle = _d["MemoryTitle"];
        StderrTitle = _d["StderrTitle"];
        DragLogReportHint = _d["DragLogReportHint"];
        CurrentSizeLabel = _d["CurrentSizeLabel"];
        EstimatedSizeLabel = _d["EstimatedSizeLabel"];
        WrittenFramesLabel = _d["WrittenFramesLabel"];
        SampleIntervalLabel = _d["SampleIntervalLabel"];
        StartedAtLabel = _d["StartedAtLabel"];
        ElapsedLabel = _d["ElapsedLabel"];
        RemainingLabel = _d["RemainingLabel"];
        CompleteAtLabel = _d["CompleteAtLabel"];
        ArgsLabel = _d["ArgsLabel"];
        SmallNoteText = _d["SmallNoteText"];
        EnableMuxText = _d["EnableMuxText"];
        MuxTimebaseHint = _d["MuxTimebaseHint"];
        DistributionUpstreamLabel = _d["DistributionUpstreamLabel"];
        DistributionDownstreamLabel = _d["DistributionDownstreamLabel"];
        DistributionCacheLabel = _d["DistributionCacheLabel"];
        DistributionAvailableLabel = _d["DistributionAvailableLabel"];
        MemoryRangeLegendTitle = _d["MemoryRangeLegendTitle"];
        SampleIntervalTickLabels = _d["SampleIntervalTickLabels"].Split('|');
        ContinueMonitoringText = _d["ContinueMonitoringText"];
        FreezeContinueText = _d["FreezeContinueText"];
        UpdateUsageText = _d["UpdateUsageText"];
        RotateLogFontSizeText = _d["RotateLogFontSizeText"];
        SaveUpstreamStderrText = _d["SaveUpstreamStderrText"];
        SaveDownstreamStderrText = _d["SaveDownstreamStderrText"];
        OpenOutputDirectoryText = _d["OpenOutputDirectoryText"];
        ViewEncodingCommandText = _d["ViewEncodingCommandText"];
        InterruptUpstreamText = _d["InterruptUpstreamText"];
        InterruptEncoderText = _d["InterruptEncoderText"];
        CloseAfterDoneText = _d["CloseAfterDoneText"];
        EncodingCommandTitle = _d["EncodingCommandTitle"];
        PhysicalMemoryTopText = _d["PhysicalMemoryTopText"];
        PhysicalMemoryBottomText = _d["PhysicalMemoryBottomText"];
        CommittedMemoryTopText = _d["CommittedMemoryTopText"];
        CommittedMemoryBottomText = _d["CommittedMemoryBottomText"];
        WorkingSetPeakTopText = _d["WorkingSetPeakTopText"];
        WorkingSetPeakBottomText = _d["WorkingSetPeakBottomText"];
        PageFileTopText = _d["PageFileTopText"];
        PageFileBottomText = _d["PageFileBottomText"];
        PageFaultTopText = _d["PageFaultTopText"];
        PageFaultBottomText = _d["PageFaultBottomText"];
        RAMStressTopText = _d["RAMStressTopText"];
        RAMStressMediumText = _d["RAMStressMediumText"];
        RAMStressHighText = _d["RAMStressHighText"];
        BlockTooltipFormat = _d["BlockTooltipFormat"];
        PipeErrorPrefix = _d["PipeErrorPrefix"];
        ReadyToStartText = _d["ReadyToStartText"];
        EncodingText = _d["EncodingText"];
        MuxingText = _d["MuxingText"];
        InterruptedText = _d["InterruptedText"];
        FailedText = _d["FailedText"];
        CompletedText = _d["CompletedText"];
        ResetUsageStatusText = _d["ResetUsageStatusText"];
        InterruptingUpstreamText = _d["InterruptingUpstreamText"];
        InterruptingEncoderText = _d["InterruptingEncoderText"];
        ModeText = _d["ModeText"];
        NotAvailableText = _d["NotAvailableText"];
        ABRText = _d["ABRText"];
        CRFText = _d["CRFText"];

        QueueSidebarTitle = this["QueueSidebarTitle"];
        QueueSidebarStartBatchText = this["QueueSidebarStartBatchText"];
        QueueSidebarCancelAllText = this["QueueSidebarCancelAllText"];
        QueueSidebarCollapseText = this["QueueSidebarCollapseText"];
    }
}
