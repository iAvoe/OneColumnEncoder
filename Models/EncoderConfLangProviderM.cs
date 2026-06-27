namespace OneColumnEncoder.Models;

public class EncoderConfLangProviderM
{
    public static EncoderConfLangProviderM Current { get; private set; } = null!;
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["RateControlTitle"] = "Rate Control Mechanism",
            ["CustomParamsTitle"] = "Custom Parameters",
            ["CrfModeText"] = "Constant Rate Factor (CRF) Mode",
            ["AbrModeText"] = "Average Bitrate (ABR) Mode",
            ["X264Text"] = "x264",
            ["X265Text"] = "x265",
            ["SvtAv1Text"] = "SVT-AV1",
            ["X264DefaultText"] = "(default 23)",
            ["X265DefaultText"] = "(default 28)",
            ["SvtAv1DefaultText"] = "(default 35)",
            ["X264AbrValueText"] = "209",
            ["X265AbrValueText"] = "200",
            ["SvtAv1AbrValueText"] = "200",
            ["BasicParamsText"] = "Basic Parameters",
            ["KeyframeSecondsText"] = "Keyframe Interval (seconds)",
            ["ThirdPartyParamsText"] = "3rd Party Extended Parameters",
            ["FreeTextControlTitle"] = "Additional Encoder Parameters (CLI Format)",
            ["PreviewFreeTextControlTitle"] = "Additional Encoder Parameters (Preview/FFmpeg Format)",
            ["PreviewFreeTextHint"] = "Use FFmpeg API format: -x264-params, -x265-params, -svtav1-params",
            ["X264ModText"] = "x264 Mod：Film Grain Rate Distortion Optimization (FGO-RD)",
            ["X265JpsdrAqText"] = "x265 jpsdr Mod: Adaptive Quantization (AQ) Hysteresis",
            ["X265JpsdrDarkText"] = "x265 jpsdr Mod: ↑AQ Strength for Dark Scenes",
            ["X265JpsdrTextureText"] = "x265 jpsdr Mod: ↑AQ Strength for Edges",
            ["SvtAv1EssentialDl2Text"] = "SVT-AV1-Essential: Precise Deblocking Filter (DLF2)",
            ["SvtAv1EssentialAutoTileText"] = "SVT-AV1-Essential: Auto Tile Size",
            ["CrfHintText"] = "CRF Slider Ticks—Lossless | UHQ | HQ | Streaming",
            ["AbrHintText"] = "ABR Slider Ticks (2K60)—Near Lossless | Near Lossless→HQ | UHQ→Streaming | HQ→Streaming",
            ["KeyframeHintText1"] = "Keyint Slider Ticks (Decoding Difficulty):\nEco./Multi-track Edit | Mid. | Hard & Mid. Compression | Extreme & High Comp.",
            ["KeyframeHintText2"] = "↑Resolution=↑Decoding difficulty, chg. keyframe dist. helps ↓decode-index difficulty or ↑compression",
            ["ThirdPartyHintText1"] = "“File Grain Opt.” heavily biases toward sharpness, not compression",
            ["ThirdPartyHintText2"] = "“AQ Hysteresis” prevents flashcut/montages from disrupting current AQ strategy",
            ["ThirdPartyHintText3"] = "“High precision deblocking”May reduce blurriness",
            ["ThirdPartyHintText4"] = "“Auto Tile” may better balance the distribution of bitrate",
            ["GeneralPurposeText"] = "General Purpose",
            ["StockFootageText"] = "Stock Footage",
            ["FilmIRLText"] = "Film / IRL Shoot",
            ["AnimeText"] = "Anime",
            ["StressTestText"] = "Stress Test",
            ["PeakQualityText"] = "Peak Quality",
            ["CompressionOptText"] = "High Compression",
            ["SpeedOptimizedText"] = "Fast",
            ["BlankPresetText"] = "None",
            ["VvencText"] = "VVenC (Preview)",
            ["VvencHintText"] = "VVenC is only for single-image preview, not used in main encoding",
            ["BlankPresetHint"] = "None: use encoder default / fully customize via additional parameter textbox",
            ["CancelButtonText"] = "Cancel",
            ["ConfirmButtonText"] = "Confirm",
        },
        ["zh-cn"] = new()
        {
            ["RateControlTitle"] = "码率控制策略",
            ["CustomParamsTitle"] = "自定义参数",
            ["CrfModeText"] = "码率调谐常量（CRF）模式",
            ["AbrModeText"] = "平均码率（ABR）模式",
            ["X264Text"] = "x264",
            ["X265Text"] = "x265",
            ["SvtAv1Text"] = "SVT-AV1",
            ["X264DefaultText"] = "（默认 23）",
            ["X265DefaultText"] = "（默认 28）",
            ["SvtAv1DefaultText"] = "（默认 35）",
            ["X264AbrValueText"] = "209",
            ["X265AbrValueText"] = "200",
            ["SvtAv1AbrValueText"] = "200",
            ["BasicParamsText"] = "基础参数",
            ["KeyframeSecondsText"] = "关键帧距离秒数",
            ["ThirdPartyParamsText"] = "第三方扩展参数（不支持或不确定则关）",
            ["FreeTextControlTitle"] = "附加编码器参数（CLI 格式）",
            ["PreviewFreeTextControlTitle"] = "附加编码器参数（预览/FFmpeg 格式）",
            ["PreviewFreeTextHint"] = "使用 FFmpeg API 格式：-x264-params, -x265-params, -svtav1-params",
            ["X264ModText"] = "x264 Mod：基于高频信息的率失真优化（FGO-RD）",
            ["X265JpsdrAqText"] = "x265 jpsdr Mod：自适应量化迟滞（AQ Hysteresis）",
            ["X265JpsdrDarkText"] = "x265 jpsdr Mod：对暗场提高自适应量化强度",
            ["X265JpsdrTextureText"] = "x265 jpsdr Mod：对纹理提高自适应量化强度",
            ["SvtAv1EssentialDl2Text"] = "SVT-AV1-Essential：高精度去块滤镜（DLF2）",
            ["SvtAv1EssentialAutoTileText"] = "SVT-AV1-Essential：自动计算瓦片大小（Auto Tile）",
            ["CrfHintText"] = "CRF 刻度—无损 | 超清 | 高清 | 流媒体",
            ["AbrHintText"] = "ABR 刻度（2K60）—近无损 | 近无损→高清 | 超清→流媒体 | 高清→流媒体",
            ["KeyframeHintText1"] = "关键帧间隔刻度：\n低功耗观影/多轨剪辑 | 中等解码与进度条索引难度 | 较难解码与索引/中压缩 | 很难解码/高压缩",
            ["KeyframeHintText2"] = "片源分辨率高则解码/索引难度上升；背景不动时下降，此时可调整关键帧距离以降低难度或增加压缩",
            ["ThirdPartyHintText1"] = "「高频基准率失真优化」偏向保留锐利细节，但不利于压缩",
            ["ThirdPartyHintText2"] = "「自适应量化迟滞」避免让频繁切换块或短暂闪过的画面影响到当前 AQ 策略",
            ["ThirdPartyHintText3"] = "「高精度去块滤镜」可能会减少模糊程度",
            ["ThirdPartyHintText4"] = "「自动瓦片大小」理论上能提高码率分配的精度 / 均衡程度",
            ["GeneralPurposeText"] = "通用",
            ["StockFootageText"] = "剪辑素材",
            ["FilmIRLText"] = "电影/实拍",
            ["AnimeText"] = "动画",
            ["StressTestText"] = "压力测试",
            ["PeakQualityText"] = "极致画质",
            ["CompressionOptText"] = "优化压缩",
            ["SpeedOptimizedText"] = "优化速度",
            ["BlankPresetText"] = "留空",
            ["VvencText"] = "VVenC（预览限定）",
            ["VvencHintText"] = "VVenC 仅用于单帧预览，不参与主编码流程",
            ["BlankPresetHint"] = "留空：使用编码器默认或底部附加编码器参数文本框写入",
            ["CancelButtonText"] = "取消",
            ["ConfirmButtonText"] = "确认",
        },
        ["zh-tw"] = new()
        {
            ["RateControlTitle"] = "碼率控制策略",
            ["CustomParamsTitle"] = "自定義參數",
            ["CrfModeText"] = "碼率調諧常量（CRF）模式",
            ["AbrModeText"] = "平均碼率（ABR）模式",
            ["X264Text"] = "x264",
            ["X265Text"] = "x265",
            ["SvtAv1Text"] = "SVT-AV1",
            ["X264DefaultText"] = "（預設 23）",
            ["X265DefaultText"] = "（預設 28）",
            ["SvtAv1DefaultText"] = "（預設 35）",
            ["X264AbrValueText"] = "209",
            ["X265AbrValueText"] = "200",
            ["SvtAv1AbrValueText"] = "200",
            ["BasicParamsText"] = "基礎參數",
            ["KeyframeSecondsText"] = "關鍵幀距離秒數",
            ["ThirdPartyParamsText"] = "第三方擴展參數（不支持或不確定則關）",
            ["FreeTextControlTitle"] = "附加編碼器參數（CLI 格式）",
            ["PreviewFreeTextControlTitle"] = "附加編碼器參數（預覽/FFmpeg 格式）",
            ["PreviewFreeTextHint"] = "使用 FFmpeg API 格式：-x264-params, -x265-params, -svtav1-params",
            ["X264ModText"] = "x264 Mod：基於高頻信息的率失真優化（FGO-RD）",
            ["X265JpsdrAqText"] = "x265 jpsdr Mod：自適應量化遲滯（AQ Hysteresis）",
            ["X265JpsdrDarkText"] = "x265 jpsdr Mod：對暗場提高自適應量化強度",
            ["X265JpsdrTextureText"] = "x265 jpsdr Mod：對紋理提高自適應量化強度",
            ["SvtAv1EssentialDl2Text"] = "SVT-AV1-Essential：高精度去塊濾鏡（DLF2）",
            ["SvtAv1EssentialAutoTileText"] = "SVT-AV1-Essential：自動計算瓦片大小（Auto Tile）",
            ["CrfHintText"] = "CRF 刻度—無損 | 超清 | 高清 | 流媒體",
            ["AbrHintText"] = "ABR 刻度—近無損 | 近無損→高清 | 超清→流媒體 | 高清→流媒體（以 2560x1440@60 為準）",
            ["KeyframeHintText1"] = "關鍵幀間隔刻度：\n低功耗觀影/多軌剪輯 | 中等解碼與進度條索引難度 | 較難解碼與索引/中壓縮 | 很難解碼/高壓縮",
            ["KeyframeHintText2"] = "片源分辨率高則解碼/索引難度上升；背景不動時下降，此時可調整關鍵幀距離以降低難度或增加壓縮",
            ["ThirdPartyHintText1"] = "「高頻基準率失真優化」偏向保留銳利細節，但不利於壓縮",
            ["ThirdPartyHintText2"] = "「自適應量化遲滯」避免讓頻繁切換塊或短暫閃過的畫面影響到當前 AQ 策略",
            ["ThirdPartyHintText3"] = "「高精度去塊濾鏡」可能會減少模糊程度",
            ["ThirdPartyHintText4"] = "「自動瓦片大小」理論上能提高碼率分配的精度 / 均衡程度",
            ["GeneralPurposeText"] = "通用",
            ["StockFootageText"] = "剪輯素材",
            ["FilmIRLText"] = "電影/實拍",
            ["AnimeText"] = "動畫",
            ["StressTestText"] = "壓力測試",
            ["PeakQualityText"] = "極致畫質",
            ["CompressionOptText"] = "優化壓縮",
            ["SpeedOptimizedText"] = "優化速度",
            ["BlankPresetText"] = "留空",
            ["VvencText"] = "VVenC（預覽限定）",
            ["VvencHintText"] = "VVenC 僅用於單幀預覽，不參與主編碼流程",
            ["BlankPresetHint"] = "留空：使用編碼器默認或底部附加編碼器參數文本框寫入",
            ["CancelButtonText"] = "取消",
            ["ConfirmButtonText"] = "確認",
        }
    };

    static EncoderConfLangProviderM()
    {
        Data["fr"] = new(Data["en"])
        {
            ["RateControlTitle"] = "Mode de contrôle du débit",
            ["CustomParamsTitle"] = "Paramètres personnalisés",
            ["CrfModeText"] = "Mode CRF (facteur de qualité constant)",
            ["AbrModeText"] = "Mode ABR (débit moyen)",
            ["BasicParamsText"] = "Paramètres de base",
            ["KeyframeSecondsText"] = "Intervalle d'images clés (s)",
            ["ThirdPartyParamsText"] = "Paramètres étendus tiers",
            ["FreeTextControlTitle"] = "Paramètres encodeur additionnels (format CLI)",
            ["PreviewFreeTextControlTitle"] = "Paramètres encodeur additionnels (format prévisualisation/FFmpeg)",
            ["PreviewFreeTextHint"] = "Utilisez le format API FFmpeg : -x264-params, -x265-params, -svtav1-params",
            ["X264ModText"] = "x264 Mod : optimisation R-D grain film (FGO-RD)",
            ["X265JpsdrAqText"] = "x265 jpsdr Mod : hystérésis AQ",
            ["X265JpsdrDarkText"] = "x265 jpsdr Mod : ↑AQ scènes sombres",
            ["X265JpsdrTextureText"] = "x265 jpsdr Mod : ↑AQ pour les textures",
            ["SvtAv1EssentialDl2Text"] = "SVT-AV1-Essential : déblocage précis (DLF2)",
            ["SvtAv1EssentialAutoTileText"] = "SVT-AV1-Essential : taille de tuile auto",
            ["CrfHintText"] = "CRF — Sans perte | UHQ | HQ | Streaming",
            ["AbrHintText"] = "ABR (2K60) — quasi sans perte | quasi sans perte→HQ | UHQ→Streaming | HQ→Streaming",
            ["KeyframeHintText1"] = "Keyint (difficulté décodage) :\nÉco./édition multi-piste | Moyen | Dur & compression moy. | Extrême & forte comp.",
            ["KeyframeHintText2"] = "↑Résolution=↑difficulté décodage; chg. dist. keyint ↓décodage-indexation ou ↑compression",
            ["ThirdPartyHintText1"] = "« Optimisation du grain film » privilégie fortement la netteté, pas la compression",
            ["ThirdPartyHintText2"] = "« Hystérésis AQ » évite que flashcuts/montages perturbent la stratégie AQ",
            ["ThirdPartyHintText3"] = "« Déblocage haute précision » peut réduire le flou",
            ["ThirdPartyHintText4"] = "« Auto Tile » peut mieux répartir le débit",
            ["GeneralPurposeText"] = "Général",
            ["StockFootageText"] = "Stock footage",
            ["FilmIRLText"] = "Film / prise réelle",
            ["AnimeText"] = "Animation",
            ["StressTestText"] = "Test de charge",
            ["PeakQualityText"] = "Qualité max.",
            ["CompressionOptText"] = "Compression élevée",
            ["SpeedOptimizedText"] = "Rapide",
            ["BlankPresetText"] = "Aucun",
            ["VvencText"] = "VVenC (Aperçu)",
            ["VvencHintText"] = "VVenC est réservé à l'aperçu d'une seule image, pas utilisé pour l'encodage principal",
            ["BlankPresetHint"] = "Aucun : défaut encodeur / personnalisation par paramètres additionnels",
            ["CancelButtonText"] = "Annuler",
            ["ConfirmButtonText"] = "Confirmer"
        };
        Data["es"] = new(Data["en"])
        {
            ["RateControlTitle"] = "Control de tasa",
            ["CustomParamsTitle"] = "Parámetros propios",
            ["CrfModeText"] = "Modo CRF (factor de calidad constante)",
            ["AbrModeText"] = "Modo ABR (bitrate medio)",
            ["BasicParamsText"] = "Parámetros básicos",
            ["KeyframeSecondsText"] = "Intervalo de keyframes (s)",
            ["ThirdPartyParamsText"] = "Parámetros extendidos de terceros",
            ["FreeTextControlTitle"] = "Parámetros extra del codificador (formato CLI)",
            ["PreviewFreeTextControlTitle"] = "Parámetros extra del codificador (formato previsualización/FFmpeg)",
            ["PreviewFreeTextHint"] = "Use formato API FFmpeg: -x264-params, -x265-params, -svtav1-params",
            ["X264ModText"] = "x264 Mod: optimización R-D de grano (FGO-RD)",
            ["X265JpsdrAqText"] = "x265 jpsdr Mod: histéresis AQ",
            ["X265JpsdrDarkText"] = "x265 jpsdr Mod: ↑AQ en escenas oscuras",
            ["X265JpsdrTextureText"] = "x265 jpsdr Mod: ↑AQ en texturas",
            ["SvtAv1EssentialDl2Text"] = "SVT-AV1-Essential: deblocking preciso (DLF2)",
            ["SvtAv1EssentialAutoTileText"] = "SVT-AV1-Essential: tamaño de tile auto",
            ["CrfHintText"] = "CRF — sin pérdida | UHQ | HQ | streaming",
            ["AbrHintText"] = "ABR (2K60) — casi sin pérdida | casi sin pérdida→HQ | UHQ→streaming | HQ→streaming",
            ["KeyframeHintText1"] = "Keyint (dificultad de decodificación):\nEco./edición multipista | Media | Difícil y comp. media | Extrema y alta comp.",
            ["KeyframeHintText2"] = "↑Resolución=↑dificultad decodif.; chg. dist. keyint ↓dificultad o ↑compresión",
            ["ThirdPartyHintText1"] = "«Grano de película» favorece nitidez, no compresión",
            ["ThirdPartyHintText2"] = "«Histéresis AQ» evita que flashcuts/montajes rompan la estrategia AQ",
            ["ThirdPartyHintText3"] = "«Deblocking de alta precisión» puede reducir borrosidad",
            ["ThirdPartyHintText4"] = "«Auto Tile» puede repartir mejor el bitrate",
            ["GeneralPurposeText"] = "General",
            ["StockFootageText"] = "Material de archivo",
            ["FilmIRLText"] = "Cine / grabación real",
            ["AnimeText"] = "Anime",
            ["StressTestText"] = "Prueba de estrés",
            ["PeakQualityText"] = "Calidad máxima",
            ["CompressionOptText"] = "Alta compresión",
            ["SpeedOptimizedText"] = "Rápido",
            ["BlankPresetText"] = "Ninguno",
            ["VvencText"] = "VVenC (Vista previa)",
            ["VvencHintText"] = "VVenC es solo para vista previa de imagen única, no se usa en codificación principal",
            ["BlankPresetHint"] = "Ninguno: usar defecto del codificador / personalizar abajo",
            ["CancelButtonText"] = "Cancelar",
            ["ConfirmButtonText"] = "Confirmar"
        };
        Data["ja"] = new(Data["en"])
        {
            ["RateControlTitle"] = "レート制御方式",
            ["CustomParamsTitle"] = "カスタムパラメータ",
            ["CrfModeText"] = "CRF（固定品質）モード",
            ["AbrModeText"] = "ABR（平均ビットレート）モード",
            ["BasicParamsText"] = "基本パラメータ",
            ["KeyframeSecondsText"] = "キーフレーム間隔 (秒)",
            ["ThirdPartyParamsText"] = "サードパーティ拡張パラメータ",
            ["FreeTextControlTitle"] = "追加エンコーダパラメータ（CLI 形式）",
            ["PreviewFreeTextControlTitle"] = "追加エンコーダパラメータ（プレビュー/FFmpeg 形式）",
            ["PreviewFreeTextHint"] = "FFmpeg API 形式を使用: -x264-params, -x265-params, -svtav1-params",
            ["X264ModText"] = "x264 Mod: フィルム粒子 R-D 最適化 (FGO-RD)",
            ["X265JpsdrAqText"] = "x265 jpsdr Mod: AQ ヒステリシス",
            ["X265JpsdrDarkText"] = "x265 jpsdr Mod: 暗部の AQ 強度↑",
            ["X265JpsdrTextureText"] = "x265 jpsdr Mod: エッジの AQ 強度↑",
            ["SvtAv1EssentialDl2Text"] = "SVT-AV1-Essential: 高精度デブロック (DLF2)",
            ["SvtAv1EssentialAutoTileText"] = "SVT-AV1-Essential: 自動タイルサイズ",
            ["CrfHintText"] = "CRF 目盛 — ロスレス | UHQ | HQ | 配信",
            ["AbrHintText"] = "ABR 目盛 (2K60) — ほぼロスレス | ほぼロスレス→HQ | UHQ→配信 | HQ→配信",
            ["KeyframeHintText1"] = "Keyint 目盛 (復号負荷):\n省電力/多トラック編集 | 中 | 高負荷・中圧縮 | 極高負荷・高圧縮",
            ["KeyframeHintText2"] = "解像度が高いほど復号/索引負荷も上昇。keyint 調整で負荷低減や圧縮向上が可能です",
            ["ThirdPartyHintText1"] = "「粒子最適化」は圧縮よりシャープさを優先します",
            ["ThirdPartyHintText2"] = "「AQ ヒステリシス」は急なカットで AQ 戦略が乱れるのを抑えます",
            ["ThirdPartyHintText3"] = "「高精度デブロック」はぼけを減らす場合があります",
            ["ThirdPartyHintText4"] = "「自動タイル」はビット配分を均衡させる場合があります",
            ["GeneralPurposeText"] = "汎用",
            ["StockFootageText"] = "素材映像",
            ["FilmIRLText"] = "映画 / 実写",
            ["AnimeText"] = "アニメ",
            ["StressTestText"] = "ストレステスト",
            ["PeakQualityText"] = "最高品質",
            ["CompressionOptText"] = "高圧縮",
            ["SpeedOptimizedText"] = "高速",
            ["BlankPresetText"] = "なし",
            ["VvencText"] = "VVenC（プレビュー）",
            ["VvencHintText"] = "VVenC は単一画像プレビューのみで使用、本エンコードでは使用しません",
            ["BlankPresetHint"] = "なし: エンコーダ既定値 / 下の追加パラメータで完全指定",
            ["CancelButtonText"] = "キャンセル",
            ["ConfirmButtonText"] = "確認"
        };
        Data["ru"] = new(Data["en"])
        {
            ["RateControlTitle"] = "Управление битрейтом",
            ["CustomParamsTitle"] = "Свои параметры",
            ["CrfModeText"] = "Режим CRF (постоянный фактор качества)",
            ["AbrModeText"] = "Режим ABR (средний битрейт)",
            ["BasicParamsText"] = "Базовые параметры",
            ["KeyframeSecondsText"] = "Интервал ключевых кадров (с)",
            ["ThirdPartyParamsText"] = "Расширения сторонних сборок",
            ["FreeTextControlTitle"] = "Доп. параметры кодера (CLI формат)",
            ["PreviewFreeTextControlTitle"] = "Доп. параметры кодера (предпросмотр/FFmpeg формат)",
            ["PreviewFreeTextHint"] = "Используйте формат FFmpeg API: -x264-params, -x265-params, -svtav1-params",
            ["X264ModText"] = "x264 Mod: R-D оптимизация зерна (FGO-RD)",
            ["X265JpsdrAqText"] = "x265 jpsdr Mod: гистерезис AQ",
            ["X265JpsdrDarkText"] = "x265 jpsdr Mod: ↑AQ для темных сцен",
            ["X265JpsdrTextureText"] = "x265 jpsdr Mod: ↑AQ для текстур",
            ["SvtAv1EssentialDl2Text"] = "SVT-AV1-Essential: точный deblocking (DLF2)",
            ["SvtAv1EssentialAutoTileText"] = "SVT-AV1-Essential: авторазмер tiles",
            ["CrfHintText"] = "CRF — lossless | UHQ | HQ | streaming",
            ["AbrHintText"] = "ABR (2K60) — почти lossless | почти lossless→HQ | UHQ→streaming | HQ→streaming",
            ["KeyframeHintText1"] = "Keyint (сложность декод.):\nэко/многодор. монтаж | средн. | сложно и средн. сжатие | экстрем. и высокое сжатие",
            ["KeyframeHintText2"] = "↑Разрешение=↑сложность декод.; изм. расст. keyint ↓декод.-индексации сложность или ↑сжатие",
            ["ThirdPartyHintText1"] = "«Film Grain Opt.» сильнее держит резкость, не сжатие",
            ["ThirdPartyHintText2"] = "«AQ Hysteresis» не дает flashcut/монтажу ломать стратегию AQ",
            ["ThirdPartyHintText3"] = "«Точный deblocking» может снизить размытость",
            ["ThirdPartyHintText4"] = "«Auto Tile» может лучше распределять битрейт",
            ["GeneralPurposeText"] = "Общее",
            ["StockFootageText"] = "Стоковое видео",
            ["FilmIRLText"] = "Фильм / съемка",
            ["AnimeText"] = "Аниме",
            ["StressTestText"] = "Стресс-тест",
            ["PeakQualityText"] = "Макс. качество",
            ["CompressionOptText"] = "Высокое сжатие",
            ["SpeedOptimizedText"] = "Быстро",
            ["BlankPresetText"] = "Нет",
            ["VvencText"] = "VVenC (предпросмотр)",
            ["VvencHintText"] = "VVenC используется только для предпросмотра одного кадра, не для основного кодирования",
            ["BlankPresetHint"] = "Нет: настройки кодера по умолчанию / полная настройка ниже",
            ["CancelButtonText"] = "Отмена",
            ["ConfirmButtonText"] = "Подтвердить"
        };
    }

    public const string WindowTitle = "1cenc Encoding Settings";
    public const string TitleText = WindowTitle;
    public string RateControlTitle { get; }
    public string CustomParamsTitle { get; }
    public string CrfModeText { get; }
    public string AbrModeText { get; }
    public string X264Text { get; }
    public string X265Text { get; }
    public string SvtAv1Text { get; }
    public string X264DefaultText { get; }
    public string X265DefaultText { get; }
    public string SvtAv1DefaultText { get; }
    public string X264AbrValueText { get; }
    public string X265AbrValueText { get; }
    public string SvtAv1AbrValueText { get; }
    public string BasicParamsText { get; }
    public string KeyframeSecondsText { get; }
    public string ThirdPartyParamsText { get; }
    public string FreeTextControlTitle { get; }
    public string PreviewFreeTextControlTitle { get; }
    public string PreviewFreeTextHint { get; }
    public string X264ModText { get; }
    public string X265JpsdrAqText { get; }
    public string X265JpsdrDarkText { get; }
    public string X265JpsdrTextureText { get; }
    public string SvtAv1EssentialDl2Text { get; }
    public string SvtAv1EssentialAutoTileText { get; }
    public string CrfHintText { get; }
    public string AbrHintText { get; }
    public string KeyframeHintText1 { get; }
    public string KeyframeHintText2 { get; }
    public string ThirdPartyHintText1 { get; }
    public string ThirdPartyHintText2 { get; }
    public string ThirdPartyHintText3 { get; }
    public string ThirdPartyHintText4 { get; }
    public string GeneralPurposeText { get; }
    public string StockFootageText { get; }
    public string FilmIRLText { get; }
    public string AnimeText { get; }
    public string StressTestText { get; }
    public string PeakQualityText { get; }
    public string CompressionOptText { get; }
    public string SpeedOptimizedText { get; }
    public string BlankPresetText { get; }
    public string BlankPresetHint { get; }
    public string VvencText { get; }
    public string VvencHintText { get; }
    public string CancelButtonText { get; }
    public string ConfirmButtonText { get; }

    public string LanguageCode { get; }
    private readonly Dictionary<string, string> _d;

    public string this[string key] => _d.TryGetValue(key, out var v) ? v : key;

    public EncoderConfLangProviderM(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];
        RateControlTitle = _d["RateControlTitle"];
        CustomParamsTitle = _d["CustomParamsTitle"];
        CrfModeText = _d["CrfModeText"];
        AbrModeText = _d["AbrModeText"];
        X264Text = _d["X264Text"];
        X265Text = _d["X265Text"];
        SvtAv1Text = _d["SvtAv1Text"];
        X264DefaultText = _d["X264DefaultText"];
        X265DefaultText = _d["X265DefaultText"];
        SvtAv1DefaultText = _d["SvtAv1DefaultText"];
        X264AbrValueText = _d["X264AbrValueText"];
        X265AbrValueText = _d["X265AbrValueText"];
        SvtAv1AbrValueText = _d["SvtAv1AbrValueText"];
        BasicParamsText = _d["BasicParamsText"];
        KeyframeSecondsText = _d["KeyframeSecondsText"];
        ThirdPartyParamsText = _d["ThirdPartyParamsText"];
        FreeTextControlTitle = _d["FreeTextControlTitle"];
        PreviewFreeTextControlTitle = _d["PreviewFreeTextControlTitle"];
        PreviewFreeTextHint = _d["PreviewFreeTextHint"];
        X264ModText = _d["X264ModText"];
        X265JpsdrAqText = _d["X265JpsdrAqText"];
        X265JpsdrDarkText = _d["X265JpsdrDarkText"];
        X265JpsdrTextureText = _d["X265JpsdrTextureText"];
        SvtAv1EssentialDl2Text = _d["SvtAv1EssentialDl2Text"];
        SvtAv1EssentialAutoTileText = _d["SvtAv1EssentialAutoTileText"];
        CrfHintText = _d["CrfHintText"];
        AbrHintText = _d["AbrHintText"];
        KeyframeHintText1 = _d["KeyframeHintText1"];
        KeyframeHintText2 = _d["KeyframeHintText2"];
        ThirdPartyHintText1 = _d["ThirdPartyHintText1"];
        ThirdPartyHintText2 = _d["ThirdPartyHintText2"];
        ThirdPartyHintText3 = _d["ThirdPartyHintText3"];
        ThirdPartyHintText4 = _d["ThirdPartyHintText4"];
        GeneralPurposeText = _d["GeneralPurposeText"];
        StockFootageText = _d["StockFootageText"];
        FilmIRLText = _d["FilmIRLText"];
        AnimeText = _d["AnimeText"];
        StressTestText = _d["StressTestText"];
        PeakQualityText = _d["PeakQualityText"];
        CompressionOptText = _d["CompressionOptText"];
        SpeedOptimizedText = _d["SpeedOptimizedText"];
        BlankPresetText = _d["BlankPresetText"];
        BlankPresetHint = _d["BlankPresetHint"];
        VvencText = _d["VvencText"];
        VvencHintText = _d["VvencHintText"];
        CancelButtonText = _d["CancelButtonText"];
        ConfirmButtonText = _d["ConfirmButtonText"];
        Current = this;
    }
}
