namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Localized strings for the L-SMASH index prefetch progress window.
/// </summary>
public class LsmashIndexLangProvider(string languageCode) : LangProviderBase(languageCode, Data)
{
    // Window titles stay in English for support and web-search consistency.
    public const string WindowTitle = "L-SMASH Indexing";

    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["LsmashIndex.CountFormat"] = "Indexed {0}/{1}",
            ["LsmashIndex.Preparing"] = "Preparing index…",
            ["LsmashIndex.CurrentFile"] = "Indexing {0}",
            ["LsmashIndex.Done"] = "Indexing complete",
            ["LsmashIndex.ConfirmTitle"] = "Pre-build L-SMASH Index",
            ["LsmashIndex.ConfirmMessage"] = "No frame server upstream (vspipe/avs2yuv/avs2pipemod) is selected.\n\nL-SMASH indexing speeds up AviSynth and VapourSynth source loading. FFmpeg does not use .lwi indexes.\n\nProceed with indexing anyway?",
        },
        ["zh-cn"] = new()
        {
            ["LsmashIndex.CountFormat"] = "已完成 {0}/{1}",
            ["LsmashIndex.Preparing"] = "正在准备索引…",
            ["LsmashIndex.CurrentFile"] = "正在建立索引：{0}",
            ["LsmashIndex.Done"] = "索引建立完成",
            ["LsmashIndex.ConfirmTitle"] = "预先建立 L-SMASH 索引",
            ["LsmashIndex.ConfirmMessage"] = "未选择帧服务器上游（vspipe/avs2yuv/avs2pipemod）。\n\nL-SMASH 索引可加速 AviSynth 和 VapourSynth 源加载。FFmpeg 不使用 .lwi 索引。\n\n是否仍要建立索引？",
        },
        ["zh-tw"] = new()
        {
            ["LsmashIndex.CountFormat"] = "已完成 {0}/{1}",
            ["LsmashIndex.Preparing"] = "正在準備索引…",
            ["LsmashIndex.CurrentFile"] = "正在建立索引：{0}",
            ["LsmashIndex.Done"] = "索引建立完成",
            ["LsmashIndex.ConfirmTitle"] = "預先建立 L-SMASH 索引",
            ["LsmashIndex.ConfirmMessage"] = "未選擇訊框伺服器上游（vspipe/avs2yuv/avs2pipemod）。\n\nL-SMASH 索引可加速 AviSynth 和 VapourSynth 來源載入。FFmpeg 不使用 .lwi 索引。\n\n是否仍要建立索引？",
        },
        ["fr"] = new()
        {
            ["LsmashIndex.CountFormat"] = "Indexés {0}/{1}",
            ["LsmashIndex.Preparing"] = "Préparation de l'index…",
            ["LsmashIndex.CurrentFile"] = "Indexation de {0}",
            ["LsmashIndex.Done"] = "Indexation terminée",
            ["LsmashIndex.ConfirmTitle"] = "Pré-créer l'index L-SMASH",
            ["LsmashIndex.ConfirmMessage"] = "Aucun serveur de trames (vspipe/avs2yuv/avs2pipemod) n'est sélectionné.\n\nL'indexation L-SMASH accélère le chargement des sources AviSynth et VapourSynth. FFmpeg n'utilise pas les index .lwi.\n\nContinuer l'indexation quand même ?",
        },
        ["es"] = new()
        {
            ["LsmashIndex.CountFormat"] = "Indexados {0}/{1}",
            ["LsmashIndex.Preparing"] = "Preparando índice…",
            ["LsmashIndex.CurrentFile"] = "Indexando {0}",
            ["LsmashIndex.Done"] = "Indexación completada",
            ["LsmashIndex.ConfirmTitle"] = "Precrear índice L-SMASH",
            ["LsmashIndex.ConfirmMessage"] = "No hay ningún servidor de fotogramas (vspipe/avs2yuv/avs2pipemod) seleccionado.\n\nLa indexación L-SMASH acelera la carga de fuentes AviSynth y VapourSynth. FFmpeg no usa índices .lwi.\n\n¿Continuar con la indexación de todas formas?",
        },
        ["ja"] = new()
        {
            ["LsmashIndex.CountFormat"] = "インデックス済み {0}/{1}",
            ["LsmashIndex.Preparing"] = "インデックスを準備中…",
            ["LsmashIndex.CurrentFile"] = "インデックス作成中: {0}",
            ["LsmashIndex.Done"] = "インデックス作成完了",
            ["LsmashIndex.ConfirmTitle"] = "L-SMASH インデックスを事前作成",
            ["LsmashIndex.ConfirmMessage"] = "フレームサーバー上流（vspipe/avs2yuv/avs2pipemod）が選択されていません。\n\nL-SMASH インデックスは AviSynth と VapourSynth のソース読み込みを高速化します。FFmpeg は .lwi インデックスを使用しません。\n\nそれでもインデックスを作成しますか？",
        },
        ["ru"] = new()
        {
            ["LsmashIndex.CountFormat"] = "Проиндексировано {0}/{1}",
            ["LsmashIndex.Preparing"] = "Подготовка индекса…",
            ["LsmashIndex.CurrentFile"] = "Индексация {0}",
            ["LsmashIndex.Done"] = "Индексация завершена",
            ["LsmashIndex.ConfirmTitle"] = "Предварительно создать индекс L-SMASH",
            ["LsmashIndex.ConfirmMessage"] = "Не выбран фреймсервер (vspipe/avs2yuv/avs2pipemod).\n\nИндексация L-SMASH ускоряет загрузку источников AviSynth и VapourSynth. FFmpeg не использует индексы .lwi.\n\nВсё равно продолжить индексацию?",
        },
        ["de"] = new()
        {
            ["LsmashIndex.CountFormat"] = "Indiziert {0}/{1}",
            ["LsmashIndex.Preparing"] = "Index wird vorbereitet…",
            ["LsmashIndex.CurrentFile"] = "Indiziere {0}",
            ["LsmashIndex.Done"] = "Indizierung abgeschlossen",
            ["LsmashIndex.ConfirmTitle"] = "L-SMASH-Index vorab erstellen",
            ["LsmashIndex.ConfirmMessage"] = "Kein Frameserver (vspipe/avs2yuv/avs2pipemod) ausgewählt.\n\nL-SMASH-Indizierung beschleunigt das Laden von AviSynth- und VapourSynth-Quellen. FFmpeg verwendet keine .lwi-Indizes.\n\nTrotzdem indizieren?",
        },
        ["ko"] = new()
        {
            ["LsmashIndex.CountFormat"] = "인덱싱 완료 {0}/{1}",
            ["LsmashIndex.Preparing"] = "인덱스 준비 중…",
            ["LsmashIndex.CurrentFile"] = "{0} 인덱싱 중",
            ["LsmashIndex.Done"] = "인덱싱 완료",
            ["LsmashIndex.ConfirmTitle"] = "L-SMASH 인덱스 미리 생성",
            ["LsmashIndex.ConfirmMessage"] = "프레임 서버 업스트림(vspipe/avs2yuv/avs2pipemod)이 선택되지 않았습니다.\n\nL-SMASH 인덱싱은 AviSynth 및 VapourSynth 소스 로딩을 가속화합니다. FFmpeg는 .lwi 인덱스를 사용하지 않습니다.\n\n그래도 인덱싱을 진행하시겠습니까?",
        },
        ["pt-br"] = new()
        {
            ["LsmashIndex.CountFormat"] = "Indexados {0}/{1}",
            ["LsmashIndex.Preparing"] = "Preparando índice…",
            ["LsmashIndex.CurrentFile"] = "Indexando {0}",
            ["LsmashIndex.Done"] = "Indexação concluída",
            ["LsmashIndex.ConfirmTitle"] = "Pré-criar índice L-SMASH",
            ["LsmashIndex.ConfirmMessage"] = "Nenhum servidor de quadros (vspipe/avs2yuv/avs2pipemod) está selecionado.\n\nA indexação L-SMASH acelera o carregamento de fontes AviSynth e VapourSynth. FFmpeg não usa índices .lwi.\n\nContinuar com a indexação mesmo assim?",
        },
    };

    public static LsmashIndexLangProvider Current => new(UILangProvider.Current.LanguageCode);
}
