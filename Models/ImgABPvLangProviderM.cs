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
            ["Hint1Text"] = "Compression is only performed after clicking Preview due to the slowness of some encoders",
            ["Hint2Text"] = "Preview is only via ffmpeg to ensure usability when no encoder is imported",
            ["Hint3Text"] = "Drag the separator line to compare; beware that you are comparing \u201Cpaused\u201D, not \u201Cmotion\u201D picture quality",
            ["PreviewButtonText"] = "Preview",
            ["CancelButtonText"] = "Cancel",
            ["StatusReady"] = "Ready",
            ["StatusExtracting"] = "Extracting source frame...",
            ["StatusConverting"] = "Converting source frame ({0})...",
            ["StatusEncoding"] = "Encoding with {0}...",
            ["StatusDecoding"] = "Decoding preview frame...",
            ["StatusPreviewReady"] = "Preview ready: {0}, CRF {1}",
            ["StatusComputingScores"] = "Computing quality metrics...",
            ["StatusCancelled"] = "Preview cancelled",
            ["StatusNoFfmpeg"] = "ffmpeg.exe is not imported",
            ["StatusNoSource"] = "No valid video source selected",
            ["StatusDisplayModeBlocked"] = "Display mode cannot be changed while preview is running",
            ["StatusDisplayModeSet"] = "Display mode: {0}",
            ["DisplayModeRaw"] = "Raw",
            ["DisplayModeLowToBt709"] = "Low gamut to BT.709",
            ["DisplayModeWcgToBt709"] = "WCG to BT.709",
            ["DisplayModeHdrToSdr"] = "HDR to SDR",
            ["DisplayModeHighHdrToSdr"] = "High HDR to SDR",
            ["WarnSvtAv1No12Bit"] = "libsvtav1 does not support 12-bit source preview.\nPlease use libx265 or select a different source",
            ["Ssimulacra2ToolMissing"] = "SSIMULACRA2.1 tool not found. Place x64-CloudinarySSIMULACRA2.1 next to the executable and restart",
            ["Ssimulacra2ToolPresent"] = "SSIMULACRA2.1 quality metric is ready",
            ["SsimulacraScoreHint"] = "SSIMULACRA score: 100 Math Lossless | 90 VQA Lossless | 85 Marginal Loss | 80 Revealing Loss | 70 Tangible loss | 50 Substantial loss | 30 Breaking loss",
            ["ButteraugliToolMissing"] = "Butteraugli tool not found. Place x64-GoogleButteraugli next to the executable and restart",
            ["ButteraugliToolPresent"] = "Butteraugli quality metric is ready",
            ["ButteraugliScoreHint"] = "Butteraugli score (heuristic): <1 Theoretically lossless | 1-2 Visually lossless | 2-4 Slight loss | 4-6 Noticeable loss | 6-8 Heavy loss | >8 Severe loss",
        },
        ["zh-cn"] = new()
        {
            ["EncoderLabel"] = "编码器",
            ["DisplayModeLabel"] = "显示",
            ["ZoomLabel"] = "缩放",
            ["PositionLabel"] = "画面位置",
            ["FitButtonText"] = "适应",
            ["RawButtonText"] = "原始",
            ["Hint1Text"] = "由于有的编码器较慢，因此压缩操作仅在点击预览后运行",
            ["Hint2Text"] = "预览仅用 ffmpeg 以确保在未导入编码器时仍可预览",
            ["Hint3Text"] = "拖拽分割线来比较源帧与编码帧；注意你在对比的是「暂停画质」而非「动态画质」",
            ["PreviewButtonText"] = "预览",
            ["CancelButtonText"] = "取消",
            ["StatusReady"] = "就绪",
            ["StatusExtracting"] = "正在提取源帧...",
            ["StatusConverting"] = "正在转换源帧（{0}）...",
            ["StatusEncoding"] = "正在用 {0} 编码...",
            ["StatusDecoding"] = "正在解码预览帧...",
            ["StatusPreviewReady"] = "预览就绪：{0}，CRF {1}",
            ["StatusComputingScores"] = "正在计算画质跑分...",
            ["StatusCancelled"] = "预览已取消",
            ["StatusNoFfmpeg"] = "未导入 ffmpeg.exe",
            ["StatusNoSource"] = "未选择有效视频源",
            ["StatusDisplayModeBlocked"] = "预览运行时无法更改显示模式",
            ["StatusDisplayModeSet"] = "显示模式：{0}",
            ["DisplayModeRaw"] = "原始",
            ["DisplayModeLowToBt709"] = "低色域转 BT.709",
            ["DisplayModeWcgToBt709"] = "WCG 转 BT.709",
            ["DisplayModeHdrToSdr"] = "HDR 转 SDR",
            ["DisplayModeHighHdrToSdr"] = "高 HDR 转 SDR",
            ["WarnSvtAv1No12Bit"] = "libsvtav1 不支持 12bit 源预览。\n请改用 libx265 或更换视频源",
            ["Ssimulacra2ToolMissing"] = "SSIMULACRA2.1 质量指标工具缺失。请将 x64-CloudinarySSIMULACRA2.1 文件夹放在程序目录旁，再重启应用",
            ["Ssimulacra2ToolPresent"] = "SSIMULACRA2.1 质量指标工具已就绪",
            ["SsimulacraScoreHint"] = "SSIMULACRA 分数：100 数据无损 | 90 视觉无损 | 85 差异极小 | 80 差异显露 | 70 差异可见 | 50 差异较大 | 30 差异巨大",
            ["ButteraugliToolMissing"] = "Butteraugli 质量指标工具缺失。请将 x64-GoogleButteraugli 文件夹放在程序目录旁，再重启应用",
            ["ButteraugliToolPresent"] = "Butteraugli 质量指标工具已就绪",
            ["ButteraugliScoreHint"] = "Butteraugli 分数（经验分级）：<1 理论无损 | 1-2 视觉无损 | 2-4 轻微可见损失 | 4-6 明显损失 | 6-8 严重损失 | >8 极重损失",
        },
        ["zh-tw"] = new()
        {
            ["EncoderLabel"] = "編碼器",
            ["DisplayModeLabel"] = "顯示",
            ["ZoomLabel"] = "縮放",
            ["PositionLabel"] = "画面位置",
            ["FitButtonText"] = "適應",
            ["RawButtonText"] = "原始",
            ["Hint1Text"] = "由於有的編碼器較慢，因此壓縮操作僅在點擊預覽後運行",
            ["Hint2Text"] = "預覽僅用 ffmpeg 以確保在未導入編碼器時仍可預覽",
            ["Hint3Text"] = "拖拽分割線來比較源幀與編碼幀；注意你在對比的是「暫停畫質」而非「動態畫質」",
            ["PreviewButtonText"] = "預覽",
            ["CancelButtonText"] = "取消",
            ["StatusReady"] = "就繬",
            ["StatusExtracting"] = "正在提取源幀...",
            ["StatusConverting"] = "正在轉換源幀（{0}）...",
            ["StatusEncoding"] = "正在用 {0} 編碼...",
            ["StatusDecoding"] = "正在解碼預覽幀...",
            ["StatusPreviewReady"] = "預覽就繬：{0}，CRF {1}",
            ["StatusComputingScores"] = "正在計算畫質跑分...",
            ["StatusCancelled"] = "預覽已取消",
            ["StatusNoFfmpeg"] = "未導入 ffmpeg.exe",
            ["StatusNoSource"] = "未選擇有效視訊源",
            ["StatusDisplayModeBlocked"] = "預覽運行時無法變更顯示模式",
            ["StatusDisplayModeSet"] = "顯示模式：{0}",
            ["DisplayModeRaw"] = "原始",
            ["DisplayModeLowToBt709"] = "低色域轉 BT.709",
            ["DisplayModeWcgToBt709"] = "WCG 轉 BT.709",
            ["DisplayModeHdrToSdr"] = "HDR 轉 SDR",
            ["DisplayModeHighHdrToSdr"] = "高 HDR 轉 SDR",
            ["WarnSvtAv1No12Bit"] = "libsvtav1 不支援 12bit 源預覽。\n请改用 libx265 或更換視訊源",
            ["Ssimulacra2ToolMissing"] = "SSIMULACRA2.1 質量指標工具缺失。請將 x64-CloudinarySSIMULACRA2.1 文件夾放在程序目錄旁，再重啟應用",
            ["Ssimulacra2ToolPresent"] = "SSIMULACRA2.1 質量指標工具已就緒",
            ["SsimulacraScoreHint"] = "SSIMULACRA 分數：100 數據無損 | 90 視覺無損 | 85 差異極小 | 80 差異顯露 | 70 差異可見 | 50 差異較大 | 30 差異巨大",
            ["ButteraugliToolMissing"] = "Butteraugli 質量指標工具缺失。請將 x64-GoogleButteraugli 文件夾放在程序目錄旁，再重啟應用",
            ["ButteraugliToolPresent"] = "Butteraugli 質量指標工具已就緒",
            ["ButteraugliScoreHint"] = "Butteraugli 分數（經驗分級）：<1 理論無損 | 1-2 視覺無損 | 2-4 輕微可見損失 | 4-6 明顯損失 | 6-8 嚴重損失 | >8 極重損失",
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
            ["Hint1Text"] = "La compression n'est effectuée qu'après avoir cliqué sur « Aperçu », en raison de la lenteur de certains encodeurs",
            ["Hint2Text"] = "L'aperçu est uniquement réalisé via ffmpeg afin de garantir la compatibilité même sans encodeur importé",
            ["Hint3Text"] = "Faites glisser la ligne de séparation pour comparer ; attention, vous comparez la qualité d'image à l'arrêt, et non en mouvement",
            ["PreviewButtonText"] = "Aperçu",
            ["CancelButtonText"] = "Annuler",
            ["StatusReady"] = "Prêt",
            ["StatusExtracting"] = "Extraction de l'image source...",
            ["StatusConverting"] = "Conversion de l'image source ({0})...",
            ["StatusEncoding"] = "Encodage avec {0}...",
            ["StatusDecoding"] = "Décodage de l'aperçu...",
            ["StatusPreviewReady"] = "Aperçu prêt : {0}, CRF {1}",
            ["StatusComputingScores"] = "Calcul des métriques de qualité...",
            ["StatusCancelled"] = "Aperçu annulé",
            ["StatusNoFfmpeg"] = "ffmpeg.exe n'est pas importé",
            ["StatusNoSource"] = "Aucune source vidéo valide sélectionnée",
            ["StatusDisplayModeBlocked"] = "Le mode d'affichage ne peut pas être changé pendant l'aperçu",
            ["StatusDisplayModeSet"] = "Mode d'affichage : {0}",
            ["DisplayModeRaw"] = "Brut",
            ["DisplayModeLowToBt709"] = "Bas gamut → BT.709",
            ["DisplayModeWcgToBt709"] = "WCG → BT.709",
            ["DisplayModeHdrToSdr"] = "HDR → SDR",
            ["DisplayModeHighHdrToSdr"] = "HDR élevé → SDR",
            ["WarnSvtAv1No12Bit"] = "libsvtav1 ne prend pas en charge la prévisualisation des sources 12 bits.\nVeuillez utiliser libx265 ou sélectionner une autre source.",
            ["Ssimulacra2ToolMissing"] = "L'outil SSIMULACRA2.1 est introuvable. Placez le fichier x64-CloudinarySSIMULACRA2.1 à côté de l'exécutable et redémarrez.",
            ["Ssimulacra2ToolPresent"] = "La métrique de qualité SSIMULACRA2.1 est prête",
            ["SsimulacraScoreHint"] = "Score SSIMULACRA : 100 Sans perte | 90 Sans perte visuelle | 85 Perte minime | 80 Légère | 70 Visible | 50 Notable | 30 Grave",
            ["ButteraugliToolMissing"] = "L'outil Butteraugli est introuvable. Placez x64-GoogleButteraugli à côté de l'exécutable et redémarrez.",
            ["ButteraugliToolPresent"] = "La métrique de qualité Butteraugli est prête",
            ["ButteraugliScoreHint"] = "Score Butteraugli (heuristique) : <1 Théoriquement sans perte | 1-2 Visuellement sans perte | 2-4 Perte légère | 4-6 Perte notable | 6-8 Perte importante | >8 Perte sévère",
        };
        Data["es"] = new(Data["en"])
        {
            ["EncoderLabel"] = "Codificador",
            ["DisplayModeLabel"] = "Pantalla",
            ["ZoomLabel"] = "Zoom",
            ["PositionLabel"] = "Posición imagen",
            ["FitButtonText"] = "Ajustar",
            ["RawButtonText"] = "Crudo",
            ["Hint1Text"] = "La compresión solo se realiza después de hacer clic en Vista previa debido a la lentitud de algunos codificadores",
            ["Hint2Text"] = "La vista previa solo se realiza mediante ffmpeg para garantizar su usabilidad cuando no se importa ningún codificador",
            ["Hint3Text"] = "Arrastre la línea separadora para comparar; tenga en cuenta que está comparando la calidad de la imagen en pausa, no en movimiento",
            ["PreviewButtonText"] = "Vista previa",
            ["CancelButtonText"] = "Cancelar",
            ["StatusReady"] = "Listo",
            ["StatusExtracting"] = "Extrayendo fotograma fuente...",
            ["StatusConverting"] = "Convirtiendo fotograma fuente ({0})...",
            ["StatusEncoding"] = "Codificando con {0}...",
            ["StatusDecoding"] = "Decodificando vista previa...",
            ["StatusPreviewReady"] = "Vista previa lista: {0}, CRF {1}",
            ["StatusComputingScores"] = "Calculando métricas de calidad...",
            ["StatusCancelled"] = "Vista previa cancelada",
            ["StatusNoFfmpeg"] = "ffmpeg.exe no está importado",
            ["StatusNoSource"] = "No hay fuente de video válida seleccionada",
            ["StatusDisplayModeBlocked"] = "No se puede cambiar el modo de visualización durante la vista previa",
            ["StatusDisplayModeSet"] = "Modo de visualización: {0}",
            ["DisplayModeRaw"] = "Crudo",
            ["DisplayModeLowToBt709"] = "Gamut bajo → BT.709",
            ["DisplayModeWcgToBt709"] = "WCG → BT.709",
            ["DisplayModeHdrToSdr"] = "HDR → SDR",
            ["DisplayModeHighHdrToSdr"] = "HDR alto → SDR",
            ["WarnSvtAv1No12Bit"] = "libsvtav1 no es compatible con la vista previa de fuentes de 12 bits.\nUtilice libx265 o seleccione una fuente diferente.",
            ["Ssimulacra2ToolMissing"] = "No se encontró la herramienta SSIMULACRA2.1. Coloque x64-CloudinarySSIMULACRA2.1 junto al ejecutable y reinicie.",
            ["Ssimulacra2ToolPresent"] = "La métrica de calidad SSIMULACRA2.1 está lista.",
            ["SsimulacraScoreHint"] = "SSIMULACRA: 100 Sin pérdida | 90 Sin pérdida visual | 85 Pérdida mínima | 80 Notable | 70 Visible | 50 Considerable | 30 Grave",
            ["ButteraugliToolMissing"] = "No se encontró la herramienta Butteraugli. Coloque x64-GoogleButteraugli junto al ejecutable y reinicie.",
            ["ButteraugliToolPresent"] = "La métrica de calidad Butteraugli está lista.",
            ["ButteraugliScoreHint"] = "Puntuación Butteraugli (heurística): <1 Sin pérdida teórica | 1-2 Sin pérdida visual | 2-4 Pérdida leve | 4-6 Pérdida notable | 6-8 Pérdida grave | >8 Pérdida severa",
        };
        Data["ja"] = new(Data["en"])
        {
            ["EncoderLabel"] = "エンコーダ",
            ["DisplayModeLabel"] = "表示",
            ["ZoomLabel"] = "ズーム",
            ["PositionLabel"] = "画像位置",
            ["FitButtonText"] = "フィット",
            ["RawButtonText"] = "生",
            ["Hint1Text"] = "一部のエンコーダーの処理速度が遅いため、圧縮はプレビューをクリックした後にのみ実行されます",
            ["Hint2Text"] = "プレビューは、エンコーダーがインポートされていない場合でもプレビューが可能であることを確認するために、ffmpegでのみ使用されます",
            ["Hint3Text"] = "区切り線をドラッグして、ソースフレームとエンコードされたフレームを比較してください。比較対象は「一時停止時の画質」であり、「動的な画質」ではないことに注意してください",
            ["PreviewButtonText"] = "プレビュー",
            ["CancelButtonText"] = "キャンセル",
            ["StatusReady"] = "準備完了",
            ["StatusExtracting"] = "ソースフレームを抽出中...",
            ["StatusConverting"] = "ソースフレームを変換中（{0}）...",
            ["StatusEncoding"] = "{0} でエンコード中...",
            ["StatusDecoding"] = "プレビューフレームをデコード中...",
            ["StatusPreviewReady"] = "プレビュー準備完了：{0}、CRF {1}",
            ["StatusComputingScores"] = "画質スコアを計算中...",
            ["StatusCancelled"] = "プレビューがキャンセルされました",
            ["StatusNoFfmpeg"] = "ffmpeg.exe がインポートされていません",
            ["StatusNoSource"] = "有効な動画ソースが選択されていません",
            ["StatusDisplayModeBlocked"] = "プレビュー実行中は表示モードを変更できません",
            ["StatusDisplayModeSet"] = "表示モード：{0}",
            ["DisplayModeRaw"] = "生",
            ["DisplayModeLowToBt709"] = "低色域→BT.709",
            ["DisplayModeWcgToBt709"] = "WCG→BT.709",
            ["DisplayModeHdrToSdr"] = "HDR→SDR",
            ["DisplayModeHighHdrToSdr"] = "高HDR→SDR",
            ["WarnSvtAv1No12Bit"] = "libsvtav1 は 12ビットソースのプレビューをサポートしていません。\nlibx265 を使用するか、別のソースを選択してください。",
            ["Ssimulacra2ToolMissing"] = "SSIMULACRA2.1 ツールが見つかりません。x64-CloudinarySSIMULACRA2.1 を実行ファイルの隣に配置して再起動してください。",
            ["Ssimulacra2ToolPresent"] = "SSIMULACRA2.1 品質メトリックの準備ができました。",
            ["SsimulacraScoreHint"] = "SSIMULACRA スコア：100 数学的無損失 | 90 視覚的無損失 | 85 わずかな損失 | 80 顕著な損失 | 70 目に見える損失 | 50 大きな損失 | 30 致命的な損失",
            ["ButteraugliToolMissing"] = "Butteraugli ツールが見つかりません。x64-GoogleButteraugli を実行ファイルの隣に配置して再起動してください。",
            ["ButteraugliToolPresent"] = "Butteraugli 品質メトリックの準備ができました。",
            ["ButteraugliScoreHint"] = "Butteraugli スコア（ヒューリスティック）：<1 理論的ロスレス | 1-2 視覚的ロスレス | 2-4 軽微な損失 | 4-6 顕著な損失 | 6-8 大きな損失 | >8 深刻な損失",
        };
        Data["ru"] = new(Data["en"])
        {
            ["EncoderLabel"] = "Кодек",
            ["DisplayModeLabel"] = "Экран",
            ["ZoomLabel"] = "Масштаб",
            ["PositionLabel"] = "Положение",
            ["FitButtonText"] = "По размеру",
            ["RawButtonText"] = "Сырой",
            ["Hint1Text"] = "Сжатие выполняется только после нажатия кнопки «Предварительный просмотр» из-за низкой скорости работы некоторых кодеков",
            ["Hint2Text"] = "Предварительный просмотр осуществляется только через ffmpeg для обеспечения удобства использования, если кодер не импортирован",
            ["Hint3Text"] = "Перетащите разделительную линию для сравнения; имейте в виду, что вы сравниваете качество изображения в режиме «пауза», а не в режиме «движение»",
            ["PreviewButtonText"] = "Предпросмотр",
            ["CancelButtonText"] = "Отмена",
            ["StatusReady"] = "Готово",
            ["StatusExtracting"] = "Извлечение исходного кадра...",
            ["StatusConverting"] = "Преобразование исходного кадра ({0})...",
            ["StatusEncoding"] = "Кодирование с {0}...",
            ["StatusDecoding"] = "Декодирование кадра предпросмотра...",
            ["StatusPreviewReady"] = "Предпросмотр готов: {0}, CRF {1}",
            ["StatusComputingScores"] = "Вычисление метрик качества...",
            ["StatusCancelled"] = "Предпросмотр отменён",
            ["StatusNoFfmpeg"] = "ffmpeg.exe не импортирован",
            ["StatusNoSource"] = "Не выбран действительный источник видео",
            ["StatusDisplayModeBlocked"] = "Нельзя сменить режим отображения во время предпросмотра",
            ["StatusDisplayModeSet"] = "Режим отображения: {0}",
            ["DisplayModeRaw"] = "Сырой",
            ["DisplayModeLowToBt709"] = "Низкий→BT.709",
            ["DisplayModeWcgToBt709"] = "WCG→BT.709",
            ["DisplayModeHdrToSdr"] = "HDR→SDR",
            ["DisplayModeHighHdrToSdr"] = "Высокий HDR→SDR",
            ["WarnSvtAv1No12Bit"] = "libsvtav1 не поддерживает предварительный просмотр 12-битных источников.\nПожалуйста, используйте libx265 или выберите другой источник.",
            ["Ssimulacra2ToolMissing"] = "Инструмент SSIMULACRA2.1 не найден. Поместите x64-CloudinarySSIMULACRA2.1 рядом с исполняемым файлом и перезапустите.",
            ["Ssimulacra2ToolPresent"] = "Метрика качества SSIMULACRA2.1 готова.",
            ["SsimulacraScoreHint"] = "Оценка SSIMULACRA: 100 Мате. без потерь | 90 Визуально без потерь | 85 Незначительные потери | 80 Заметные | 70 Видимые | 50 Значительные | 30 Критические",
            ["ButteraugliToolMissing"] = "Инструмент Butteraugli не найден. Поместите x64-GoogleButteraugli рядом с исполняемым файлом и перезапустите.",
            ["ButteraugliToolPresent"] = "Метрика качества Butteraugli готова.",
            ["ButteraugliScoreHint"] = "Оценка Butteraugli (эвристическая): <1 Теоретически без потерь | 1-2 Визуально без потерь | 2-4 Небольшая потеря | 4-6 Заметная потеря | 6-8 Значительная потеря | >8 Критическая потеря",
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
    public string StatusComputingScores { get; }
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
    public string WarnSvtAv1No12Bit { get; }
    public string Ssimulacra2ToolMissing { get; }
    public string Ssimulacra2ToolPresent { get; }
    public string SsimulacraScoreHint { get; }
    public string ButteraugliToolMissing { get; }
    public string ButteraugliToolPresent { get; }
    public string ButteraugliScoreHint { get; }

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
        StatusComputingScores = _d["StatusComputingScores"];
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
        WarnSvtAv1No12Bit = _d["WarnSvtAv1No12Bit"];
        Ssimulacra2ToolMissing = _d["Ssimulacra2ToolMissing"];
        Ssimulacra2ToolPresent = _d["Ssimulacra2ToolPresent"];
        SsimulacraScoreHint = _d["SsimulacraScoreHint"];
        ButteraugliToolMissing = _d["ButteraugliToolMissing"];
        ButteraugliToolPresent = _d["ButteraugliToolPresent"];
        ButteraugliScoreHint = _d["ButteraugliScoreHint"];
    }
}
