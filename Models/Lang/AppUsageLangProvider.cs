namespace OneColumnEncoder.Models.Lang;

public class AppUsageLangProvider
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["HowToUse"] = "How to use this program",
            ["UpdateTitle"] = "How to Update This Application",
            ["UpdateDesc"] = "Keep the 1cenc folder and replace OneColumnEncoder.exe. All your configurations and data will be preserved.",
            ["Description"] = "This program stictly follows a top\u2192down, left\u2192right operation sequence——all 'next' buttons are on the right side.",
            ["CopyHint"] = "Tip: This window supports text selection and Ctrl+C to copy texts",
            ["GettingStarted"] = "The simplest way to get started is to:",
            ["Step1"] = "1. Import & select an upstream tool (ffmpeg, vspipe, avs2yuv, etc.)",
            ["Step2"] = "2. Import & select an encoder / downstream tool (x264, x265, etc.)",
            ["Step3"] = "3. Import source video file",
            ["Step4"] = "4. Select encoding settings (that is validated as compatible & healthy)",
            ["Step5"] = "5. Clear the checklist and start",
            ["WhyDisabledTitle"] = "Why is my Start Encoding button disabled",
            ["WhyDisabled1"] = "1. Click Bypass button below a checklist that has detected error",
            ["WhyDisabled2"] = "2. Garbage in, garbage out (most of them are not disabling though)",
            ["WhyDisabled3"] = "3. Encoding of corrupted video can crash your PC (BSOD) in rare cases",
            ["ToolDownloadTitle"] = "Download Video Encoding Related Tools",
            ["ComplianceTitle"] = "Commercial Usage Compliance",
            ["ComplianceDesc"] = "This program is licensed under the Apache License 2.0. For commercial usage, please refer to compliance requirement of the programs imported to this tool.",
            ["LicenseFfmpeg"] = "\u00B7 FFmpeg / FFprobe Legal & License: https://ffmpeg.org/legal.html",
            ["LicenseVapourSynth"] = "\u00B7 VapourSynth License: https://github.com/vapoursynth/vapoursynth/blob/master/COPYING",
            ["LicenseAvs2yuv"] = "\u00B7 Avs2YUV License: https://github.com/FFMS/ffms2/blob/master/COPYING.GPLv3",
            ["LicenseAvs2pipemod"] = "\u00B7 Avs2Pipemod License: https://github.com/pinterf/AvsPmod",
            ["LicenseSvfi"] = "\u00B7 SVFI License: https://github.com/Justin62628/Squirrel-RIFE/blob/master/LICENSE",
            ["LicenseX264"] = "\u00B7 x264 License & AVC Patent Info: https://x264.org/licensing/",
            ["LicenseX265"] = "\u00B7 x265 License & HEVC Patent Info: https://www.videolan.org/developers/x265.html",
            ["LicenseSvtAv1"] = "\u00B7 SVT-AV1 / AV1 License Info: https://gitlab.com/AOMediaCodec/SVT-AV1/-/blob/master/LICENSE.md",
            ["ComplianceFooter"] = "...Including the video container formats, audio codecs, and most importantly, the font types involved for commercial usage.",
            ["ComplianceDisclaimer"] = "Users are responsible for ensuring compliance with software licenses, codec patents, media formats, and font licenses in their region.",
            ["ParamConfigTitle"] = "Parameter Configuration Feature Description",
            ["ParamConfigIntro"] = "This program uses an align-with-source strategy for parameter configuration, while attempts to leave enough room for the encoder to perform.",
            ["AutoParamTitle"] = "Auto-specified or Y4M-provided Parameters",
            ["AutoParamBase"] = "· Basic parameters: frame rate, resolution, total frames, color matrix, transfer characteristics, color primaries",
            ["AutoParamEncode"] = "· Encoding parameters: lookahead distance, motion estimation radius, subpixel motion estimation strength",
            ["AutoParamFooter"] = "...excluding encoders that can auto-configure the above parameters",
            ["ManualBaseTitle"] = "Manually Specified - Basic Parameters",
            ["ManualBaseList"] = "· x264: general-purpose, stock footage\n· x265: general-purpose, film, stock footage, anime, stress test\n· SVT-AV1: ultra HQ, high compression, fast",
            ["ManualBaseFooter"] = "...determined by current CPU perf. & settings exposed to commandline (dev's intent), affecting the precision of bit allocation (% of bits to be losslessly compressed)",
            ["ManualExtTitle"] = "Manually Specified - 3rd-party Extended Parameters",
            ["ManualExtIntro"] = "Modded video encoders may provide extended features (unlike official / other modified versions), check their existence before checking the checkboxes.",
            ["CloseButtonText"] = "Close"
        },
        ["zh-cn"] = new()
        {
            ["HowToUse"] = "如何使用本程序",
            ["UpdateTitle"] = "如何更新本应用",
            ["UpdateDesc"] = "保留 1cenc 文件夹，替换 OneColumnEncoder.exe 即可。所有配置和数据将保留。",
            ["Description"] = "本程序严格遵循自上而下、从左到右的操作顺序——确认按钮皆位于右侧。",
            ["CopyHint"] = "提示：本窗口支持拖选与 Ctrl+C 复制",
            ["GettingStarted"] = "开始使用的最简单方法是：",
            ["Step1"] = "1. 导入并点选上游工具（ffmpeg、vspipe、avs2yuv 等）",
            ["Step2"] = "2. 导入并点选编码器/下游工具（x264、x265 等）",
            ["Step3"] = "3. 导入源视频文件",
            ["Step4"] = "4. 选择编码设置（将验证兼容性与健康状态）",
            ["Step5"] = "5. 清除检查清单并开始",
            ["WhyDisabledTitle"] = "为什么「开始编码」按钮不可用",
            ["WhyDisabled1"] = "1. 在发现问题的检查栏下方点击绕过",
            ["WhyDisabled2"] = "2. 视频源—朽木不可雕也（但大多数情况并不会禁用）",
            ["WhyDisabled3"] = "3. 编码损坏的视频在极少数情况下可能导致电脑崩溃（蓝屏）",
            ["ToolDownloadTitle"] = "视频压制相关工具下载",
            ["ComplianceTitle"] = "商业使用合规要求",
            ["ComplianceDesc"] = "本程序使用 Apache 2.0 许可证。对于商业用途，请参考导入本工具的程序的相关合规要求。",
            ["LicenseFfmpeg"] = "\u00B7 FFmpeg / FFprobe 法律与许可证：https://ffmpeg.org/legal.html",
            ["LicenseVapourSynth"] = "\u00B7 VapourSynth 许可证：https://github.com/vapoursynth/vapoursynth/blob/master/COPYING",
            ["LicenseAvs2yuv"] = "\u00B7 Avs2YUV 许可证：https://github.com/FFMS/ffms2/blob/master/COPYING.GPLv3",
            ["LicenseAvs2pipemod"] = "\u00B7 Avs2Pipemod 许可证：https://github.com/pinterf/AvsPmod",
            ["LicenseSvfi"] = "\u00B7 SVFI 许可证：https://github.com/Justin62628/Squirrel-RIFE/blob/master/LICENSE",
            ["LicenseX264"] = "\u00B7 x264 许可证与 AVC 专利信息：https://x264.org/licensing/",
            ["LicenseX265"] = "\u00B7 x265 许可证与 HEVC 专利信息：https://www.videolan.org/developers/x265.html",
            ["LicenseSvtAv1"] = "\u00B7 SVT-AV1 / AV1 许可证信息：https://gitlab.com/AOMediaCodec/SVT-AV1/-/blob/master/LICENSE.md",
            ["ComplianceFooter"] = "……包括视频容器格式、音频编码器，以及商业用途中涉及的字体的合规要求。",
            ["ComplianceDisclaimer"] = "用户有责任确保其所在地区的软件许可证、编解码器专利、媒体格式和字体许可证的合规性。",
            ["ParamConfigTitle"] = "参数配置功能说明",
            ["ParamConfigIntro"] = "本程序配置参数的策略为保持与源一致，并给予视频编码器充足的发挥空间。",
            ["AutoParamTitle"] = "自动指定或通过 Y4M 管道配置的编码参数",
            ["AutoParamBase"] = "· 基础参数：帧率、分辨率、总帧数、矩阵格式、传输特质、原色色系",
            ["AutoParamEncode"] = "· 编码参数：前瞻帧数、动态搜索直径、子像素对齐搜索强度",
            ["AutoParamFooter"] = "...除能够自主配置上述参数的编码器外",
            ["ManualBaseTitle"] = "手动指定—基础参数",
            ["ManualBaseList"] = "· x264：通用、剪辑素材\n· x265：通用、录像、剪辑素材、动漫、压力测试\n· SVT-AV1：极致画质、压缩优先、速度优先",
            ["ManualBaseFooter"] = "「手动指定」参数由当下的处理器性能与下放到命令行的参数（即开发者意图）而定，这些参数会影响码率分配的精度，即无损压缩机制的压缩率。",
            ["ManualExtTitle"] = "手动指定—第三方扩展参数",
            ["ManualExtIntro"] = "非官方版的视频编码器提供了扩展功能（官方和其它修改版并不支持），勾选使用前应检查参数是否存在。",
            ["CloseButtonText"] = "关闭"
        },
        ["zh-tw"] = new()
        {
            ["HowToUse"] = "如何使用本程式",
            ["UpdateTitle"] = "如何更新本應用",
            ["UpdateDesc"] = "保留 1cenc 資料夾，替換 OneColumnEncoder.exe 即可。所有配置和資料將保留。",
            ["Description"] = "本程式嚴格遵循自上而下、由左至右的操作順序—確認按鈕皆位於右側。",
            ["CopyHint"] = "提示：本視窗支援拖選與 Ctrl+C 複製",
            ["GettingStarted"] = "開始使用的最簡單方法是：",
            ["Step1"] = "1. 匯入並點選上游工具（ffmpeg、vspipe、avs2yuv 等）",
            ["Step2"] = "2. 匯入並點選編碼器/下游工具（x264、x265 等）",
            ["Step3"] = "3. 匯入來源影片檔案",
            ["Step4"] = "4. 選擇編碼設定（將驗證相容性與健康狀態）",
            ["Step5"] = "5. 清除檢查清單並開始",
            ["WhyDisabledTitle"] = "為什麼「開始編碼」按鈕不可用",
            ["WhyDisabled1"] = "1. 在發現問題的檢查欄下方點擊繞過",
            ["WhyDisabled2"] = "2. 影片源—朽木不可雕也（但大多數情況並不會禁用）",
            ["WhyDisabled3"] = "3. 編碼損壞的影片在極少數情況下可能導致電腦當機（藍屏）",
            ["ToolDownloadTitle"] = "視訊壓制相關工具下載",
            ["ComplianceTitle"] = "商業使用合規要求",
            ["ComplianceDesc"] = "本程式使用 Apache 2.0 授權。對於商業用途，請參考導入本程式的程式的相關合規要求。",
            ["LicenseFfmpeg"] = "\u00B7 FFmpeg / FFprobe 法律與授權：https://ffmpeg.org/legal.html",
            ["LicenseVapourSynth"] = "\u00B7 VapourSynth 授權：https://github.com/vapoursynth/vapoursynth/blob/master/COPYING",
            ["LicenseAvs2yuv"] = "\u00B7 Avs2YUV 授權：https://github.com/FFMS/ffms2/blob/master/COPYING.GPLv3",
            ["LicenseAvs2pipemod"] = "\u00B7 Avs2Pipemod 授權：https://github.com/pinterf/AvsPmod",
            ["LicenseSvfi"] = "\u00B7 SVFI 授權：https://github.com/Justin62628/Squirrel-RIFE/blob/master/LICENSE",
            ["LicenseX264"] = "\u00B7 x264 授權與 AVC 專利資訊：https://x264.org/licensing/",
            ["LicenseX265"] = "\u00B7 x265 授權與 HEVC 專利資訊：https://www.videolan.org/developers/x265.html",
            ["LicenseSvtAv1"] = "\u00B7 SVT-AV1 / AV1 授權資訊：https://gitlab.com/AOMediaCodec/SVT-AV1/-/blob/master/LICENSE.md",
            ["ComplianceFooter"] = "……包括影片容器格式、音訊編碼器，以及商業用途中涉及的字型的合規要求。",
            ["ComplianceDisclaimer"] = "使用者有責任確保其所在地區的軟體授權、編解碼器專利、媒體格式和字型授權的合規性。",
            ["ParamConfigTitle"] = "參數配置功能說明",
            ["ParamConfigIntro"] = "本程式配置參數的策略為保持與源一致，並給予視訊編碼器充足的發揮空間。",
            ["AutoParamTitle"] = "自動指定或通過 Y4M 管道配置的編碼參數",
            ["AutoParamBase"] = "· 基礎參數：幀率、解析度、總幀數、矩陣格式、傳輸特質、原色色系",
            ["AutoParamEncode"] = "· 編碼參數：前瞻幀數、動態搜尋直徑、子像素對齊搜尋強度",
            ["AutoParamFooter"] = "...除能夠自主配置上述參數的編碼器外",
            ["ManualBaseTitle"] = "手動指定—基礎參數",
            ["ManualBaseList"] = "· x264：通用、剪輯素材\n· x265：通用、錄像、剪輯素材、動漫、壓力測試\n· SVT-AV1：極致畫質、壓縮優先、速度優先",
            ["ManualBaseFooter"] = "「手動指定」參數當下的處理器效能與下放到命令列的參數（即開發者意圖）而定，這些參數會影響碼率分配的精度，即無損壓縮機制的壓縮率。",
            ["ManualExtTitle"] = "手動指定—第三方擴展參數",
            ["ManualExtIntro"] = "非官方版的影片編碼器提供了擴展功能（官方和其它修改版並不支持），勾選使用前應檢查參數是否存在。",
            ["CloseButtonText"] = "關閉"
        }
    };

    static AppUsageLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["HowToUse"] = "Comment utiliser ce programme",
            ["UpdateTitle"] = "Comment mettre à jour",
            ["UpdateDesc"] = "Conservez le dossier 1cenc et remplacez OneColumnEncoder.exe. Toutes vos configurations et données seront préservées.",
            ["Description"] = "Ce programme suit strictement l'ordre haut→bas, gauche→droite; les boutons « suivant » sont à droite.",
            ["CopyHint"] = "Astuce : sélection de texte et Ctrl+C sont pris en charge",
            ["GettingStarted"] = "La méthode la plus simple :",
            ["Step1"] = "1. Importer et choisir un outil amont (ffmpeg, vspipe, avs2yuv, etc.)",
            ["Step2"] = "2. Importer et choisir un encodeur / outil aval (x264, x265, etc.)",
            ["Step3"] = "3. Importer le fichier vidéo source",
            ["Step4"] = "4. Choisir des réglages validés comme compatibles et sains",
            ["Step5"] = "5. Valider la checklist et démarrer",
            ["WhyDisabledTitle"] = "Pourquoi le bouton Démarrer est désactivé",
            ["WhyDisabled1"] = "1. Cliquez sur Bypass sous une checklist avec erreur détectée",
            ["WhyDisabled2"] = "2. Entrée mauvaise, sortie mauvaise (la plupart ne bloquent pas)",
            ["WhyDisabled3"] = "3. Encoder une vidéo corrompue peut rarement planter le PC (BSOD)",
            ["ToolDownloadTitle"] = "Télécharger les outils d'encodage vidéo",
            ["ComplianceTitle"] = "Conformité usage commercial",
            ["ComplianceDesc"] = "Ce programme est sous licence Apache 2.0. Pour un usage commercial, vérifiez aussi les exigences des programmes importés.",
            ["LicenseFfmpeg"] = "· FFmpeg / FFprobe droits & licence : https://ffmpeg.org/legal.html",
            ["LicenseVapourSynth"] = "· Licence VapourSynth : https://github.com/vapoursynth/vapoursynth/blob/master/COPYING",
            ["LicenseAvs2yuv"] = "· Licence Avs2YUV : https://github.com/FFMS/ffms2/blob/master/COPYING.GPLv3",
            ["LicenseAvs2pipemod"] = "· Licence Avs2Pipemod : https://github.com/pinterf/AvsPmod",
            ["LicenseSvfi"] = "· Licence SVFI : https://github.com/Justin62628/Squirrel-RIFE/blob/master/LICENSE",
            ["LicenseX264"] = "· Licence x264 & brevets AVC : https://x264.org/licensing/",
            ["LicenseX265"] = "· Licence x265 & brevets HEVC : https://www.videolan.org/developers/x265.html",
            ["LicenseSvtAv1"] = "· Licence SVT-AV1 / AV1 : https://gitlab.com/AOMediaCodec/SVT-AV1/-/blob/master/LICENSE.md",
            ["ComplianceFooter"] = "...y compris conteneurs vidéo, codecs audio et surtout polices utilisées commercialement.",
            ["ComplianceDisclaimer"] = "L'utilisateur doit assurer la conformité aux licences, brevets codecs, formats média et licences de polices de sa région.",
            ["ParamConfigTitle"] = "Description de la configuration des paramètres",
            ["ParamConfigIntro"] = "Le programme aligne les paramètres sur la source tout en laissant assez de marge à l'encodeur.",
            ["AutoParamTitle"] = "Paramètres auto ou fournis par Y4M",
            ["AutoParamBase"] = "· Base : cadence, résolution, total images, matrice, transfert, primaires",
            ["AutoParamEncode"] = "· Encodage : lookahead, rayon de recherche, précision subpixel",
            ["AutoParamFooter"] = "...sauf encodeurs capables de configurer ces paramètres automatiquement",
            ["ManualBaseTitle"] = "Spécifié manuellement - paramètres de base",
            ["ManualBaseList"] = "· x264 : général, stock footage\n· x265 : général, film, stock footage, anime, stress test\n· SVT-AV1 : ultra HQ, haute compression, rapide",
            ["ManualBaseFooter"] = "...dépend des performances CPU et des options exposées en ligne de commande; affecte la précision d'allocation des bits.",
            ["ManualExtTitle"] = "Spécifié manuellement - paramètres étendus tiers",
            ["ManualExtIntro"] = "Les encodeurs modifiés peuvent fournir des fonctions absentes des versions officielles; vérifiez l'existence des paramètres avant de cocher.",
            ["CloseButtonText"] = "Fermer"
        };
        Data["es"] = new(Data["en"])
        {
            ["HowToUse"] = "Cómo usar este programa",
            ["UpdateTitle"] = "Cómo actualizar esta aplicación",
            ["UpdateDesc"] = "Conserve la carpeta 1cenc y reemplace OneColumnEncoder.exe. Toda su configuración y datos se conservarán.",
            ["Description"] = "El programa sigue un orden estricto de arriba→abajo e izquierda→derecha; los botones siguientes están a la derecha.",
            ["CopyHint"] = "Consejo: esta ventana permite seleccionar texto y copiar con Ctrl+C",
            ["GettingStarted"] = "La forma más simple de empezar:",
["Step1"] = "1. Importe y seleccione una herramienta aguas arriba (ffmpeg, vspipe, avs2yuv, etc.)",
            ["Step2"] = "2. Importe y seleccione un codificador / aguas abajo (x264, x265, etc.)",
            ["Step3"] = "3. Importe el archivo de vídeo fuente",
            ["Step4"] = "4. Elija ajustes validados como compatibles y sanos",
            ["Step5"] = "5. Supere la lista de comprobación e inicie",
            ["WhyDisabledTitle"] = "Por qué está desactivado Iniciar codificación",
            ["WhyDisabled1"] = "1. Pulse Bypass bajo una lista con error detectado",
            ["WhyDisabled2"] = "2. Basura entra, basura sale (casi nunca bloquea)",
            ["WhyDisabled3"] = "3. Codificar vídeo corrupto puede colgar el PC (BSOD) en casos raros",
            ["ToolDownloadTitle"] = "Descarga de herramientas de codificación",
            ["ComplianceTitle"] = "Cumplimiento para uso comercial",
            ["ComplianceDesc"] = "Este programa usa licencia Apache 2.0. Para uso comercial, revise también las exigencias de los programas importados.",
            ["LicenseFfmpeg"] = "· FFmpeg / FFprobe legal y licencia: https://ffmpeg.org/legal.html",
            ["LicenseVapourSynth"] = "· Licencia VapourSynth: https://github.com/vapoursynth/vapoursynth/blob/master/COPYING",
            ["LicenseAvs2yuv"] = "· Licencia Avs2YUV: https://github.com/FFMS/ffms2/blob/master/COPYING.GPLv3",
            ["LicenseAvs2pipemod"] = "· Licencia Avs2Pipemod: https://github.com/pinterf/AvsPmod",
            ["LicenseSvfi"] = "· Licencia SVFI: https://github.com/Justin62628/Squirrel-RIFE/blob/master/LICENSE",
            ["LicenseX264"] = "· Licencia x264 e info de patentes AVC: https://x264.org/licensing/",
            ["LicenseX265"] = "· Licencia x265 e info de patentes HEVC: https://www.videolan.org/developers/x265.html",
            ["LicenseSvtAv1"] = "· Licencia SVT-AV1 / AV1: https://gitlab.com/AOMediaCodec/SVT-AV1/-/blob/master/LICENSE.md",
            ["ComplianceFooter"] = "...incluidos contenedores de vídeo, codecs de audio y, sobre todo, fuentes usadas comercialmente.",
            ["ComplianceDisclaimer"] = "El usuario debe asegurar cumplimiento de licencias, patentes de codecs, formatos y fuentes en su región.",
            ["ParamConfigTitle"] = "Descripción de configuración de parámetros",
            ["ParamConfigIntro"] = "El programa alinea parámetros con la fuente y deja margen suficiente al codificador.",
            ["AutoParamTitle"] = "Parámetros automáticos o provistos por Y4M",
            ["AutoParamBase"] = "· Básicos: FPS, resolución, total de fotogramas, matriz, transferencia, primarios",
            ["AutoParamEncode"] = "· Codificación: lookahead, radio de búsqueda, fuerza subpixel",
            ["AutoParamFooter"] = "...excepto codificadores que autoconfiguren esos parámetros",
            ["ManualBaseTitle"] = "Manual - parámetros básicos",
            ["ManualBaseList"] = "· x264: general, stock footage\n· x265: general, cine, stock footage, anime, stress test\n· SVT-AV1: ultra HQ, alta compresión, rápido",
            ["ManualBaseFooter"] = "...depende de CPU y opciones expuestas a línea de comandos; afecta la precisión de asignación de bits.",
            ["ManualExtTitle"] = "Manual - parámetros extendidos de terceros",
            ["ManualExtIntro"] = "Codificadores modificados pueden exponer funciones no oficiales; confirme que el parámetro existe antes de marcar.",
            ["CloseButtonText"] = "Cerrar"
        };
        Data["ja"] = new(Data["en"])
        {
            ["HowToUse"] = "このプログラムの使い方",
            ["UpdateTitle"] = "このアプリケーションの更新方法",
            ["UpdateDesc"] = "1cenc フォルダを保持し、OneColumnEncoder.exe を置き換えてください。設定とデータはすべて保持されます。",
            ["Description"] = "操作順は上→下、左→右です。次へ進むボタンは右側にあります。",
            ["CopyHint"] = "ヒント: このウィンドウは文字選択と Ctrl+C コピーに対応しています",
            ["GettingStarted"] = "最も簡単な開始手順:",
            ["Step1"] = "1. 上流ツール (ffmpeg, vspipe, avs2yuv など) を取込・選択",
            ["Step2"] = "2. エンコーダ / 下流ツール (x264, x265 など) を取込・選択",
            ["Step3"] = "3. ソース動画ファイルを取込",
            ["Step4"] = "4. 互換性と状態が検証されたエンコード設定を選択",
            ["Step5"] = "5. チェックリストを解決して開始",
            ["WhyDisabledTitle"] = "開始ボタンが無効な理由",
            ["WhyDisabled1"] = "1. エラーを検出したチェックリスト下の Bypass をクリック",
            ["WhyDisabled2"] = "2. 入力が悪ければ出力も悪い（多くは開始を止めません）",
            ["WhyDisabled3"] = "3. 破損動画のエンコードは稀に PC クラッシュ (BSOD) を起こします",
            ["ToolDownloadTitle"] = "動画エンコード関連ツールの入手",
            ["ComplianceTitle"] = "商用利用のコンプライアンス",
            ["ComplianceDesc"] = "本プログラムは Apache License 2.0 です。商用利用では、取込む各プログラムの要件も確認してください。",
            ["LicenseFfmpeg"] = "· FFmpeg / FFprobe 法務・ライセンス: https://ffmpeg.org/legal.html",
            ["LicenseVapourSynth"] = "· VapourSynth ライセンス: https://github.com/vapoursynth/vapoursynth/blob/master/COPYING",
            ["LicenseAvs2yuv"] = "· Avs2YUV ライセンス: https://github.com/FFMS/ffms2/blob/master/COPYING.GPLv3",
            ["LicenseAvs2pipemod"] = "· Avs2Pipemod ライセンス: https://github.com/pinterf/AvsPmod",
            ["LicenseSvfi"] = "· SVFI ライセンス: https://github.com/Justin62628/Squirrel-RIFE/blob/master/LICENSE",
            ["LicenseX264"] = "· x264 ライセンスと AVC 特許情報: https://x264.org/licensing/",
            ["LicenseX265"] = "· x265 ライセンスと HEVC 特許情報: https://www.videolan.org/developers/x265.html",
            ["LicenseSvtAv1"] = "· SVT-AV1 / AV1 ライセンス情報: https://gitlab.com/AOMediaCodec/SVT-AV1/-/blob/master/LICENSE.md",
            ["ComplianceFooter"] = "...動画コンテナ、音声コーデック、とくに商用利用に含まれるフォントも対象です。",
            ["ComplianceDisclaimer"] = "利用者は地域のソフトウェアライセンス、コーデック特許、媒体形式、フォントライセンスに従う責任があります。",
            ["ParamConfigTitle"] = "パラメータ設定機能の説明",
            ["ParamConfigIntro"] = "ソースに合わせつつ、エンコーダが動ける余地を残す方針です。",
            ["AutoParamTitle"] = "自動指定または Y4M 由来のパラメータ",
            ["AutoParamBase"] = "· 基本: フレームレート、解像度、総フレーム、色行列、伝達特性、原色",
            ["AutoParamEncode"] = "· エンコード: lookahead、動き探索半径、サブピクセル探索強度",
            ["AutoParamFooter"] = "...上記を自動設定できるエンコーダを除く",
            ["ManualBaseTitle"] = "手動指定 - 基本パラメータ",
            ["ManualBaseList"] = "· x264: 汎用、素材映像\n· x265: 汎用、映画、素材映像、アニメ、ストレステスト\n· SVT-AV1: ultra HQ、高圧縮、高速",
            ["ManualBaseFooter"] = "...CPU 性能とコマンドラインに出す設定に依存し、ビット配分精度に影響します。",
            ["ManualExtTitle"] = "手動指定 - サードパーティ拡張",
            ["ManualExtIntro"] = "改造版エンコーダの拡張機能は公式版や他改造版で未対応の場合があります。チェック前に存在を確認してください。",
            ["CloseButtonText"] = "閉じる"
        };
        Data["ru"] = new(Data["en"])
        {
            ["HowToUse"] = "Как пользоваться программой",
            ["UpdateTitle"] = "Как обновить приложение",
            ["UpdateDesc"] = "Сохраните папку 1cenc и замените OneColumnEncoder.exe. Все ваши настройки и данные будут сохранены.",
            ["Description"] = "Программа строго идет сверху вниз и слева направо; все кнопки «далее» находятся справа.",
            ["CopyHint"] = "Совет: здесь можно выделять текст и копировать Ctrl+C",
            ["GettingStarted"] = "Самый простой старт:",
["Step1"] = "1. Импортируйте и выберите апстрим-инструмент (ffmpeg, vspipe, avs2yuv и т. п.)",
            ["Step2"] = "2. Импортируйте и выберите кодер / даунстрим (x264, x265 и т. п.)",
            ["Step3"] = "3. Импортируйте исходный видеофайл",
            ["Step4"] = "4. Выберите настройки, проверенные на совместимость и состояние",
            ["Step5"] = "5. Пройдите список проверок и запустите",
            ["WhyDisabledTitle"] = "Почему кнопка запуска отключена",
            ["WhyDisabled1"] = "1. Нажмите Bypass под checklist с найденной ошибкой",
            ["WhyDisabled2"] = "2. Плохой ввод дает плохой вывод (обычно это не блокирует)",
            ["WhyDisabled3"] = "3. Кодирование поврежденного видео редко может вызвать BSOD",
            ["ToolDownloadTitle"] = "Загрузка инструментов кодирования видео",
            ["ComplianceTitle"] = "Соответствие для коммерческого использования",
            ["ComplianceDesc"] = "Программа под Apache License 2.0. Для коммерческого использования учитывайте требования импортируемых программ.",
            ["LicenseFfmpeg"] = "· FFmpeg / FFprobe legal & license: https://ffmpeg.org/legal.html",
            ["LicenseVapourSynth"] = "· Лицензия VapourSynth: https://github.com/vapoursynth/vapoursynth/blob/master/COPYING",
            ["LicenseAvs2yuv"] = "· Лицензия Avs2YUV: https://github.com/FFMS/ffms2/blob/master/COPYING.GPLv3",
            ["LicenseAvs2pipemod"] = "· Лицензия Avs2Pipemod: https://github.com/pinterf/AvsPmod",
            ["LicenseSvfi"] = "· Лицензия SVFI: https://github.com/Justin62628/Squirrel-RIFE/blob/master/LICENSE",
            ["LicenseX264"] = "· Лицензия x264 и патенты AVC: https://x264.org/licensing/",
            ["LicenseX265"] = "· Лицензия x265 и патенты HEVC: https://www.videolan.org/developers/x265.html",
            ["LicenseSvtAv1"] = "· Лицензия SVT-AV1 / AV1: https://gitlab.com/AOMediaCodec/SVT-AV1/-/blob/master/LICENSE.md",
            ["ComplianceFooter"] = "...включая контейнеры видео, аудиокодеки и особенно шрифты для коммерческого использования.",
            ["ComplianceDisclaimer"] = "Пользователь отвечает за соблюдение лицензий ПО, патентов кодеков, медиaформатов и лицензий шрифтов в своем регионе.",
            ["ParamConfigTitle"] = "Описание настройки параметров",
            ["ParamConfigIntro"] = "Программа выравнивает параметры под источник и оставляет кодеру достаточно свободы.",
            ["AutoParamTitle"] = "Авто- или Y4M-параметры",
            ["AutoParamBase"] = "· База: FPS, разрешение, кадры всего, матрица, transfer, primaries",
            ["AutoParamEncode"] = "· Кодирование: lookahead, радиус ME, subpixel ME",
            ["AutoParamFooter"] = "...кроме кодеров, способных настроить это автоматически",
            ["ManualBaseTitle"] = "Ручные базовые параметры",
            ["ManualBaseList"] = "· x264: общее, stock footage\n· x265: общее, фильм, stock footage, anime, stress test\n· SVT-AV1: ultra HQ, высокое сжатие, быстро",
            ["ManualBaseFooter"] = "...зависит от CPU и параметров CLI; влияет на точность распределения битов.",
            ["ManualExtTitle"] = "Ручные сторонние расширения",
            ["ManualExtIntro"] = "Модифицированные кодеры могут иметь функции, которых нет в официальных версиях; проверьте параметры перед включением.",
            ["CloseButtonText"] = "Закрыть"
        };
    }

    public const string WindowTitle = "1cenc Usage & Compliance";
    public string HowToUse { get; }
    public string Description { get; }
    public string CopyHint { get; }
    public string GettingStarted { get; }
    public string Step1 { get; }
    public string Step2 { get; }
    public string Step3 { get; }
    public string Step4 { get; }
    public string Step5 { get; }
    public string WhyDisabledTitle { get; }
    public string WhyDisabled1 { get; }
    public string WhyDisabled2 { get; }
    public string WhyDisabled3 { get; }
    public string ToolDownloadTitle { get; }
    public string ToolDownloadLink { get; } = "\u00B7 https://github.com/iAvoe/encoding-tools-download-tutorial";
    public string UpdateTitle { get; }
    public string UpdateDesc { get; }
    public string ComplianceTitle { get; }
    public string ComplianceDesc { get; }
    public string LicenseFfmpeg { get; }
    public string LicenseVapourSynth { get; }
    public string LicenseAvs2yuv { get; }
    public string LicenseAvs2pipemod { get; }
    public string LicenseSvfi { get; }
    public string LicenseX264 { get; }
    public string LicenseX265 { get; }
    public string LicenseSvtAv1 { get; }
    public string ComplianceFooter { get; }
    public string ComplianceDisclaimer { get; }
    public string ParamConfigTitle { get; }
    public string ParamConfigIntro { get; }
    public string AutoParamTitle { get; }
    public string AutoParamBase { get; }
    public string AutoParamEncode { get; }
    public string AutoParamFooter { get; }
    public string ManualBaseTitle { get; }
    public string ManualBaseList { get; }
    public string ManualBaseFooter { get; }
    public string ManualExtTitle { get; }
    public string ManualExtIntro { get; }
    public string CloseButtonText { get; }

    public AppUsageLangProvider(string languageCode)
    {
        var d = Data.TryGetValue(languageCode, out var lang) ? lang : Data["en"];
        HowToUse = d["HowToUse"];
        Description = d["Description"];
        CopyHint = d["CopyHint"];
        GettingStarted = d["GettingStarted"];
        Step1 = d["Step1"];
        Step2 = d["Step2"];
        Step3 = d["Step3"];
        Step4 = d["Step4"];
        Step5 = d["Step5"];
        WhyDisabledTitle = d["WhyDisabledTitle"];
        WhyDisabled1 = d["WhyDisabled1"];
        WhyDisabled2 = d["WhyDisabled2"];
        WhyDisabled3 = d["WhyDisabled3"];
        ToolDownloadTitle = d["ToolDownloadTitle"];
        UpdateTitle = d["UpdateTitle"];
        UpdateDesc = d["UpdateDesc"];
        ComplianceTitle = d["ComplianceTitle"];
        ComplianceDesc = d["ComplianceDesc"];
        LicenseFfmpeg = d["LicenseFfmpeg"];
        LicenseVapourSynth = d["LicenseVapourSynth"];
        LicenseAvs2yuv = d["LicenseAvs2yuv"];
        LicenseAvs2pipemod = d["LicenseAvs2pipemod"];
        LicenseSvfi = d["LicenseSvfi"];
        LicenseX264 = d["LicenseX264"];
        LicenseX265 = d["LicenseX265"];
        LicenseSvtAv1 = d["LicenseSvtAv1"];
        ComplianceFooter = d["ComplianceFooter"];
        ComplianceDisclaimer = d["ComplianceDisclaimer"];
        ParamConfigTitle = d["ParamConfigTitle"];
        ParamConfigIntro = d["ParamConfigIntro"];
        AutoParamTitle = d["AutoParamTitle"];
        AutoParamBase = d["AutoParamBase"];
        AutoParamEncode = d["AutoParamEncode"];
        AutoParamFooter = d["AutoParamFooter"];
        ManualBaseTitle = d["ManualBaseTitle"];
        ManualBaseList = d["ManualBaseList"];
        ManualBaseFooter = d["ManualBaseFooter"];
        ManualExtTitle = d["ManualExtTitle"];
        ManualExtIntro = d["ManualExtIntro"];
        CloseButtonText = d["CloseButtonText"];
    }
}
