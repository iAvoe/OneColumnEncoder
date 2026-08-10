namespace OneColumnEncoder.Models.Lang;

public class ClipRangeSelectorLangProvider : LangProviderBase
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["TimelineSectionTitle"] = "Time Axis Segment",
            ["SelectionHintText"] = "Drag the handle to choose the segment position",
            ["DurationSectionTitle"] = "Duration Settings",
            ["ClipLengthLabel"] = "Duration (s)",
            ["StartTimeLabel"] = "Start Time",
            ["ClipDurationLabel"] = "Segment Duration",
            ["EndTimeLabel"] = "End Time",
            ["TimeFormatText"] = "hh:mm:ss.sss",
            ["StartFrameLabel"] = "Start Frame/Field",
            ["ClipFrameCountLabel"] = "Frame/Field Duration",
            ["EndFrameLabel"] = "End Frame/Field",
            ["FrameFormatText"] = "frames|fields",
            ["Note2Text"] = "Mismatched clip durations, time-bases prevents alignment, thus can't do quality metric benchmarks",
            ["ConfirmButtonText"] = "Encode Sample",
            ["SummaryDurationLabel"] = "Duration",
            ["SummaryTotalFramesLabel"] = "Total Frames",
            ["SummaryFrameRateLabel"] = "Frame Rate",
            ["SummarySecondsUnit"] = "s",
            ["SummaryProgressive"] = "Progressive",
            ["SummaryInterlaced"] = "Interlaced",
            ["SummaryUnknown"] = "Unknown",
            ["SummaryConstantFrameRate"] = "Constant",
            ["SummaryVariableFrameRate"] = "Variable",
            ["SummaryFrameRateUnknown"] = "Unknown"
        },
        ["zh-cn"] = new()
        {
            ["TimelineSectionTitle"] = "时间轴取段",
            ["SelectionHintText"] = "拖拽滑块以选择取段位置",
            ["DurationSectionTitle"] = "时长设定",
            ["ClipLengthLabel"] = "片长（秒）",
            ["StartTimeLabel"] = "起始时间",
            ["ClipDurationLabel"] = "片段时长",
            ["EndTimeLabel"] = "结束时间",
            ["TimeFormatText"] = "hh:mm:ss.sss",
            ["StartFrameLabel"] = "起始帧数/场数",
            ["ClipFrameCountLabel"] = "帧数/场数时长",
            ["EndFrameLabel"] = "结束帧数/场数",
            ["FrameFormatText"] = "frames|fields",
            ["Note2Text"] = "由于取段时长与源视频不同，且难以对齐，因此不适合进行画质跑分",
            ["ConfirmButtonText"] = "开始打样",
            ["SummaryDurationLabel"] = "总时长",
            ["SummaryTotalFramesLabel"] = "总帧数",
            ["SummaryFrameRateLabel"] = "帧率",
            ["SummarySecondsUnit"] = "秒",
            ["SummaryProgressive"] = "逐行扫描",
            ["SummaryInterlaced"] = "隔行扫描",
            ["SummaryUnknown"] = "未知",
            ["SummaryConstantFrameRate"] = "恒定帧率",
            ["SummaryVariableFrameRate"] = "可变帧率",
            ["SummaryFrameRateUnknown"] = "未知"
        },
        ["zh-tw"] = new()
        {
            ["TimelineSectionTitle"] = "時間軸取段",
            ["SelectionHintText"] = "拖曳滑塊以選擇取段位置",
            ["DurationSectionTitle"] = "時長設定",
            ["ClipLengthLabel"] = "片長（秒）",
            ["StartTimeLabel"] = "起始時間",
            ["ClipDurationLabel"] = "片段時長",
            ["EndTimeLabel"] = "結束時間",
            ["TimeFormatText"] = "hh:mm:ss.sss",
            ["StartFrameLabel"] = "起始幀數/場數",
            ["ClipFrameCountLabel"] = "幀數/場數時長",
            ["EndFrameLabel"] = "結束幀數/場數",
            ["FrameFormatText"] = "frames|fields",
            ["Note2Text"] = "由於取段時長與源影片不同，且難以對齊，因此不適合進行畫質跑分",
            ["ConfirmButtonText"] = "開始打樣",
            ["SummaryDurationLabel"] = "總時長",
            ["SummaryTotalFramesLabel"] = "總幀數",
            ["SummaryFrameRateLabel"] = "幀率",
            ["SummarySecondsUnit"] = "秒",
            ["SummaryProgressive"] = "逐行掃描",
            ["SummaryInterlaced"] = "隔行掃描",
            ["SummaryUnknown"] = "未知",
            ["SummaryConstantFrameRate"] = "恆定幀率",
            ["SummaryVariableFrameRate"] = "可變幀率",
            ["SummaryFrameRateUnknown"] = "未知"
        }
    };

    static ClipRangeSelectorLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["TimelineSectionTitle"] = "Segment temporel",
            ["SelectionHintText"] = "Glissez la poignée pour choisir la position",
            ["DurationSectionTitle"] = "Durée",
            ["ClipLengthLabel"] = "Durée (s)",
            ["StartTimeLabel"] = "Début",
            ["ClipDurationLabel"] = "Durée du segment",
            ["EndTimeLabel"] = "Fin",
            ["StartFrameLabel"] = "Image/champ début",
            ["ClipFrameCountLabel"] = "Durée images/champs",
            ["EndFrameLabel"] = "Image/champ fin",
            ["Note2Text"] = "Durées ou bases temps divergentes empêchent l'alignement; métriques qualité non fiables.",
            ["ConfirmButtonText"] = "Encoder l'échantillon",
            ["SummaryDurationLabel"] = "Durée",
            ["SummaryTotalFramesLabel"] = "Images totales",
            ["SummaryFrameRateLabel"] = "Cadence",
            ["SummarySecondsUnit"] = "s",
            ["SummaryProgressive"] = "Progressif",
            ["SummaryInterlaced"] = "Entrelacé",
            ["SummaryUnknown"] = "Inconnu",
            ["SummaryConstantFrameRate"] = "Constante",
            ["SummaryVariableFrameRate"] = "Variable",
            ["SummaryFrameRateUnknown"] = "Inconnue"
        };
        Data["es"] = new(Data["en"])
        {
            ["TimelineSectionTitle"] = "Segmento temporal",
            ["SelectionHintText"] = "Arrastre el control para elegir la posición",
            ["DurationSectionTitle"] = "Duración",
            ["ClipLengthLabel"] = "Duración (s)",
            ["StartTimeLabel"] = "Inicio",
            ["ClipDurationLabel"] = "Duración del segmento",
            ["EndTimeLabel"] = "Fin",
            ["StartFrameLabel"] = "Fotograma/campo inicial",
            ["ClipFrameCountLabel"] = "Duración en fotogramas/campos",
            ["EndFrameLabel"] = "Fotograma/campo final",
            ["Note2Text"] = "Duraciones o bases de tiempo distintas impiden alinear; no sirven para métricas de calidad.",
            ["ConfirmButtonText"] = "Codificar muestra",
            ["SummaryDurationLabel"] = "Duración",
            ["SummaryTotalFramesLabel"] = "Fotogramas",
            ["SummaryFrameRateLabel"] = "FPS",
            ["SummarySecondsUnit"] = "s",
            ["SummaryProgressive"] = "Progresivo",
            ["SummaryInterlaced"] = "Entrelazado",
            ["SummaryUnknown"] = "Desconocido",
            ["SummaryConstantFrameRate"] = "Constante",
            ["SummaryVariableFrameRate"] = "Variable",
            ["SummaryFrameRateUnknown"] = "Desconocida"
        };
        Data["ja"] = new(Data["en"])
        {
            ["TimelineSectionTitle"] = "時間軸セグメント",
            ["SelectionHintText"] = "ハンドルをドラッグして位置を選択",
            ["DurationSectionTitle"] = "長さ設定",
            ["ClipLengthLabel"] = "長さ (秒)",
            ["StartTimeLabel"] = "開始時刻",
            ["ClipDurationLabel"] = "区間長",
            ["EndTimeLabel"] = "終了時刻",
            ["StartFrameLabel"] = "開始フレーム/フィールド",
            ["ClipFrameCountLabel"] = "フレーム/フィールド数",
            ["EndFrameLabel"] = "終了フレーム/フィールド",
            ["Note2Text"] = "区間長や時間基準が一致しないため、品質指標の比較には不向きです。",
            ["ConfirmButtonText"] = "サンプルをエンコード",
            ["SummaryDurationLabel"] = "長さ",
            ["SummaryTotalFramesLabel"] = "総フレーム",
            ["SummaryFrameRateLabel"] = "フレームレート",
            ["SummarySecondsUnit"] = "秒",
            ["SummaryProgressive"] = "プログレッシブ",
            ["SummaryInterlaced"] = "インターレース",
            ["SummaryUnknown"] = "不明",
            ["SummaryConstantFrameRate"] = "固定",
            ["SummaryVariableFrameRate"] = "可変",
            ["SummaryFrameRateUnknown"] = "不明"
        };
        Data["ru"] = new(Data["en"])
        {
            ["TimelineSectionTitle"] = "Отрезок шкалы времени",
            ["SelectionHintText"] = "Перетащите маркер, чтобы выбрать позицию",
            ["DurationSectionTitle"] = "Длительность",
            ["ClipLengthLabel"] = "Длительность (с)",
            ["StartTimeLabel"] = "Начало",
            ["ClipDurationLabel"] = "Длина отрезка",
            ["EndTimeLabel"] = "Конец",
            ["StartFrameLabel"] = "Начальный кадр/поле",
            ["ClipFrameCountLabel"] = "Кадры/поля",
            ["EndFrameLabel"] = "Конечный кадр/поле",
            ["Note2Text"] = "Разные длительности или time-base мешают выравниванию; метрики качества неприменимы.",
            ["ConfirmButtonText"] = "Кодировать образец",
            ["SummaryDurationLabel"] = "Длительность",
            ["SummaryTotalFramesLabel"] = "Кадры всего",
            ["SummaryFrameRateLabel"] = "Частота кадров",
            ["SummarySecondsUnit"] = "с",
            ["SummaryProgressive"] = "Прогрессивное",
            ["SummaryInterlaced"] = "Чересстрочное",
            ["SummaryUnknown"] = "Неизвестно",
            ["SummaryConstantFrameRate"] = "Постоянная",
            ["SummaryVariableFrameRate"] = "Переменная",
            ["SummaryFrameRateUnknown"] = "Неизвестна"
        };
        Data["de"] = new(Data["en"])
        {
            ["TimelineSectionTitle"] = "Zeitachsenausschnitt",
            ["SelectionHintText"] = "Ziehen Sie den Griff, um die Position zu wählen",
            ["DurationSectionTitle"] = "Dauereinstellung",
            ["ClipLengthLabel"] = "Dauer (s)",
            ["StartTimeLabel"] = "Startzeit",
            ["ClipDurationLabel"] = "Ausschnittsdauer",
            ["EndTimeLabel"] = "Endzeit",
            ["StartFrameLabel"] = "Start Frame/Feld",
            ["ClipFrameCountLabel"] = "Frame/Feld-Dauer",
            ["EndFrameLabel"] = "End Frame/Feld",
            ["Note2Text"] = "Unterschiedliche Dauern oder Time-Bases verhindern Ausrichtung; Qualitätsmetriken nicht anwendbar.",
            ["ConfirmButtonText"] = "Probe kodieren",
            ["SummaryDurationLabel"] = "Dauer",
            ["SummaryTotalFramesLabel"] = "Gesamtframes",
            ["SummaryFrameRateLabel"] = "Bildrate",
            ["SummarySecondsUnit"] = "s",
            ["SummaryProgressive"] = "Progressiv",
            ["SummaryInterlaced"] = "Interlaced",
            ["SummaryUnknown"] = "Unbekannt",
            ["SummaryConstantFrameRate"] = "Konstant",
            ["SummaryVariableFrameRate"] = "Variabel",
            ["SummaryFrameRateUnknown"] = "Unbekannt"
        };
        Data["ko"] = new(Data["en"])
        {
            ["TimelineSectionTitle"] = "시간 축 구간",
            ["SelectionHintText"] = "핸들을 드래그하여 구간 위치를 선택",
            ["DurationSectionTitle"] = "길이 설정",
            ["ClipLengthLabel"] = "길이 (초)",
            ["StartTimeLabel"] = "시작 시간",
            ["ClipDurationLabel"] = "구간 길이",
            ["EndTimeLabel"] = "종료 시간",
            ["TimeFormatText"] = "hh:mm:ss.sss",
            ["StartFrameLabel"] = "시작 프레임/필드",
            ["ClipFrameCountLabel"] = "프레임/필드 길이",
            ["EndFrameLabel"] = "종료 프레임/필드",
            ["FrameFormatText"] = "frames|fields",
            ["Note2Text"] = "구간 길이나 시간 기준이 일치하지 않으면 정렬이 불가하여 품질 지표 비교가 불가합니다",
            ["ConfirmButtonText"] = "샘플 인코딩",
            ["SummaryDurationLabel"] = "길이",
            ["SummaryTotalFramesLabel"] = "총 프레임",
            ["SummaryFrameRateLabel"] = "프레임레이트",
            ["SummarySecondsUnit"] = "초",
            ["SummaryProgressive"] = "프로그레시브",
            ["SummaryInterlaced"] = "인터레이스",
            ["SummaryUnknown"] = "알 수 없음",
            ["SummaryConstantFrameRate"] = "고정",
            ["SummaryVariableFrameRate"] = "가변",
            ["SummaryFrameRateUnknown"] = "알 수 없음"
        };
    }

    public const string WindowTitle = "1cenc Sample Clip";
    public string TimelineSectionTitle { get; }
    public string SelectionHintText { get; }
    public string DurationSectionTitle { get; }
    public string ClipLengthLabel { get; }
    public string StartTimeLabel { get; }
    public string ClipDurationLabel { get; }
    public string EndTimeLabel { get; }
    public string TimeFormatText { get; }
    public string StartFrameLabel { get; }
    public string ClipFrameCountLabel { get; }
    public string EndFrameLabel { get; }
    public string FrameFormatText { get; }
    public string Note2Text { get; }
    public string CancelButtonText { get; }
    public string ConfirmButtonText { get; }
    public string SummaryDurationLabel { get; }
    public string SummaryTotalFramesLabel { get; }
    public string SummaryFrameRateLabel { get; }
    public string SummarySecondsUnit { get; }
    public string SummaryProgressive { get; }
    public string SummaryInterlaced { get; }
    public string SummaryUnknown { get; }
    public string SummaryConstantFrameRate { get; }
    public string SummaryVariableFrameRate { get; }
    public string SummaryFrameRateUnknown { get; }

    public ClipRangeSelectorLangProvider(string languageCode) : base(languageCode, Data)
    {
        TimelineSectionTitle = this["TimelineSectionTitle"];
        SelectionHintText = this["SelectionHintText"];
        DurationSectionTitle = this["DurationSectionTitle"];
        ClipLengthLabel = this["ClipLengthLabel"];
        StartTimeLabel = this["StartTimeLabel"];
        ClipDurationLabel = this["ClipDurationLabel"];
        EndTimeLabel = this["EndTimeLabel"];
        TimeFormatText = this["TimeFormatText"];
        StartFrameLabel = this["StartFrameLabel"];
        ClipFrameCountLabel = this["ClipFrameCountLabel"];
        EndFrameLabel = this["EndFrameLabel"];
        FrameFormatText = this["FrameFormatText"];
        Note2Text = this["Note2Text"];
        CancelButtonText = this["CancelButtonText"];
        ConfirmButtonText = this["ConfirmButtonText"];
        SummaryDurationLabel = this["SummaryDurationLabel"];
        SummaryTotalFramesLabel = this["SummaryTotalFramesLabel"];
        SummaryFrameRateLabel = this["SummaryFrameRateLabel"];
        SummarySecondsUnit = this["SummarySecondsUnit"];
        SummaryProgressive = this["SummaryProgressive"];
        SummaryInterlaced = this["SummaryInterlaced"];
        SummaryUnknown = this["SummaryUnknown"];
        SummaryConstantFrameRate = this["SummaryConstantFrameRate"];
        SummaryVariableFrameRate = this["SummaryVariableFrameRate"];
        SummaryFrameRateUnknown = this["SummaryFrameRateUnknown"];
    }
}
