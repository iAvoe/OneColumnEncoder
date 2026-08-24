namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Localized strings for encoder configuration.
/// </summary>
public class EncoderConfLangProvider : LangProviderBase
{
    public static EncoderConfLangProvider Current { get; private set; } = null!;
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
            ["AutoFfprobeHintX264"] = "Auto-filled params: --rc-lookahead --colormatrix --transfer --colorprim --chromaloc --mastering-display --cll",
            ["AutoFfprobeHintX265"] = "Auto-filled params: --rc-lookahead --merange --subme --colormatrix --transfer --colorprim --range full/limited --chromaloc --master-display --max-cll",
            ["AutoFfprobeHintSvtAv1"] = "Auto-filled params: --matrix-coefficients --transfer-characteristics --color-primaries --color-range --chroma-sample-position --mastering-display --content-light",
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
            ["VvencHintText"] = "VVenC is only for single-image preview, not used in main encoding",
            ["BlankPresetHint"] = "None: use encoder default / fully customize via additional parameter textbox",
            ["DoviDesc"] = "Dolby Vision Profile 5, 7, 8 supports HEVC; P9=AVC, P10=AV1. AVC & HEVC need HRD to write SEI, but HRD relies on VBV mode, which comes down to 2 options:\n  1. Use VBV rate limit normally (use --level/--level-idc or specify manually)\n  2. Set a Tbps-ish VBV bitrate to bypass the limit, and avoid specifying the level parameter to prevent VBV override",
            ["DoviTitle"] = "Dolby Vision Compatibility",
            ["DoviOnOff"] = "When to enable",
            ["DoviSrcOnly"] = "Source is DOVI → Enable",
            ["X264HrdTitle"] = "x264 HRD (requires VBV)",
            ["X265HrdTitle"] = "x265 HRD (requires VBV)",
            ["Hrd100GbpsText"] = "100Gbps (anti-rate-limit)",
            ["Hrd1TbpsText"] = "1Tbps (8K+ anti-rate-limit)",
            ["HrdManualText"] = "Manual Specify VBV",
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
            ["AutoFfprobeHintX264"] = "自动补齐参数：--rc-lookahead --colormatrix --transfer --colorprim --chromaloc --mastering-display --cll",
            ["AutoFfprobeHintX265"] = "自动补齐参数：--rc-lookahead --merange --subme --colormatrix --transfer --colorprim --range full/limited --chromaloc --master-display --max-cll",
            ["AutoFfprobeHintSvtAv1"] = "自动补齐参数：--matrix-coefficients --transfer-characteristics --color-primaries --color-range --chroma-sample-position --mastering-display --content-light",
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
            ["VvencHintText"] = "VVenC 仅用于单帧预览，不参与主编码流程",
            ["BlankPresetHint"] = "留空：使用编码器默认或底部附加编码器参数文本框写入",
            ["DoviDesc"] = "杜比视界 Profile 5/7/8 支持 HEVC，P9=AVC，P10=AV1。AVC 与 HEVC 需开启 HRD（假想参考解码器）以写入 SEI，而 HRD 又依赖 VBV 模式。因此需要二选一：\n  1. 正常使用 VBV（通过 --level/--level-idc 级别参数或手动设定）\n  2. 指定极高 VBV 码率以绕过限码——此时不写级别参数，以避免覆盖 VBV 设定",
            ["DoviTitle"] = "杜比视界兼容性",
            ["DoviOnOff"] = "启用时机",
            ["DoviSrcOnly"] = "视频源为 DOVI → 启用",
            ["X264HrdTitle"] = "x264——开启 HRD（需 VBV 模式）",
            ["X265HrdTitle"] = "x265——开启器 HRD（需 VBV 模式）",
            ["Hrd100GbpsText"] = "100Gbps（防限码）",
            ["Hrd1TbpsText"] = "1Tbps（8K+ 防限码）",
            ["HrdManualText"] = "手动设置 VBV",
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
            ["AutoFfprobeHintX264"] = "自動補齊參數：--rc-lookahead --colormatrix --transfer --colorprim --chromaloc --mastering-display --cll",
            ["AutoFfprobeHintX265"] = "自動補齊參數：--rc-lookahead --merange --subme --colormatrix --transfer --colorprim --range full/limited --chromaloc --master-display --max-cll",
            ["AutoFfprobeHintSvtAv1"] = "自動補齊參數：--matrix-coefficients --transfer-characteristics --color-primaries --color-range --chroma-sample-position --mastering-display --content-light",
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
            ["VvencHintText"] = "VVenC 僅用於單幀預覽，不參與主編碼流程",
            ["BlankPresetHint"] = "留空：使用編碼器默認或底部附加編碼器參數文本框寫入",
            ["DoviDesc"] = "杜比視界 Profile 5/7/8 支持 HEVC，P9=AVC，P10=AV1。AVC 與 HEVC 需開啟 HRD（假想參考解碼器）以寫入 SEI，而 HRD 又依賴 VBV 模式。因此需要二選一：\n  1. 正常使用 VBV（通過 --level/--level-idc 級別參數或手動設定）\n  2. 指定極高 VBV 碼率以繞過限碼——此時不寫級別參數，以避免覆蓋 VBV 設定",
            ["DoviTitle"] = "杜比視界相容性",
            ["DoviOnOff"] = "啟用時機",
            ["DoviSrcOnly"] = "視訊源為 DOVI → 啟用",
            ["X264HrdTitle"] = "x264——開啟 HRD（需 VBV 模式）",
            ["X265HrdTitle"] = "x265——開啟 HRD（需 VBV 模式）",
            ["Hrd100GbpsText"] = "100Gbps（防限碼）",
            ["Hrd1TbpsText"] = "1Tbps（8K+ 防限碼）",
            ["HrdManualText"] = "手動設定 VBV",
        }
    };

    static EncoderConfLangProvider()
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
            ["AutoFfprobeHintX264"] = "Paramètres auto-renseignés : --rc-lookahead --colormatrix --transfer --colorprim --chromaloc --mastering-display --cll",
            ["AutoFfprobeHintX265"] = "Paramètres auto-renseignés : --rc-lookahead --merange --subme --colormatrix --transfer --colorprim --range full/limited --chromaloc --master-display --max-cll",
            ["AutoFfprobeHintSvtAv1"] = "Paramètres auto-renseignés : --matrix-coefficients --transfer-characteristics --color-primaries --color-range --chroma-sample-position --mastering-display --content-light",
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
            ["StockFootageText"] = "Images d'archives",
            ["FilmIRLText"] = "Film / Prises de vue réelles",
            ["AnimeText"] = "Animation",
            ["StressTestText"] = "Test de charge",
            ["PeakQualityText"] = "Qualité max.",
            ["CompressionOptText"] = "Compression élevée",
            ["SpeedOptimizedText"] = "Rapide",
            ["BlankPresetText"] = "Aucun",
            ["VvencHintText"] = "VVenC est réservé à l'aperçu d'une seule image, pas utilisé pour l'encodage principal",
            ["BlankPresetHint"] = "Aucun : défaut encodeur / personnalisation par paramètres additionnels",
            ["DoviDesc"] = "Dolby Vision Profile 5/7/8 prend en charge HEVC (P9=AVC, P10=AV1). AVC et HEVC nécessitent l'activation du HRD pour inclure les infos SEI, ce qui dépend du mode VBV. Deux options possibles :\n  1. Utiliser le mode VBV standard (via --level/--level-idc ou paramétrage manuel)\n  2. Définir un débit VBV extrêmement élevé (de l'ordre du Tbps) pour ignorer la limite (omettre le paramètre level pour éviter d'écraser la configuration VBV)",
            ["DoviTitle"] = "Compatibilité Dolby Vision",
            ["DoviOnOff"] = "Quand activer",
            ["DoviSrcOnly"] = "Source DOVI → Activer",
            ["X264HrdTitle"] = "HRD x264 (VBV requis)",
            ["X265HrdTitle"] = "HRD x265 (VBV requis)",
            ["Hrd100GbpsText"] = "100Gbps (anti-limite)",
            ["Hrd1TbpsText"] = "1Tbps (8K+ anti-limite)",
            ["HrdManualText"] = "Spécifier le VBV",
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
            ["AutoFfprobeHintX264"] = "Parámetros auto-completados: --rc-lookahead --colormatrix --transfer --colorprim --chromaloc --mastering-display --cll",
            ["AutoFfprobeHintX265"] = "Parámetros auto-completados: --rc-lookahead --merange --subme --colormatrix --transfer --colorprim --range full/limited --chromaloc --master-display --max-cll",
            ["AutoFfprobeHintSvtAv1"] = "Parámetros auto-completados: --matrix-coefficients --transfer-characteristics --color-primaries --color-range --chroma-sample-position --mastering-display --content-light",
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
            ["VvencHintText"] = "VVenC es solo para vista previa de imagen única, no se usa en codificación principal",
            ["BlankPresetHint"] = "Ninguno: usar defecto del codificador / personalizar abajo",
            ["DoviDesc"] = "Dolby Vision Profile 5/7/8 admite HEVC (P9=AVC, P10=AV1). AVC y HEVC requieren activar HRD para incluir información SEI, lo cual depende del modo VBV. Existen dos opciones:\n  1. Usar el modo VBV estándar (mediante --level/--level-idc o configuración manual)\n  2. Configurar una tasa VBV extremadamente alta (del orden de Tbps) para omitir el límite (evitar el parámetro level para no sobrescribir VBV)",
            ["DoviTitle"] = "Compatibilidad Dolby Vision",
            ["DoviOnOff"] = "Cuándo activar",
            ["DoviSrcOnly"] = "Fuente DOVI → Activar",
            ["X264HrdTitle"] = "HRD x264 (requiere VBV)",
            ["X265HrdTitle"] = "HRD x265 (requiere VBV)",
            ["Hrd100GbpsText"] = "100Gbps (anti-límite)",
            ["Hrd1TbpsText"] = "1Tbps (8K+ anti-límite)",
            ["HrdManualText"] = "Especificar VBV",
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
            ["AutoFfprobeHintX264"] = "自動入力されたパラメータ: --rc-lookahead --colormatrix --transfer --colorprim --chromaloc --mastering-display --cll",
            ["AutoFfprobeHintX265"] = "自動入力されたパラメータ: --rc-lookahead --merange --subme --colormatrix --transfer --colorprim --range full/limited --chromaloc --master-display --max-cll",
            ["AutoFfprobeHintSvtAv1"] = "自動入力されたパラメータ: --matrix-coefficients --transfer-characteristics --color-primaries --color-range --chroma-sample-position --mastering-display --content-light",
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
            ["VvencHintText"] = "VVenC は単一画像プレビューのみで使用、本エンコードでは使用しません",
            ["BlankPresetHint"] = "なし: エンコーダ既定値 / 下の追加パラメータで完全指定",
            ["DoviDesc"] = "Dolby Vision Profile 5/7/8はHEVCに対応（P9=AVC、P10=AV1）。AVC/HEVCでSEIを書き込むにはHRDの有効化が必要で、HRDはVBVモードに依存します。対応策は以下の2択です：\n  1. 通常のVBV制限を使用（--level/--level-idc または手動指定）\n  2. 制限回避のため極めて高いVBVレート（Tbps級）を指定（VBV上書き防止のため level パラメーターは未指定）",
            ["DoviTitle"] = "Dolby Vision互換性",
            ["DoviOnOff"] = "有効にする条件",
            ["DoviSrcOnly"] = "ソースがDOVI → 有効",
            ["X264HrdTitle"] = "x264 HRD（VBV必須）",
            ["X265HrdTitle"] = "x265 HRD（VBV必須）",
            ["Hrd100GbpsText"] = "100Gbps（制限回避）",
            ["Hrd1TbpsText"] = "1Tbps（8K+制限回避）",
            ["HrdManualText"] = "VBVを手動指定",
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
            ["AutoFfprobeHintX264"] = "Автозаполняемые параметры: --rc-lookahead --colormatrix --transfer --colorprim --chromaloc --mastering-display --cll",
            ["AutoFfprobeHintX265"] = "Автозаполняемые параметры: --rc-lookahead --merange --subme --colormatrix --transfer --colorprim --range full/limited --chromaloc --master-display --max-cll",
            ["AutoFfprobeHintSvtAv1"] = "Автозаполняемые параметры: --matrix-coefficients --transfer-characteristics --color-primaries --color-range --chroma-sample-position --mastering-display --content-light",
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
            ["VvencHintText"] = "VVenC используется только для предпросмотра одного кадра, не для основного кодирования",
            ["BlankPresetHint"] = "Нет: настройки кодера по умолчанию / полная настройка ниже",
            ["DoviDesc"] = "Dolby Vision Profile 5/7/8 поддерживает HEVC (P9=AVC, P10=AV1). Для AVC и HEVC требуется включение HRD для записи SEI, что зависит от режима VBV. Доступно два варианта:\n  1. Использовать VBV в обычном режиме (через --level/--level-idc или вручную)\n  2. Указать сверхвысокий битрейт VBV (порядка Тбит/с) для обхода ограничения (не указывать параметр level, чтобы не перезаписать VBV)",
            ["DoviTitle"] = "Совместимость с Dolby Vision",
            ["DoviOnOff"] = "Когда включать",
            ["DoviSrcOnly"] = "Источник DOVI → Включить",
            ["X264HrdTitle"] = "HRD x264 (требуется VBV)",
            ["X265HrdTitle"] = "HRD x265 (требуется VBV)",
            ["Hrd100GbpsText"] = "100 Гбит/с (без ограничения)",
            ["Hrd1TbpsText"] = "1 Тбит/с (8K+ без ограничения)",
            ["HrdManualText"] = "Указать VBV вручную",
        };
        Data["de"] = new(Data["en"])
        {
            ["RateControlTitle"] = "Ratensteuerung",
            ["CustomParamsTitle"] = "Benutzerparameter",
            ["CrfModeText"] = "CRF-Modus (konstanter Qualitätsfaktor)",
            ["AbrModeText"] = "ABR-Modus (durchschnittliche Bitrate)",
            ["BasicParamsText"] = "Basisparameter",
            ["KeyframeSecondsText"] = "Keyframe-Intervall (Sekunden)",
            ["ThirdPartyParamsText"] = "Drittanbieter-Erweiterungsparameter",
            ["FreeTextControlTitle"] = "Zusätzliche Encoder-Parameter (CLI-Format)",
            ["PreviewFreeTextControlTitle"] = "Zusätzliche Encoder-Parameter (Vorschau/FFmpeg-Format)",
            ["PreviewFreeTextHint"] = "FFmpeg-API-Format verwenden: -x264-params, -x265-params, -svtav1-params",
            ["AutoFfprobeHintX264"] = "Automatisch ausgefüllte Parameter: --rc-lookahead --colormatrix --transfer --colorprim --chromaloc --mastering-display --cll",
            ["AutoFfprobeHintX265"] = "Automatisch ausgefüllte Parameter: --rc-lookahead --merange --subme --colormatrix --transfer --colorprim --range full/limited --chromaloc --master-display --max-cll",
            ["AutoFfprobeHintSvtAv1"] = "Automatisch ausgefüllte Parameter: --matrix-coefficients --transfer-characteristics --color-primaries --color-range --chroma-sample-position --mastering-display --content-light",
            ["X264ModText"] = "x264 Mod: Filmkorn-RD-Optimierung (FGO-RD)",
            ["X265JpsdrAqText"] = "x265 jpsdr Mod: AQ-Hysterese",
            ["X265JpsdrDarkText"] = "x265 jpsdr Mod: ↑AQ-Stärke für dunkle Szenen",
            ["X265JpsdrTextureText"] = "x265 jpsdr Mod: ↑AQ-Stärke für Kanten",
            ["SvtAv1EssentialDl2Text"] = "SVT-AV1-Essential: Präzises Deblocking (DLF2)",
            ["SvtAv1EssentialAutoTileText"] = "SVT-AV1-Essential: Automatische Tile-Größe",
            ["CrfHintText"] = "CRF-Skala — Verlustfrei | UHQ | HQ | Streaming",
            ["AbrHintText"] = "ABR-Skala (2K60) — Fast verlustfrei | Fast verlustfrei→HQ | UHQ→Streaming | HQ→Streaming",
            ["KeyframeHintText1"] = "Keyint-Skala (Dekodierschwierigkeit):\nEco./Multi-Track-Bearbeitung | Mittel | Schwer & mittlere Kompression | Extrem & hohe Kompression",
            ["KeyframeHintText2"] = "↑Auflösung=↑Dekodierschwierigkeit; Keyint-Änderung ↓Dekodier-Index-Schwierigkeit oder ↑Kompression",
            ["ThirdPartyHintText1"] = "\"Filmkorn-Optimierung\" begünstigt Schärfe, nicht Kompression",
            ["ThirdPartyHintText2"] = "\"AQ-Hysterese\" verhindert, dass Schnitte/Montagen AQ-Strategie stören",
            ["ThirdPartyHintText3"] = "\"Präzises Deblocking\" kann Unschärfe reduzieren",
            ["ThirdPartyHintText4"] = "\"Auto Tile\" kann Bitverteilung besser ausbalancieren",
            ["GeneralPurposeText"] = "Allgemein",
            ["StockFootageText"] = "Archivmaterial",
            ["FilmIRLText"] = "Film / Reale Aufnahmen",
            ["AnimeText"] = "Anime",
            ["StressTestText"] = "Stresstest",
            ["PeakQualityText"] = "Maximale Qualität",
            ["CompressionOptText"] = "Hohe Kompression",
            ["SpeedOptimizedText"] = "Schnell",
            ["BlankPresetText"] = "Keine",
            ["VvencHintText"] = "VVenC ist nur für Einzelbildvorschau, nicht für Hauptkodierung",
            ["BlankPresetHint"] = "Keine: Encoder-Standard / vollständige Anpassung über Zusatzparameter",
            ["DoviDesc"] = "Dolby Vision Profile 5/7/8 unterstützt HEVC (P9=AVC, P10=AV1). AVC und HEVC erfordern die Aktivierung von HRD für SEI-Daten, was vom VBV-Modus abhängt. Es gibt zwei Optionen:\n  1. VBV normal nutzen (über --level/--level-idc oder manuelle Einstellung)\n  2. Extrem hohe VBV-Bitrate (im Tbps-Bereich) festlegen, um das Limit zu umgehen (keinen level-Parameter angeben, um ein Überschreiben von VBV zu vermeiden)",
            ["DoviTitle"] = "Dolby-Vision-Kompatibilität",
            ["DoviOnOff"] = "Wann aktivieren",
            ["DoviSrcOnly"] = "Quelle ist DOVI → Aktivieren",
            ["X264HrdTitle"] = "x264 HRD (VBV erforderlich)",
            ["X265HrdTitle"] = "x265 HRD (VBV erforderlich)",
            ["Hrd100GbpsText"] = "100 Gbit/s (Limitierung vermeiden)",
            ["Hrd1TbpsText"] = "1 Tbit/s (8K+ Limitierung vermeiden)",
            ["HrdManualText"] = "VBV manuell festlegen",
        };
        Data["ko"] = new(Data["en"])
        {
            ["RateControlTitle"] = "비트레이트 제어 방식",
            ["CustomParamsTitle"] = "사용자 정의 파라미터",
            ["CrfModeText"] = "고정 비트레이트 팩터(CRF) 모드",
            ["AbrModeText"] = "평균 비트레이트(ABR) 모드",
            ["X264Text"] = "x264",
            ["X265Text"] = "x265",
            ["SvtAv1Text"] = "SVT-AV1",
            ["X264DefaultText"] = "(기본값 23)",
            ["X265DefaultText"] = "(기본값 28)",
            ["SvtAv1DefaultText"] = "(기본값 35)",
            ["X264AbrValueText"] = "209",
            ["X265AbrValueText"] = "200",
            ["SvtAv1AbrValueText"] = "200",
            ["BasicParamsText"] = "기본 파라미터",
            ["KeyframeSecondsText"] = "키프레임 간격(초)",
            ["ThirdPartyParamsText"] = "서드파티 확장 파라미터",
            ["FreeTextControlTitle"] = "추가 인코더 파라미터(CLI 형식)",
            ["PreviewFreeTextControlTitle"] = "추가 인코더 파라미터(미리보기/FFmpeg 형식)",
            ["PreviewFreeTextHint"] = "FFmpeg API 형식 사용: -x264-params, -x265-params, -svtav1-params",
            ["AutoFfprobeHintX264"] = "자동 입력 파라미터: --rc-lookahead --colormatrix --transfer --colorprim --chromaloc --mastering-display --cll",
            ["AutoFfprobeHintX265"] = "자동 입력 파라미터: --rc-lookahead --merange --subme --colormatrix --transfer --colorprim --range full/limited --chromaloc --master-display --max-cll",
            ["AutoFfprobeHintSvtAv1"] = "자동 입력 파라미터: --matrix-coefficients --transfer-characteristics --color-primaries --color-range --chroma-sample-position --mastering-display --content-light",
            ["X264ModText"] = "x264 Mod: 필름 그레인 비율 왜곡 최적화(FGO-RD)",
            ["X265JpsdrAqText"] = "x265 jpsdr Mod: 적응형 양자화 히스테리시스(AQ Hysteresis)",
            ["X265JpsdrDarkText"] = "x265 jpsdr Mod: 어두운 장면의 AQ 강도 ↑",
            ["X265JpsdrTextureText"] = "x265 jpsdr Mod: 엣지의 AQ 강도 ↑",
            ["SvtAv1EssentialDl2Text"] = "SVT-AV1-Essential: 고정밀 디블로킹 필터(DLF2)",
            ["SvtAv1EssentialAutoTileText"] = "SVT-AV1-Essential: 자동 타일 크기",
            ["CrfHintText"] = "CRF 눈금 — 무손실 | 초고화질 | 고화질 | 스트리밍",
            ["AbrHintText"] = "ABR 눈금(2K60) — 근무손실 | 근무손실→고화질 | 초고화질→스트리밍 | 고화질→스트리밍",
            ["KeyframeHintText1"] = "Keyint 눈금(디코딩 난이도):\n절전/멀티트랙 편집 | 중간 | 높은 부하·중간 압축 | 극도 높은 부하·높은 압축",
            ["KeyframeHintText2"] = "해상도↑ = 디코딩/인덱싱 부하↑. 키프레임 간격 조절로 부하를 낮추거나 압축률을 높일 수 있음",
            ["ThirdPartyHintText1"] = "「그레인 최적화」는 압축보다 선명도를 우선함",
            ["ThirdPartyHintText2"] = "「AQ 히스테리시스」는 빠른 컷이나 몽타주가 현재 AQ 전략을 흐트리는 것을 방지",
            ["ThirdPartyHintText3"] = "「고정밀 디블로킹」은 뭉개짐을 줄일 수 있음",
            ["ThirdPartyHintText4"] = "「자동 타일」은 비트 분배의 균형을 개선할 수 있음",
            ["GeneralPurposeText"] = "범용",
            ["StockFootageText"] = "스톡 영상",
            ["FilmIRLText"] = "영화/실사 촬영",
            ["AnimeText"] = "애니메이션",
            ["StressTestText"] = "스트레스 테스트",
            ["PeakQualityText"] = "최고 품질",
            ["CompressionOptText"] = "고압축",
            ["SpeedOptimizedText"] = "고속",
            ["BlankPresetText"] = "없음",
            ["VvencHintText"] = "VVenC는 단일 이미지 미리보기에만 사용되며, 본 인코딩에는 사용하지 않음",
            ["BlankPresetHint"] = "없음: 인코더 기본값 사용 / 하단 추가 파라미터 텍스트상자로 완전 지정",
            ["DoviDesc"] = "Dolby Vision Profile 5/7/8은 HEVC를 지원합니다(P9=AVC, P10=AV1). AVC/HEVC는 SEI 기록을 위해 HRD 활성화가 필요하며, HRD는 VBV 모드에 의존합니다. 선택지는 두 가지입니다:\n  1. 일반 VBV 제한 사용 (--level/--level-idc 또는 수동 설정)\n  2. 제한 우회를 위해 초고속 VBV 비트레이트(Tbps급) 지정 (VBV 덮어쓰기 방지를 위해 level 파라미터 미작성)",
            ["DoviTitle"] = "Dolby Vision 호환성",
            ["DoviOnOff"] = "활성화 조건",
            ["DoviSrcOnly"] = "소스가 DOVI → 활성화",
            ["X264HrdTitle"] = "x264 HRD(VBV 필수)",
            ["X265HrdTitle"] = "x265 HRD(VBV 필수)",
            ["Hrd100GbpsText"] = "100Gbps(비트레이트 제한 우회)",
            ["Hrd1TbpsText"] = "1Tbps(8K+ 비트레이트 제한 우회)",
            ["HrdManualText"] = "수동 VBV 지정",
        };
        Data["pt-br"] = new(Data["en"])
        {
            ["RateControlTitle"] = "Mecanismo de controle de taxa",
            ["CustomParamsTitle"] = "Parâmetros personalizados",
            ["CrfModeText"] = "Modo CRF (fator de taxa constante)",
            ["AbrModeText"] = "Modo ABR (taxa de bits média)",
            ["BasicParamsText"] = "Parâmetros básicos",
            ["KeyframeSecondsText"] = "Intervalo de quadros-chave (segundos)",
            ["ThirdPartyParamsText"] = "Parâmetros estendidos de terceiros",
            ["FreeTextControlTitle"] = "Parâmetros adicionais do codificador (formato CLI)",
            ["PreviewFreeTextControlTitle"] = "Parâmetros adicionais do codificador (formato Visualização/FFmpeg)",
            ["PreviewFreeTextHint"] = "Use o formato da API FFmpeg: -x264-params, -x265-params, -svtav1-params",
            ["AutoFfprobeHintX264"] = "Parâmetros auto-preenchidos: --rc-lookahead --colormatrix --transfer --colorprim --chromaloc --mastering-display --cll",
            ["AutoFfprobeHintX265"] = "Parâmetros auto-preenchidos: --rc-lookahead --merange --subme --colormatrix --transfer --colorprim --range full/limited --chromaloc --master-display --max-cll",
            ["AutoFfprobeHintSvtAv1"] = "Parâmetros auto-preenchidos: --matrix-coefficients --transfer-characteristics --color-primaries --color-range --chroma-sample-position --mastering-display --content-light",
            ["X264ModText"] = "x264 Mod: Otimização R-D de granulação de filme (FGO-RD)",
            ["X265JpsdrAqText"] = "x265 jpsdr Mod: Histerese de quantização adaptativa (AQ)",
            ["X265JpsdrDarkText"] = "x265 jpsdr Mod: ↑Força AQ para cenas escuras",
            ["X265JpsdrTextureText"] = "x265 jpsdr Mod: ↑Força AQ para texturas",
            ["SvtAv1EssentialDl2Text"] = "SVT-AV1-Essential: Filtro deblocking preciso (DLF2)",
            ["SvtAv1EssentialAutoTileText"] = "SVT-AV1-Essential: Tamanho de tile automático",
            ["CrfHintText"] = "Marcas do CRF — Sem perda | UHQ | HQ | Streaming",
            ["AbrHintText"] = "Marcas do ABR (2K60) — Quase sem perda | Quase sem perda→HQ | UHQ→Streaming | HQ→Streaming",
            ["KeyframeHintText1"] = "Marcas do Keyint (dificuldade de decodificação):\nEco./Edição multi-track | Médio | Difícil e compressão média | Extrema e alta compressão",
            ["KeyframeHintText2"] = "↑Resolução=↑dificuldade de decodificação; alterar dist. keyint ajuda ↓dificuldade de decodificação-indexação ou ↑compressão",
            ["ThirdPartyHintText1"] = "\"Otimização de granulação\" tende fortemente à nitidez, não à compressão",
            ["ThirdPartyHintText2"] = "\"Histerese AQ\" evita que flashcuts/montagens perturbem a estratégia AQ atual",
            ["ThirdPartyHintText3"] = "\"Deblocking de alta precisão\" pode reduzir a desfocagem",
            ["ThirdPartyHintText4"] = "\"Auto Tile\" pode equilibrar melhor a distribuição da taxa de bits",
            ["GeneralPurposeText"] = "Propósito geral",
            ["StockFootageText"] = "Material de arquivo",
            ["FilmIRLText"] = "Filmagem / Cena real",
            ["AnimeText"] = "Anime",
            ["StressTestText"] = "Teste de estresse",
            ["PeakQualityText"] = "Qualidade máxima",
            ["CompressionOptText"] = "Alta compressão",
            ["SpeedOptimizedText"] = "Rápido",
            ["BlankPresetText"] = "Nenhum",
            ["VvencHintText"] = "VVenC é apenas para visualização de imagem única, não usado na codificação principal",
            ["BlankPresetHint"] = "Nenhum: usar padrão do codificar / personalizar totalmente via caixa de texto de parâmetros adicionais",
            ["DoviDesc"] = "Dolby Vision Profile 5/7/8 suporta HEVC (P9=AVC, P10=AV1). AVC e HEVC exigem ativar HRD para gravar dados SEI, o que depende do modo VBV. Há duas opções:\n  1. Usar VBV normalmente (via --level/--level-idc ou configuração manual)\n  2. Definir uma taxa VBV extremamente alta (na casa dos Tbps) para ignorar o limite (omitir o parâmetro level para não sobrescrever o VBV)",
            ["DoviTitle"] = "Compatibilidade com Dolby Vision",
            ["DoviOnOff"] = "Quando ativar",
            ["DoviSrcOnly"] = "Fonte é DOVI → Ativar",
            ["X264HrdTitle"] = "HRD x264 (requer VBV)",
            ["X265HrdTitle"] = "HRD x265 (requer VBV)",
            ["Hrd100GbpsText"] = "100Gbps (anti-limite)",
            ["Hrd1TbpsText"] = "1Tbps (8K+ anti-limite)",
            ["HrdManualText"] = "Especificar VBV manualmente",
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
    public string AutoFfprobeHintX264 { get; }
    public string AutoFfprobeHintX265 { get; }
    public string AutoFfprobeHintSvtAv1 { get; }
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
    public string DoviTitle { get; }
    public string DoviDesc { get; }
    public string DoviOnOff { get; }
    public string DoviSrcOnly { get; }
    public string X264HrdTitle { get; }
    public string X265HrdTitle { get; }
    public string Hrd100GbpsText { get; }
    public string Hrd1TbpsText { get; }
    public string HrdManualText { get; }
    public string CancelButtonText { get; }
    public string ConfirmButtonText { get; }

    public EncoderConfLangProvider(string languageCode) : base(languageCode, Data)
    {
        RateControlTitle = this["RateControlTitle"];
        CustomParamsTitle = this["CustomParamsTitle"];
        CrfModeText = this["CrfModeText"];
        AbrModeText = this["AbrModeText"];
        X264Text = this["X264Text"];
        X265Text = this["X265Text"];
        SvtAv1Text = this["SvtAv1Text"];
        X264DefaultText = this["X264DefaultText"];
        X265DefaultText = this["X265DefaultText"];
        SvtAv1DefaultText = this["SvtAv1DefaultText"];
        X264AbrValueText = this["X264AbrValueText"];
        X265AbrValueText = this["X265AbrValueText"];
        SvtAv1AbrValueText = this["SvtAv1AbrValueText"];
        BasicParamsText = this["BasicParamsText"];
        KeyframeSecondsText = this["KeyframeSecondsText"];
        ThirdPartyParamsText = this["ThirdPartyParamsText"];
        FreeTextControlTitle = this["FreeTextControlTitle"];
        PreviewFreeTextControlTitle = this["PreviewFreeTextControlTitle"];
        PreviewFreeTextHint = this["PreviewFreeTextHint"];
        AutoFfprobeHintX264 = this["AutoFfprobeHintX264"];
        AutoFfprobeHintX265 = this["AutoFfprobeHintX265"];
        AutoFfprobeHintSvtAv1 = this["AutoFfprobeHintSvtAv1"];
        X264ModText = this["X264ModText"];
        X265JpsdrAqText = this["X265JpsdrAqText"];
        X265JpsdrDarkText = this["X265JpsdrDarkText"];
        X265JpsdrTextureText = this["X265JpsdrTextureText"];
        SvtAv1EssentialDl2Text = this["SvtAv1EssentialDl2Text"];
        SvtAv1EssentialAutoTileText = this["SvtAv1EssentialAutoTileText"];
        CrfHintText = this["CrfHintText"];
        AbrHintText = this["AbrHintText"];
        KeyframeHintText1 = this["KeyframeHintText1"];
        KeyframeHintText2 = this["KeyframeHintText2"];
        ThirdPartyHintText1 = this["ThirdPartyHintText1"];
        ThirdPartyHintText2 = this["ThirdPartyHintText2"];
        ThirdPartyHintText3 = this["ThirdPartyHintText3"];
        ThirdPartyHintText4 = this["ThirdPartyHintText4"];
        GeneralPurposeText = this["GeneralPurposeText"];
        StockFootageText = this["StockFootageText"];
        FilmIRLText = this["FilmIRLText"];
        AnimeText = this["AnimeText"];
        StressTestText = this["StressTestText"];
        PeakQualityText = this["PeakQualityText"];
        CompressionOptText = this["CompressionOptText"];
        SpeedOptimizedText = this["SpeedOptimizedText"];
        BlankPresetText = this["BlankPresetText"];
        BlankPresetHint = this["BlankPresetHint"];
        VvencText = "VVenC (PV)";
        VvencHintText = this["VvencHintText"];
        DoviTitle = this["DoviTitle"];
        DoviDesc = this["DoviDesc"];
        DoviOnOff = this["DoviOnOff"];
        DoviSrcOnly = this["DoviSrcOnly"];
        X264HrdTitle = this["X264HrdTitle"];
        X265HrdTitle = this["X265HrdTitle"];
        Hrd100GbpsText = this["Hrd100GbpsText"];
        Hrd1TbpsText = this["Hrd1TbpsText"];
        HrdManualText = this["HrdManualText"];
        CancelButtonText = this["CancelButtonText"];
        ConfirmButtonText = this["ConfirmButtonText"];
        Current = this;
    }
}
