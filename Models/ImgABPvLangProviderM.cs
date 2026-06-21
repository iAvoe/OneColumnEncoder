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
            ["EncoderLabel"] = "\u7f16\u7801\u5668",
            ["DisplayModeLabel"] = "\u663e\u793a",
            ["ZoomLabel"] = "\u7f29\u653e",
            ["PositionLabel"] = "\u753b\u9762\u4f4d\u7f6e",
            ["FitButtonText"] = "\u9002\u5e94",
            ["RawButtonText"] = "\u539f\u59cb",
            ["Hint1Text"] = "\u62d6\u62fd\u5206\u5272\u7ebf\u6765\u6bd4\u8f83\u6e90\u5e27\u4e0e\u7f16\u7801\u5e27\u3002",
            ["Hint2Text"] = "\u9884\u89c8\u4ec5\u4f7f\u7528 ffmpeg\uff0c\u53ef\u7528\u7f16\u7801\u9009\u9879\u53ef\u80fd\u4e0d\u540c\u4e8e\u5bfc\u5165\u7684\u7f16\u7801\u5668\u3002",
            ["Hint3Text"] = "\u538b\u7f29\u4ec5\u5728\u70b9\u51fb\u9884\u89c8\u540e\u8fd0\u884c\u3002",
            ["PreviewButtonText"] = "\u9884\u89c8",
            ["CancelButtonText"] = "\u53d6\u6d88",
            ["StatusReady"] = "\u5c31\u7eea\u3002",
            ["StatusExtracting"] = "\u6b63\u5728\u63d0\u53d6\u6e90\u5e27...",
            ["StatusConverting"] = "\u6b63\u5728\u8f6c\u6362\u6e90\u5e27\uff08{0}\uff09...",
            ["StatusEncoding"] = "\u6b63\u5728\u7528 {0} \u7f16\u7801...",
            ["StatusDecoding"] = "\u6b63\u5728\u89e3\u7801\u9884\u89c8\u5e27...",
            ["StatusPreviewReady"] = "\u9884\u89c8\u5c31\u7eea\uff1a{0}\uff0cCRF {1}\u3002",
            ["StatusCancelled"] = "\u9884\u89c8\u5df2\u53d6\u6d88\u3002",
            ["StatusNoFfmpeg"] = "\u672a\u5bfc\u5165 ffmpeg.exe\u3002",
            ["StatusNoSource"] = "\u672a\u9009\u62e9\u6709\u6548\u89c6\u9891\u6e90\u3002",
            ["StatusDisplayModeBlocked"] = "\u9884\u89c8\u8fd0\u884c\u65f6\u65e0\u6cd5\u66f4\u6539\u663e\u793a\u6a21\u5f0f\u3002",
            ["StatusDisplayModeSet"] = "\u663e\u793a\u6a21\u5f0f\uff1a{0}\u3002",
            ["DisplayModeRaw"] = "\u539f\u59cb",
            ["DisplayModeLowToBt709"] = "\u4f4e\u8272\u57df\u8f6c BT.709",
            ["DisplayModeWcgToBt709"] = "WCG \u8f6c BT.709",
            ["DisplayModeHdrToSdr"] = "HDR \u8f6c SDR",
            ["DisplayModeHighHdrToSdr"] = "\u9ad8 HDR \u8f6c SDR",
        },
        ["zh-tw"] = new()
        {
            ["EncoderLabel"] = "\u7de8\u78bc\u5668",
            ["DisplayModeLabel"] = "\u986f\u793a",
            ["ZoomLabel"] = "\u7e2e\u653e",
            ["PositionLabel"] = "\u753b\u9762\u4f4d\u7f6e",
            ["FitButtonText"] = "\u9069\u61c9",
            ["RawButtonText"] = "\u539f\u59cb",
            ["Hint1Text"] = "\u62d6\u62fd\u5206\u5272\u7dda\u4f86\u6bd4\u8f03\u6e90\u5e40\u8207\u7de8\u78bc\u5e40\u3002",
            ["Hint2Text"] = "\u9810\u89bd\u50c5\u4f7f\u7528 ffmpeg\uff0c\u53ef\u7528\u7de8\u78bc\u9078\u9805\u53ef\u80fd\u4e0d\u540c\u65bc\u5c0e\u5165\u7684\u7de8\u78bc\u5668\u3002",
            ["Hint3Text"] = "\u58d3\u7e2e\u50c5\u5728\u9ede\u64ca\u9810\u89bd\u5f8c\u904b\u884c\u3002",
            ["PreviewButtonText"] = "\u9810\u89bd",
            ["CancelButtonText"] = "\u53d6\u6d88",
            ["StatusReady"] = "\u5c31\u7e6c\u3002",
            ["StatusExtracting"] = "\u6b63\u5728\u63d0\u53d6\u6e90\u5e40...",
            ["StatusConverting"] = "\u6b63\u5728\u8f49\u63db\u6e90\u5e40\uff08{0}\uff09...",
            ["StatusEncoding"] = "\u6b63\u5728\u7528 {0} \u7de8\u78bc...",
            ["StatusDecoding"] = "\u6b63\u5728\u89e3\u78bc\u9810\u89bd\u5e40...",
            ["StatusPreviewReady"] = "\u9810\u89bd\u5c31\u7e6c\uff1a{0}\uff0cCRF {1}\u3002",
            ["StatusCancelled"] = "\u9810\u89bd\u5df2\u53d6\u6d88\u3002",
            ["StatusNoFfmpeg"] = "\u672a\u5c0e\u5165 ffmpeg.exe\u3002",
            ["StatusNoSource"] = "\u672a\u9078\u64c7\u6709\u6548\u8996\u8a0a\u6e90\u3002",
            ["StatusDisplayModeBlocked"] = "\u9810\u89bd\u904b\u884c\u6642\u7121\u6cd5\u8b8a\u66f4\u986f\u793a\u6a21\u5f0f\u3002",
            ["StatusDisplayModeSet"] = "\u986f\u793a\u6a21\u5f0f\uff1a{0}\u3002",
            ["DisplayModeRaw"] = "\u539f\u59cb",
            ["DisplayModeLowToBt709"] = "\u4f4e\u8272\u57df\u8f49 BT.709",
            ["DisplayModeWcgToBt709"] = "WCG \u8f49 BT.709",
            ["DisplayModeHdrToSdr"] = "HDR \u8f49 SDR",
            ["DisplayModeHighHdrToSdr"] = "\u9ad8 HDR \u8f49 SDR",
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
