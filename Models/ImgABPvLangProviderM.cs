namespace OneColumnEncoder.Models;

public class ImgABPvLangProviderM
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["EncoderLabel"] = "Encoder",
            ["DisplayModeLabel"] = "Display",
            ["ZoomLabel"] = "Zoom",
            ["PositionLabel"] = "Image Position",
            ["FitButtonText"] = "Fit",
            ["RawButtonText"] = "Raw",
            ["Hint1Text"] = "Drag the split line to compare source and encoded frame.",
            ["Hint2Text"] = "Preview uses ffmpeg only; available encoder options may differ from imported encoders.",
            ["Hint3Text"] = "Compression runs only after Preview is clicked.",
            ["PreviewButtonText"] = "Preview",
            ["CancelButtonText"] = "Cancel",
            ["StatusReady"] = "Ready.",
            ["StatusExtracting"] = "Extracting source frame...",
            ["StatusConverting"] = "Converting source frame ({0})...",
            ["StatusEncoding"] = "Encoding with {0}...",
            ["StatusDecoding"] = "Decoding preview frame...",
            ["StatusPreviewReady"] = "Preview ready: {0}, CRF {1}.",
            ["StatusCancelled"] = "Preview cancelled.",
            ["StatusNoFfmpeg"] = "ffmpeg.exe is not imported.",
            ["StatusNoSource"] = "No valid video source selected.",
            ["StatusDisplayModeBlocked"] = "Display mode cannot be changed while preview is running.",
            ["StatusDisplayModeSet"] = "Display mode: {0}.",
            ["DisplayModeRaw"] = "Raw",
            ["DisplayModeLowToBt709"] = "Low gamut to BT.709",
            ["DisplayModeWcgToBt709"] = "WCG to BT.709",
            ["DisplayModeHdrToSdr"] = "HDR to SDR",
            ["DisplayModeHighHdrToSdr"] = "High HDR to SDR",
        },
        ["zh-cn"] = new()
        {
            ["EncoderLabel"] = "编码器",
            ["DisplayModeLabel"] = "显示",
            ["ZoomLabel"] = "缩放",
            ["PositionLabel"] = "画面位置",
            ["FitButtonText"] = "适应",
            ["RawButtonText"] = "原始",
            ["Hint1Text"] = "拖拽分割线来比较源帧与编码帧。",
            ["Hint2Text"] = "预览仅使用 ffmpeg，可用编码选项可能不同于导入的编码器。",
            ["Hint3Text"] = "压缩仅在点击预览后运行。",
            ["PreviewButtonText"] = "预览",
            ["CancelButtonText"] = "取消",
            ["StatusReady"] = "就绪。",
            ["StatusExtracting"] = "正在提取源帧...",
            ["StatusConverting"] = "正在转换源帧（{0}）...",
            ["StatusEncoding"] = "正在用 {0} 编码...",
            ["StatusDecoding"] = "正在解码预览帧...",
            ["StatusPreviewReady"] = "预览就绪：{0}，CRF {1}。",
            ["StatusCancelled"] = "预览已取消。",
            ["StatusNoFfmpeg"] = "未导入 ffmpeg.exe。",
            ["StatusNoSource"] = "未选择有效视频源。",
            ["StatusDisplayModeBlocked"] = "预览运行时无法更改显示模式。",
            ["StatusDisplayModeSet"] = "显示模式：{0}。",
            ["DisplayModeRaw"] = "原始",
            ["DisplayModeLowToBt709"] = "低色域转 BT.709",
            ["DisplayModeWcgToBt709"] = "WCG 转 BT.709",
            ["DisplayModeHdrToSdr"] = "HDR 转 SDR",
            ["DisplayModeHighHdrToSdr"] = "高 HDR 转 SDR",
        },
        ["zh-tw"] = new()
        {
            ["EncoderLabel"] = "編碼器",
            ["DisplayModeLabel"] = "顯示",
            ["ZoomLabel"] = "縮放",
            ["PositionLabel"] = "画面位置",
            ["FitButtonText"] = "適應",
            ["RawButtonText"] = "原始",
            ["Hint1Text"] = "拖拽分割線來比較源幀與編碼幀。",
            ["Hint2Text"] = "預覽僅使用 ffmpeg，可用編碼選項可能不同於導入的編碼器。",
            ["Hint3Text"] = "壓縮僅在點擊預覽後運行。",
            ["PreviewButtonText"] = "預覽",
            ["CancelButtonText"] = "取消",
            ["StatusReady"] = "就繬。",
            ["StatusExtracting"] = "正在提取源幀...",
            ["StatusConverting"] = "正在轉換源幀（{0}）...",
            ["StatusEncoding"] = "正在用 {0} 編碼...",
            ["StatusDecoding"] = "正在解碼預覽幀...",
            ["StatusPreviewReady"] = "預覽就繬：{0}，CRF {1}。",
            ["StatusCancelled"] = "預覽已取消。",
            ["StatusNoFfmpeg"] = "未導入 ffmpeg.exe。",
            ["StatusNoSource"] = "未選擇有效視訊源。",
            ["StatusDisplayModeBlocked"] = "預覽運行時無法變更顯示模式。",
            ["StatusDisplayModeSet"] = "顯示模式：{0}。",
            ["DisplayModeRaw"] = "原始",
            ["DisplayModeLowToBt709"] = "低色域轉 BT.709",
            ["DisplayModeWcgToBt709"] = "WCG 轉 BT.709",
            ["DisplayModeHdrToSdr"] = "HDR 轉 SDR",
            ["DisplayModeHighHdrToSdr"] = "高 HDR 轉 SDR",
        },
    };

    static ImgABPvLangProviderM()
    {
        Data["fr"] = new(Data["en"])
        {
            ["EncoderLabel"] = "Encodeur",
            ["DisplayModeLabel"] = "Affichage",
            ["ZoomLabel"] = "Zoom",
            ["PositionLabel"] = "Position image",
            ["FitButtonText"] = "Ajuster",
            ["RawButtonText"] = "Brut",
            ["Hint1Text"] = "Faites glisser la ligne de séparation pour comparer source et encodé.",
            ["Hint2Text"] = "L'aperçu utilise uniquement ffmpeg ; les options disponibles peuvent différer des encodeurs importés.",
            ["Hint3Text"] = "La compression ne s'exécute qu'après avoir cliqué sur Aperçu.",
            ["PreviewButtonText"] = "Aperçu",
            ["CancelButtonText"] = "Annuler",
            ["StatusReady"] = "Prêt.",
            ["StatusExtracting"] = "Extraction de l'image source...",
            ["StatusConverting"] = "Conversion de l'image source ({0})...",
            ["StatusEncoding"] = "Encodage avec {0}...",
            ["StatusDecoding"] = "Décodage de l'aperçu...",
            ["StatusPreviewReady"] = "Aperçu prêt : {0}, CRF {1}.",
            ["StatusCancelled"] = "Aperçu annulé.",
            ["StatusNoFfmpeg"] = "ffmpeg.exe n'est pas importé.",
            ["StatusNoSource"] = "Aucune source vidéo valide sélectionnée.",
            ["StatusDisplayModeBlocked"] = "Le mode d'affichage ne peut pas être changé pendant l'aperçu.",
            ["StatusDisplayModeSet"] = "Mode d'affichage : {0}.",
            ["DisplayModeRaw"] = "Brut",
            ["DisplayModeLowToBt709"] = "Bas gamut → BT.709",
            ["DisplayModeWcgToBt709"] = "WCG → BT.709",
            ["DisplayModeHdrToSdr"] = "HDR → SDR",
            ["DisplayModeHighHdrToSdr"] = "HDR élevé → SDR",
        };
        Data["es"] = new(Data["en"])
        {
            ["EncoderLabel"] = "Codificador",
            ["DisplayModeLabel"] = "Pantalla",
            ["ZoomLabel"] = "Zoom",
            ["PositionLabel"] = "Posición imagen",
            ["FitButtonText"] = "Ajustar",
            ["RawButtonText"] = "Crudo",
            ["Hint1Text"] = "Arrastre la línea divisoria para comparar fuente y codificado.",
            ["Hint2Text"] = "La vista previa usa solo ffmpeg; las opciones pueden diferir de los codificadores importados.",
            ["Hint3Text"] = "La compresión solo se ejecuta tras hacer clic en Vista previa.",
            ["PreviewButtonText"] = "Vista previa",
            ["CancelButtonText"] = "Cancelar",
            ["StatusReady"] = "Listo.",
            ["StatusExtracting"] = "Extrayendo fotograma fuente...",
            ["StatusConverting"] = "Convirtiendo fotograma fuente ({0})...",
            ["StatusEncoding"] = "Codificando con {0}...",
            ["StatusDecoding"] = "Decodificando vista previa...",
            ["StatusPreviewReady"] = "Vista previa lista: {0}, CRF {1}.",
            ["StatusCancelled"] = "Vista previa cancelada.",
            ["StatusNoFfmpeg"] = "ffmpeg.exe no está importado.",
            ["StatusNoSource"] = "No hay fuente de video válida seleccionada.",
            ["StatusDisplayModeBlocked"] = "No se puede cambiar el modo de visualización durante la vista previa.",
            ["StatusDisplayModeSet"] = "Modo de visualización: {0}.",
            ["DisplayModeRaw"] = "Crudo",
            ["DisplayModeLowToBt709"] = "Gamut bajo → BT.709",
            ["DisplayModeWcgToBt709"] = "WCG → BT.709",
            ["DisplayModeHdrToSdr"] = "HDR → SDR",
            ["DisplayModeHighHdrToSdr"] = "HDR alto → SDR",
        };
        Data["ja"] = new(Data["en"])
        {
            ["EncoderLabel"] = "エンコーダ",
            ["DisplayModeLabel"] = "表示",
            ["ZoomLabel"] = "ズーム",
            ["PositionLabel"] = "画像位置",
            ["FitButtonText"] = "フィット",
            ["RawButtonText"] = "生",
            ["Hint1Text"] = "分割線をドラッグしてソースとエンコード済みを比較します。",
            ["Hint2Text"] = "プレビューは ffmpeg のみを使用。利用可能なエンコーダオプションはインポート済みと異なる場合があります。",
            ["Hint3Text"] = "圧縮はプレビューをクリックした後にのみ実行されます。",
            ["PreviewButtonText"] = "プレビュー",
            ["CancelButtonText"] = "キャンセル",
            ["StatusReady"] = "準備完了。",
            ["StatusExtracting"] = "ソースフレームを抽出中...",
            ["StatusConverting"] = "ソースフレームを変換中（{0}）...",
            ["StatusEncoding"] = "{0} でエンコード中...",
            ["StatusDecoding"] = "プレビューフレームをデコード中...",
            ["StatusPreviewReady"] = "プレビュー準備完了：{0}、CRF {1}。",
            ["StatusCancelled"] = "プレビューがキャンセルされました。",
            ["StatusNoFfmpeg"] = "ffmpeg.exe がインポートされていません。",
            ["StatusNoSource"] = "有効な動画ソースが選択されていません。",
            ["StatusDisplayModeBlocked"] = "プレビュー実行中は表示モードを変更できません。",
            ["StatusDisplayModeSet"] = "表示モード：{0}。",
            ["DisplayModeRaw"] = "生",
            ["DisplayModeLowToBt709"] = "低色域→BT.709",
            ["DisplayModeWcgToBt709"] = "WCG→BT.709",
            ["DisplayModeHdrToSdr"] = "HDR→SDR",
            ["DisplayModeHighHdrToSdr"] = "高HDR→SDR",
        };
        Data["ru"] = new(Data["en"])
        {
            ["EncoderLabel"] = "Кодек",
            ["DisplayModeLabel"] = "Экран",
            ["ZoomLabel"] = "Масштаб",
            ["PositionLabel"] = "Положение",
            ["FitButtonText"] = "По размеру",
            ["RawButtonText"] = "Сырой",
            ["Hint1Text"] = "Перетащите линию разделения для сравнения исходного и закодированного.",
            ["Hint2Text"] = "Предпросмотр использует только ffmpeg; доступные опции могут отличаться от импортированных кодеров.",
            ["Hint3Text"] = "Сжатие запускается только после нажатия Предпросмотр.",
            ["PreviewButtonText"] = "Предпросмотр",
            ["CancelButtonText"] = "Отмена",
            ["StatusReady"] = "Готово.",
            ["StatusExtracting"] = "Извлечение исходного кадра...",
            ["StatusConverting"] = "Преобразование исходного кадра ({0})...",
            ["StatusEncoding"] = "Кодирование с {0}...",
            ["StatusDecoding"] = "Декодирование кадра предпросмотра...",
            ["StatusPreviewReady"] = "Предпросмотр готов: {0}, CRF {1}.",
            ["StatusCancelled"] = "Предпросмотр отменён.",
            ["StatusNoFfmpeg"] = "ffmpeg.exe не импортирован.",
            ["StatusNoSource"] = "Не выбран действительный источник видео.",
            ["StatusDisplayModeBlocked"] = "Нельзя сменить режим отображения во время предпросмотра.",
            ["StatusDisplayModeSet"] = "Режим отображения: {0}.",
            ["DisplayModeRaw"] = "Сырой",
            ["DisplayModeLowToBt709"] = "Низкий→BT.709",
            ["DisplayModeWcgToBt709"] = "WCG→BT.709",
            ["DisplayModeHdrToSdr"] = "HDR→SDR",
            ["DisplayModeHighHdrToSdr"] = "Высокий HDR→SDR",
        };
    }

    public string LanguageCode { get; }
    private readonly Dictionary<string, string> _d;
    public string this[string key] => _d.TryGetValue(key, out var v) ? v : key;

    public string EncoderLabel { get; }
    public string DisplayModeLabel { get; }
    public string ZoomLabel { get; }
    public string PositionLabel { get; }
    public string FitButtonText { get; }
    public string RawButtonText { get; }
    public string Hint1Text { get; }
    public string Hint2Text { get; }
    public string Hint3Text { get; }
    public string PreviewButtonText { get; }
    public string CancelButtonText { get; }
    public string StatusReady { get; }
    public string StatusExtracting { get; }
    public string StatusConverting { get; }
    public string StatusEncoding { get; }
    public string StatusDecoding { get; }
    public string StatusPreviewReady { get; }
    public string StatusCancelled { get; }
    public string StatusNoFfmpeg { get; }
    public string StatusNoSource { get; }
    public string StatusDisplayModeBlocked { get; }
    public string StatusDisplayModeSet { get; }
    public string DisplayModeRaw { get; }
    public string DisplayModeLowToBt709 { get; }
    public string DisplayModeWcgToBt709 { get; }
    public string DisplayModeHdrToSdr { get; }
    public string DisplayModeHighHdrToSdr { get; }

    public ImgABPvLangProviderM(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];
        EncoderLabel = _d["EncoderLabel"];
        DisplayModeLabel = _d["DisplayModeLabel"];
        ZoomLabel = _d["ZoomLabel"];
        PositionLabel = _d["PositionLabel"];
        FitButtonText = _d["FitButtonText"];
        RawButtonText = _d["RawButtonText"];
        Hint1Text = _d["Hint1Text"];
        Hint2Text = _d["Hint2Text"];
        Hint3Text = _d["Hint3Text"];
        PreviewButtonText = _d["PreviewButtonText"];
        CancelButtonText = _d["CancelButtonText"];
        StatusReady = _d["StatusReady"];
        StatusExtracting = _d["StatusExtracting"];
        StatusConverting = _d["StatusConverting"];
        StatusEncoding = _d["StatusEncoding"];
        StatusDecoding = _d["StatusDecoding"];
        StatusPreviewReady = _d["StatusPreviewReady"];
        StatusCancelled = _d["StatusCancelled"];
        StatusNoFfmpeg = _d["StatusNoFfmpeg"];
        StatusNoSource = _d["StatusNoSource"];
        StatusDisplayModeBlocked = _d["StatusDisplayModeBlocked"];
        StatusDisplayModeSet = _d["StatusDisplayModeSet"];
        DisplayModeRaw = _d["DisplayModeRaw"];
        DisplayModeLowToBt709 = _d["DisplayModeLowToBt709"];
        DisplayModeWcgToBt709 = _d["DisplayModeWcgToBt709"];
        DisplayModeHdrToSdr = _d["DisplayModeHdrToSdr"];
        DisplayModeHighHdrToSdr = _d["DisplayModeHighHdrToSdr"];
    }
}
