namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Localized strings for the image A/B preview.
/// </summary>
public class ImgABPvLangProvider : LangProviderBase
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["EncoderLabel"] = "Encoder",
            ["DisplayModeLabel"] = "Display",
            ["ZoomLabel"] = "Zoom",
            ["PositionLabel"] = "Frame#",
            ["RawButtonText"] = "Raw",
            ["Hint1Text"] = "Compression is only performed after clicking Preview due to the slowness of some encoders",
            ["Hint2Text"] = "Preview is only via ffmpeg to ensure usability when no encoder is imported",
            ["Hint3Text"] = "Drag the separator line to compare; beware that you are comparing \u201Cpaused\u201D, not \u201Cmotion\u201D picture quality",
            ["StatusReady"] = "Ready",
            ["StatusExtracting"] = "Extracting source frame...",
            ["StatusConverting"] = "Converting source frame ({0})...",
            ["StatusEncoding"] = "Encoding with {0}...",
            ["StatusDecoding"] = "Decoding preview frame...",
            ["StatusPreviewReady"] = "Preview ready: {0}, CRF/QP {1}",
            ["StatusComputingScores"] = "Computing quality metrics...",
            ["StatusCancelled"] = "Preview cancelled",
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
            ["PositionLabel"] = "帧#",
            ["RawButtonText"] = "原始",
            ["Hint1Text"] = "由于有的编码器较慢，因此压缩操作仅在点击预览后运行",
            ["Hint2Text"] = "预览仅用 ffmpeg 以确保在未导入编码器时仍可预览",
            ["Hint3Text"] = "拖拽分割线来比较源帧与编码帧；注意你在对比的是「暂停画质」而非「动态画质」",
            ["StatusReady"] = "就绪",
            ["StatusExtracting"] = "正在提取源帧...",
            ["StatusConverting"] = "正在转换源帧（{0}）...",
            ["StatusEncoding"] = "正在用 {0} 编码...",
            ["StatusDecoding"] = "正在解码预览帧...",
            ["StatusPreviewReady"] = "预览就绪：{0}，CRF/QP {1}",
            ["StatusComputingScores"] = "正在计算画质跑分...",
            ["StatusCancelled"] = "预览已取消",
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
            ["PositionLabel"] = "幀#",
            ["RawButtonText"] = "原始",
            ["Hint1Text"] = "由於有的編碼器較慢，因此壓縮操作僅在點擊預覽後運行",
            ["Hint2Text"] = "預覽僅用 ffmpeg 以確保在未導入編碼器時仍可預覽",
            ["Hint3Text"] = "拖拽分割線來比較源幀與編碼幀；注意你在對比的是「暫停畫質」而非「動態畫質」",
            ["StatusReady"] = "就緒",
            ["StatusExtracting"] = "正在提取源幀...",
            ["StatusConverting"] = "正在轉換源幀（{0}）...",
            ["StatusEncoding"] = "正在用 {0} 編碼...",
            ["StatusDecoding"] = "正在解碼預覽幀...",
            ["StatusPreviewReady"] = "預覽就緒：{0}，CRF/QP {1}",
            ["StatusComputingScores"] = "正在計算畫質跑分...",
            ["StatusCancelled"] = "預覽已取消",
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

    static ImgABPvLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["EncoderLabel"] = "Encodeur",
            ["DisplayModeLabel"] = "Affichage",
            ["ZoomLabel"] = "Zoom",
            ["PositionLabel"] = "Cadre#",
            ["RawButtonText"] = "Brut",
            ["Hint1Text"] = "La compression n'est effectuée qu'après avoir cliqué sur « Aperçu », en raison de la lenteur de certains encodeurs",
            ["Hint2Text"] = "L'aperçu est uniquement réalisé via ffmpeg afin de garantir la compatibilité même sans encodeur importé",
            ["Hint3Text"] = "Faites glisser la ligne de séparation pour comparer ; attention, vous comparez la qualité d'image à l'arrêt, et non en mouvement",
            ["StatusReady"] = "Prêt",
            ["StatusExtracting"] = "Extraction de l'image source...",
            ["StatusConverting"] = "Conversion de l'image source ({0})...",
            ["StatusEncoding"] = "Encodage avec {0}...",
            ["StatusDecoding"] = "Décodage de l'aperçu...",
            ["StatusPreviewReady"] = "Aperçu prêt : {0}, CRF/QP {1}",
            ["StatusComputingScores"] = "Calcul des métriques de qualité...",
            ["StatusCancelled"] = "Aperçu annulé",
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
            ["PositionLabel"] = "Fotograma#",
            ["RawButtonText"] = "Crudo",
            ["Hint1Text"] = "La compresión solo se realiza después de hacer clic en Vista previa debido a la lentitud de algunos codificadores",
            ["Hint2Text"] = "La vista previa solo se realiza mediante ffmpeg para garantizar su usabilidad cuando no se importa ningún codificador",
            ["Hint3Text"] = "Arrastre la línea separadora para comparar; tenga en cuenta que está comparando la calidad de la imagen en pausa, no en movimiento",
            ["StatusReady"] = "Listo",
            ["StatusExtracting"] = "Extrayendo fotograma fuente...",
            ["StatusConverting"] = "Convirtiendo fotograma fuente ({0})...",
            ["StatusEncoding"] = "Codificando con {0}...",
            ["StatusDecoding"] = "Decodificando vista previa...",
            ["StatusPreviewReady"] = "Vista previa lista: {0}, CRF/QP {1}",
            ["StatusComputingScores"] = "Calculando métricas de calidad...",
            ["StatusCancelled"] = "Vista previa cancelada",
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
            ["PositionLabel"] = "コマ#",
            ["RawButtonText"] = "生",
            ["Hint1Text"] = "一部のエンコーダーの処理速度が遅いため、圧縮はプレビューをクリックした後にのみ実行されます",
            ["Hint2Text"] = "プレビューは、エンコーダーがインポートされていない場合でもプレビューが可能であることを確認するために、ffmpegでのみ使用されます",
            ["Hint3Text"] = "区切り線をドラッグして、ソースフレームとエンコードされたフレームを比較してください。比較対象は「一時停止時の画質」であり、「動的な画質」ではないことに注意してください",
            ["StatusReady"] = "準備完了",
            ["StatusExtracting"] = "ソースフレームを抽出中...",
            ["StatusConverting"] = "ソースフレームを変換中（{0}）...",
            ["StatusEncoding"] = "{0} でエンコード中...",
            ["StatusDecoding"] = "プレビューフレームをデコード中...",
            ["StatusPreviewReady"] = "プレビュー準備完了：{0}、CRF/QP {1}",
            ["StatusComputingScores"] = "画質スコアを計算中...",
            ["StatusCancelled"] = "プレビューがキャンセルされました",
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
            ["PositionLabel"] = "Кадр#",
            ["RawButtonText"] = "Сырой",
            ["Hint1Text"] = "Сжатие выполняется только после нажатия кнопки «Предварительный просмотр» из-за низкой скорости работы некоторых кодеков",
            ["Hint2Text"] = "Предварительный просмотр осуществляется только через ffmpeg для обеспечения удобства использования, если кодер не импортирован",
            ["Hint3Text"] = "Перетащите разделительную линию для сравнения; имейте в виду, что вы сравниваете качество изображения в режиме «пауза», а не в режиме «движение»",
            ["StatusReady"] = "Готово",
            ["StatusExtracting"] = "Извлечение исходного кадра...",
            ["StatusConverting"] = "Преобразование исходного кадра ({0})...",
            ["StatusEncoding"] = "Кодирование с {0}...",
            ["StatusDecoding"] = "Декодирование кадра предпросмотра...",
            ["StatusPreviewReady"] = "Предпросмотр готов: {0}, CRF/QP {1}",
            ["StatusComputingScores"] = "Вычисление метрик качества...",
            ["StatusCancelled"] = "Предпросмотр отменён",
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
            ["ButteraugliScoreHint"] = "Оценка Butteraugli (эвристическая): <1 Теоретически без потерь | 1-2 Визуально без потерь | 2-4 Небольшая потеря | 4-6 Заметная потеря | 6-8 Значительная потеря | >8 Критическая потеря"
        };
        Data["de"] = new(Data["en"])
        {
            ["EncoderLabel"] = "Encoder",
            ["DisplayModeLabel"] = "Anzeige",
            ["ZoomLabel"] = "Zoom",
            ["PositionLabel"] = "Einzelbild#",
            ["RawButtonText"] = "Roh",
            ["Hint1Text"] = "Komprimierung erfolgt erst nach Klick auf Vorschau aufgrund der Langsamkeit mancher Encoder",
            ["Hint2Text"] = "Vorschau nur via ffmpeg, um Nutzbarkeit ohne importierten Encoder sicherzustellen",
            ["Hint3Text"] = "Trennlinie zum Vergleich ziehen; beachten Sie, dass Sie \"angehalten\" und nicht \"Bewegung\" vergleichen",
            ["StatusReady"] = "Bereit",
            ["StatusExtracting"] = "Quellframe wird extrahiert...",
            ["StatusConverting"] = "Quellframe wird konvertiert ({0})...",
            ["StatusEncoding"] = "Kodierung mit {0}...",
            ["StatusDecoding"] = "Vorschauframe wird dekodiert...",
            ["StatusPreviewReady"] = "Vorschau bereit: {0}, CRF/QP {1}",
            ["StatusComputingScores"] = "Qualitätsmetriken werden berechnet...",
            ["StatusCancelled"] = "Vorschau abgebrochen",
            ["StatusNoSource"] = "Keine gültige Videoquelle ausgewählt",
            ["StatusDisplayModeBlocked"] = "Anzeigemodus kann während Vorschau nicht geändert werden",
            ["StatusDisplayModeSet"] = "Anzeigemodus: {0}",
            ["DisplayModeRaw"] = "Roh",
            ["DisplayModeLowToBt709"] = "Enger Farbraum → BT.709",
            ["DisplayModeWcgToBt709"] = "WCG → BT.709",
            ["DisplayModeHdrToSdr"] = "HDR → SDR",
            ["DisplayModeHighHdrToSdr"] = "Hoher HDR → SDR",
            ["WarnSvtAv1No12Bit"] = "libsvtav1 unterstützt keine 12-Bit-Quellvorschau.\nBitte libx265 verwenden oder eine andere Quelle wählen.",
            ["Ssimulacra2ToolMissing"] = "SSIMULACRA2.1-Tool nicht gefunden. x64-CloudinarySSIMULACRA2.1 neben dem Executable platzieren und neu starten.",
            ["Ssimulacra2ToolPresent"] = "SSIMULACRA2.1-Qualitätsmetrik bereit.",
            ["SsimulacraScoreHint"] = "SSIMULACRA-Punktzahl: 100 Mathematisch verlustfrei | 90 Visuell verlustfrei | 85 Minimaler Verlust | 80 Sichtbar | 70 Deutlich | 50 Erheblich | 30 Gravierend",
            ["ButteraugliToolMissing"] = "Butteraugli-Tool nicht gefunden. x64-GoogleButteraugli neben dem Executable platzieren und neu starten.",
            ["ButteraugliToolPresent"] = "Butteraugli-Qualitätsmetrik bereit.",
            ["ButteraugliScoreHint"] = "Butteraugli-Punktzahl (heuristisch): <1 Theoretisch verlustfrei | 1-2 Visuell verlustfrei | 2-4 Leichter Verlust | 4-6 Deutlicher Verlust | 6-8 Schwerer Verlust | >8 Gravierender Verlust"
        };
        Data["ko"] = new(Data["en"])
        {
            ["EncoderLabel"] = "엔코더",
            ["DisplayModeLabel"] = "표시",
            ["ZoomLabel"] = "줌",
            ["PositionLabel"] = "프레임#",
            ["RawButtonText"] = "원본",
            ["Hint1Text"] = "일부 엔코더가 느려서 압축은 미리보기 클릭 후에만 실행됩니다",
            ["Hint2Text"] = "엔코더가 가져와지지 않은 경우에도 사용성을 보장하기 위해 미리보기는 ffmpeg로만 제공됩니다",
            ["Hint3Text"] = "구분선을 드래그하여 비교하세요. 비교 대상은 \"일시정지\" 상태의 화질이지 \"동적\" 화질이 아닙니다",
            ["StatusReady"] = "준비 완료",
            ["StatusExtracting"] = "소스 프레임 추출 중...",
            ["StatusConverting"] = "소스 프레임 변환 중 ({0})...",
            ["StatusEncoding"] = "{0}(으)로 인코딩 중...",
            ["StatusDecoding"] = "미리보기 프레임 디코딩 중...",
            ["StatusPreviewReady"] = "미리보기 준비 완료: {0}, CRF/QP {1}",
            ["StatusComputingScores"] = "화질 지표 계산 중...",
            ["StatusCancelled"] = "미리보기 취소됨",
            ["StatusNoSource"] = "유효한 비디오 소스가 선택되지 않음",
            ["StatusDisplayModeBlocked"] = "미리보기 실행 중에는 표시 모드를 변경할 수 없습니다",
            ["StatusDisplayModeSet"] = "표시 모드: {0}",
            ["DisplayModeRaw"] = "원본",
            ["DisplayModeLowToBt709"] = "낮은 색역 → BT.709",
            ["DisplayModeWcgToBt709"] = "WCG → BT.709",
            ["DisplayModeHdrToSdr"] = "HDR → SDR",
            ["DisplayModeHighHdrToSdr"] = "높은 HDR → SDR",
            ["WarnSvtAv1No12Bit"] = "libsvtav1은 12비트 소스 미리보기를 지원하지 않습니다.\nlibx265를 사용하거나 다른 소스를 선택하세요.",
            ["Ssimulacra2ToolMissing"] = "SSIMULACRA2.1 도구를 찾을 수 없습니다. x64-CloudinarySSIMULACRA2.1을 실행 파일 옆에 놓고 다시 시작하세요.",
            ["Ssimulacra2ToolPresent"] = "SSIMULACRA2.1 화질 지표 준비 완료",
            ["SsimulacraScoreHint"] = "SSIMULACRA 점수: 100 수학적 무손실 | 90 시각적 무손실 | 85 미미한 손실 | 80 드러난 손실 | 70 눈에 보이는 손실 | 50 상당한 손실 | 30 치명적 손실",
            ["ButteraugliToolMissing"] = "Butteraugli 도구를 찾을 수 없습니다. x64-GoogleButteraugli을 실행 파일 옆에 놓고 다시 시작하세요.",
            ["ButteraugliToolPresent"] = "Butteraugli 화질 지표 준비 완료",
            ["ButteraugliScoreHint"] = "Butteraugli 점수(경험적): <1 이론적 무손실 | 1-2 시각적 무손실 | 2-4 경미한 손실 | 4-6 눈에 띄는 손실 | 6-8 심각한 손실 | >8 극심한 손실"
        };
        Data["pt-br"] = new(Data["en"])
        {
            ["EncoderLabel"] = "Codificador",
            ["DisplayModeLabel"] = "Exibição",
            ["ZoomLabel"] = "Zoom",
            ["PositionLabel"] = "Quadro#",
            ["RawButtonText"] = "Bruto",
            ["Hint1Text"] = "A compressão só é realizada após clicar em Visualizar devido à lentidão de alguns codificadores",
            ["Hint2Text"] = "A visualização é apenas via ffmpeg para garantir usabilidade quando nenhum codificador está importado",
            ["Hint3Text"] = "Arraste a linha separadora para comparar; observe que você está comparando qualidade de imagem \"pausada\", não \"em movimento\"",
            ["StatusReady"] = "Pronto",
            ["StatusExtracting"] = "Extraindo quadro fonte...",
            ["StatusConverting"] = "Convertendo quadro fonte ({0})...",
            ["StatusEncoding"] = "Codificando com {0}...",
            ["StatusDecoding"] = "Decodificando quadro de visualização...",
            ["StatusPreviewReady"] = "Visualização pronta: {0}, CRF/QP {1}",
            ["StatusComputingScores"] = "Calculando métricas de qualidade...",
            ["StatusCancelled"] = "Visualização cancelada",
            ["StatusNoSource"] = "Nenhuma fonte de vídeo válida selecionada",
            ["StatusDisplayModeBlocked"] = "O modo de exibição não pode ser alterado durante a visualização",
            ["StatusDisplayModeSet"] = "Modo de exibição: {0}",
            ["DisplayModeRaw"] = "Bruto",
            ["DisplayModeLowToBt709"] = "Gamut baixo → BT.709",
            ["DisplayModeWcgToBt709"] = "WCG → BT.709",
            ["DisplayModeHdrToSdr"] = "HDR → SDR",
            ["DisplayModeHighHdrToSdr"] = "HDR alto → SDR",
            ["WarnSvtAv1No12Bit"] = "libsvtav1 não suporta visualização de fontes de 12 bits.\nUse libx265 ou selecione uma fonte diferente.",
            ["Ssimulacra2ToolMissing"] = "Ferramenta SSIMULACRA2.1 não encontrada. Coloque x64-CloudinarySSIMULACRA2.1 ao lado do executável e reinicie.",
            ["Ssimulacra2ToolPresent"] = "Métrica de qualidade SSIMULACRA2.1 está pronta.",
            ["SsimulacraScoreHint"] = "Pontuação SSIMULACRA: 100 Sem perda (matemático) | 90 Sem perda visual | 85 Perda mínima | 80 Reveladora | 70 Visível | 50 Substancial | 30 Grave",
            ["ButteraugliToolMissing"] = "Ferramenta Butteraugli não encontrada. Coloque x64-GoogleButteraugli ao lado do executável e reinicie.",
            ["ButteraugliToolPresent"] = "Métrica de qualidade Butteraugli está pronta.",
            ["ButteraugliScoreHint"] = "Pontuação Butteraugli (heurística): <1 Teoricamente sem perda | 1-2 Visualmente sem perda | 2-4 Perda leve | 4-6 Perda perceptível | 6-8 Perda pesada | >8 Perda severa",
        };
    }

    public string EncoderLabel { get; }
    public string DisplayModeLabel { get; }
    public string ZoomLabel { get; }
    public string PositionLabel { get; }
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

    public ImgABPvLangProvider(string languageCode) : base(languageCode, Data)
    {
        StatusNoFfmpeg = "!ffmpeg.exe";
        EncoderLabel = this["EncoderLabel"];
        DisplayModeLabel = this["DisplayModeLabel"];
        ZoomLabel = this["ZoomLabel"];
        PositionLabel = this["PositionLabel"];
        RawButtonText = this["RawButtonText"];
        Hint1Text = this["Hint1Text"];
        Hint2Text = this["Hint2Text"];
        Hint3Text = this["Hint3Text"];
        PreviewButtonText = this["PreviewButtonText"];
        CancelButtonText = this["CancelButtonText"];
        StatusReady = this["StatusReady"];
        StatusExtracting = this["StatusExtracting"];
        StatusConverting = this["StatusConverting"];
        StatusEncoding = this["StatusEncoding"];
        StatusDecoding = this["StatusDecoding"];
        StatusPreviewReady = this["StatusPreviewReady"];
        StatusComputingScores = this["StatusComputingScores"];
        StatusCancelled = this["StatusCancelled"];
        StatusNoSource = this["StatusNoSource"];
        StatusDisplayModeBlocked = this["StatusDisplayModeBlocked"];
        StatusDisplayModeSet = this["StatusDisplayModeSet"];
        DisplayModeRaw = this["DisplayModeRaw"];
        DisplayModeLowToBt709 = this["DisplayModeLowToBt709"];
        DisplayModeWcgToBt709 = this["DisplayModeWcgToBt709"];
        DisplayModeHdrToSdr = this["DisplayModeHdrToSdr"];
        DisplayModeHighHdrToSdr = this["DisplayModeHighHdrToSdr"];
        WarnSvtAv1No12Bit = this["WarnSvtAv1No12Bit"];
        Ssimulacra2ToolMissing = this["Ssimulacra2ToolMissing"];
        Ssimulacra2ToolPresent = this["Ssimulacra2ToolPresent"];
        SsimulacraScoreHint = this["SsimulacraScoreHint"];
        ButteraugliToolMissing = this["ButteraugliToolMissing"];
        ButteraugliToolPresent = this["ButteraugliToolPresent"];
        ButteraugliScoreHint = this["ButteraugliScoreHint"];
    }
}
