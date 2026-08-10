namespace OneColumnEncoder.Models.Lang;

public class StartEncCmdLangProvider : LangProviderBase
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["WarnTitle"] = "Encoding",
            ["MissingUpstreamMsg"] = "Missing upstream input path. Make sure a video source or script source is selected for the chosen upstream tool.",
            ["ConfirmTitle"] = "Encoding Command",
            ["OverwriteTitle"] = "Overwrite Output",
            ["OverwriteMsg"] = "There are files already existing will be overwritten",
            ["EncodedOutputLabel"] = "Encoder output",
            ["MuxOutputLabel"] = "Mux output",
            ["OverwriteTargetLabel"] = "{0}: {1} ({2})",
            ["LargestExistingSizeLabel"] = "Largest existing size: {0}",
            ["ConfirmDelayLabel"] = "Confirm button unlocks after {0} seconds.",
            ["AdditionalOverwriteTargetsLabel"] = "...and {0} more target(s).",
            ["QueueJsonMissingMsg"] = "Queue JSON is missing. Run source queue analysis before starting queue encoding.",
            ["QueueJsonInvalidMsg"] = "Queue JSON cannot be read or parsed: {0}",
            ["QueueJsonNoEntriesMsg"] = "Queue JSON contains no accepted source entries.",
            ["QueueUnsupportedRouteMsg"] = "Queue encoding does not support the selected upstream tool.",
            ["QueueSourceMissingMsg"] = "Queue source file(s) are missing. Encoding cannot continue:",
            ["QueueDuplicateOutputMsg"] = "Queue output paths collide. Encoding cannot continue:",
            ["QueueEncodingPendingMsg"] = "Queue overwrite protection completed. Queue execution is not wired yet.",
            ["AllFilteredOutMsg"] = "All videos were filtered out by the duration filter.",
            ["GbSuffix"] = " GB",
            ["MbSuffix"] = " MB",
        },
        ["zh-cn"] = new()
        {
            ["WarnTitle"] = "编码",
            ["MissingUpstreamMsg"] = "缺少上游输入路径。请确保已为所选上游工具选择了视频源或脚本源。",
            ["ConfirmTitle"] = "编码命令",
            ["OverwriteTitle"] = "覆盖输出",
            ["OverwriteMsg"] = "以下输出文件已存在，将被覆盖。",
            ["EncodedOutputLabel"] = "编码器输出",
            ["MuxOutputLabel"] = "封装输出",
            ["OverwriteTargetLabel"] = "{0}：{1}（{2}）",
            ["LargestExistingSizeLabel"] = "最大现有大小：{0}",
            ["ConfirmDelayLabel"] = "确认按钮将在 {0} 秒后解锁。",
            ["AdditionalOverwriteTargetsLabel"] = "……以及另外 {0} 个目标。",
            ["QueueJsonMissingMsg"] = "缺少队列 JSON。请先运行源队列分析再开始队列压制。",
            ["QueueJsonInvalidMsg"] = "无法读取或解析队列 JSON：{0}",
            ["QueueJsonNoEntriesMsg"] = "队列 JSON 中没有可接受的源条目。",
            ["QueueUnsupportedRouteMsg"] = "队列压制不支持当前选中的上游工具。",
            ["QueueSourceMissingMsg"] = "以下队列源文件缺失，无法继续压制：",
            ["QueueDuplicateOutputMsg"] = "以下队列输出路径发生冲突，无法继续压制：",
            ["QueueEncodingPendingMsg"] = "队列覆盖保护检查已完成。队列执行流程尚未接入。",
            ["AllFilteredOutMsg"] = "所有视频已被时长过滤器过滤掉。",
            ["GbSuffix"] = " GB",
            ["MbSuffix"] = " MB",
        },
        ["zh-tw"] = new()
        {
            ["WarnTitle"] = "編碼",
            ["MissingUpstreamMsg"] = "缺少上游輸入路徑。請確保已為所選上游工具選擇了影片源或腳本源。",
            ["ConfirmTitle"] = "編碼命令",
            ["OverwriteTitle"] = "覆蓋輸出",
            ["OverwriteMsg"] = "以下輸出檔案已存在，將被覆蓋。",
            ["EncodedOutputLabel"] = "編碼器輸出",
            ["MuxOutputLabel"] = "封裝輸出",
            ["OverwriteTargetLabel"] = "{0}：{1}（{2}）",
            ["LargestExistingSizeLabel"] = "最大現有大小：{0}",
            ["ConfirmDelayLabel"] = "確認按鈕將在 {0} 秒後解鎖。",
            ["AdditionalOverwriteTargetsLabel"] = "……以及另外 {0} 個目標。",
            ["QueueJsonMissingMsg"] = "缺少隊列 JSON。請先執行來源隊列分析再開始隊列壓製。",
            ["QueueJsonInvalidMsg"] = "無法讀取或解析隊列 JSON：{0}",
            ["QueueJsonNoEntriesMsg"] = "隊列 JSON 中沒有可接受的來源項目。",
            ["QueueUnsupportedRouteMsg"] = "隊列壓製不支援目前選取的上游工具。",
            ["QueueSourceMissingMsg"] = "以下隊列來源檔案缺失，無法繼續壓製：",
            ["QueueDuplicateOutputMsg"] = "以下隊列輸出路徑發生衝突，無法繼續壓製：",
            ["QueueEncodingPendingMsg"] = "隊列覆蓋保護檢查已完成。隊列執行流程尚未接入。",
            ["AllFilteredOutMsg"] = "所有影片已被時長過濾器過濾掉。",
            ["GbSuffix"] = " GB",
            ["MbSuffix"] = " MB",
        }
    };

    static StartEncCmdLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["WarnTitle"] = "Encodage",
            ["MissingUpstreamMsg"] = "Chemin d'entrée amont manquant. Vérifiez qu'une source vidéo ou script est sélectionnée pour l'outil amont choisi.",
            ["ConfirmTitle"] = "Commande d'encodage",
            ["OverwriteTitle"] = "Écraser la sortie",
            ["OverwriteMsg"] = "Des fichiers existent déjà et seront écrasés.",
            ["EncodedOutputLabel"] = "Sortie encodeur",
            ["MuxOutputLabel"] = "Sortie mux",
            ["OverwriteTargetLabel"] = "{0} : {1} ({2})",
            ["LargestExistingSizeLabel"] = "Plus grande taille existante : {0}",
            ["ConfirmDelayLabel"] = "Le bouton de confirmation se déverrouille dans {0} s.",
            ["AdditionalOverwriteTargetsLabel"] = "...et {0} autre(s) cible(s).",
            ["QueueJsonMissingMsg"] = "JSON de file d'attente manquant. Exécutez l'analyse de la file d'attente source avant de lancer l'encodage.",
            ["QueueJsonInvalidMsg"] = "Le JSON de file d'attente est illisible ou invalide : {0}",
            ["QueueJsonNoEntriesMsg"] = "Le JSON de file d'attente ne contient aucune entrée source acceptée.",
            ["QueueUnsupportedRouteMsg"] = "L'encodage en file d'attente ne prend pas en charge l'outil amont sélectionné.",
            ["QueueSourceMissingMsg"] = "Fichier(s) source de file d'attente manquant(s). L'encodage ne peut pas continuer :",
            ["QueueDuplicateOutputMsg"] = "Les chemins de sortie de la file d'attente entrent en collision. L'encodage ne peut pas continuer :",
            ["QueueEncodingPendingMsg"] = "Protection contre l'écrasement de la file d'attente terminée. L'exécution de la file d'attente n'est pas encore câblée.",
            ["AllFilteredOutMsg"] = "Toutes les vidéos ont été filtrées par le filtre de durée.",
            ["GbSuffix"] = " Go",
            ["MbSuffix"] = " Mo"
        };
        Data["es"] = new(Data["en"])
        {
            ["WarnTitle"] = "Codificación",
            ["MissingUpstreamMsg"] = "Falta la ruta de entrada aguas arriba. Asegure una fuente de vídeo o script para la herramienta elegida.",
            ["ConfirmTitle"] = "Comando de codificación",
            ["OverwriteTitle"] = "Sobrescribir salida",
            ["OverwriteMsg"] = "Hay archivos que ya existen y serán sobrescritos",
            ["EncodedOutputLabel"] = "Salida del codificador",
            ["MuxOutputLabel"] = "Salida mux",
            ["OverwriteTargetLabel"] = "{0}: {1} ({2})",
            ["LargestExistingSizeLabel"] = "Mayor tamaño existente: {0}",
            ["ConfirmDelayLabel"] = "El botón se desbloquea en {0} s.",
            ["AdditionalOverwriteTargetsLabel"] = "...y {0} destino(s) más.",
            ["QueueJsonMissingMsg"] = "Falta el JSON de la cola. Ejecute el análisis de la cola de origen antes de iniciar la codificación por cola.",
            ["QueueJsonInvalidMsg"] = "No se puede leer o analizar el JSON de la cola: {0}",
            ["QueueJsonNoEntriesMsg"] = "El JSON de la cola no contiene entradas de origen aceptadas.",
            ["QueueUnsupportedRouteMsg"] = "La codificación por cola no admite la herramienta aguas arriba seleccionada.",
            ["QueueSourceMissingMsg"] = "Faltan archivos de origen de la cola. No se puede continuar la codificación:",
            ["QueueDuplicateOutputMsg"] = "Las rutas de salida de la cola colisionan. No se puede continuar la codificación:",
            ["QueueEncodingPendingMsg"] = "Protección contra sobrescritura de cola completada. La ejecución de la cola aún no está conectada.",
            ["AllFilteredOutMsg"] = "Todos los vídeos fueron filtrados por el filtro de duración.",
            ["GbSuffix"] = " GB",
            ["MbSuffix"] = " MB"
        };
        Data["ja"] = new(Data["en"])
        {
            ["WarnTitle"] = "エンコード",
            ["MissingUpstreamMsg"] = "上流入力パスがありません。選択した上流ツールに動画ソースまたはスクリプトを指定してください。",
            ["ConfirmTitle"] = "エンコードコマンド",
            ["OverwriteTitle"] = "出力を上書き",
            ["OverwriteMsg"] = "以下の既存ファイルは上書きされます",
            ["EncodedOutputLabel"] = "エンコーダ出力",
            ["MuxOutputLabel"] = "Mux 出力",
            ["OverwriteTargetLabel"] = "{0}: {1} ({2})",
            ["LargestExistingSizeLabel"] = "既存最大サイズ: {0}",
            ["ConfirmDelayLabel"] = "確認ボタンは {0} 秒後に有効になります。",
            ["AdditionalOverwriteTargetsLabel"] = "...他 {0} 個のターゲット。",
            ["QueueJsonMissingMsg"] = "キュー JSON が見つかりません。キューエンコードを開始する前にソースキュー分析を実行してください。",
            ["QueueJsonInvalidMsg"] = "キュー JSON を読み取りまたは解析できません: {0}",
            ["QueueJsonNoEntriesMsg"] = "キュー JSON に受け入れられたソースエントリがありません。",
            ["QueueUnsupportedRouteMsg"] = "キューエンコードは選択された上流ツールをサポートしていません。",
            ["QueueSourceMissingMsg"] = "キューのソースファイルが見つかりません。エンコードを続行できません:",
            ["QueueDuplicateOutputMsg"] = "キューの出力パスが衝突しています。エンコードを続行できません:",
            ["QueueEncodingPendingMsg"] = "キューの上書き保護が完了しました。キューの実行はまだ配線されていません。",
            ["AllFilteredOutMsg"] = "すべての動画が時間フィルターによって除外されました。",
            ["GbSuffix"] = " GB",
            ["MbSuffix"] = " MB"
        };
        Data["ru"] = new(Data["en"])
        {
            ["WarnTitle"] = "Кодирование",
            ["MissingUpstreamMsg"] = "Нет входного пути апстрима. Выберите видеоисточник или скрипт для выбранного апстрим-инструмента.",
            ["ConfirmTitle"] = "Команда кодирования",
            ["OverwriteTitle"] = "Перезапись вывода",
            ["OverwriteMsg"] = "Если существуют файлы, они будут перезаписаны",
            ["EncodedOutputLabel"] = "Вывод кодера",
            ["MuxOutputLabel"] = "Вывод mux",
            ["OverwriteTargetLabel"] = "{0}: {1} ({2})",
            ["LargestExistingSizeLabel"] = "Наибольший существующий размер: {0}",
            ["ConfirmDelayLabel"] = "Кнопка подтверждения откроется через {0} с.",
            ["AdditionalOverwriteTargetsLabel"] = "...и ещё {0} цель(ей).",
            ["QueueJsonMissingMsg"] = "Отсутствует JSON очереди. Запустите анализ очереди источников перед запуском пакетного кодирования.",
            ["QueueJsonInvalidMsg"] = "Невозможно прочитать или разобрать JSON очереди: {0}",
            ["QueueJsonNoEntriesMsg"] = "JSON очереди не содержит принятых записей источников.",
            ["QueueUnsupportedRouteMsg"] = "Пакетное кодирование не поддерживает выбранный апстрим-инструмент.",
            ["QueueSourceMissingMsg"] = "Файлы источников очереди отсутствуют. Кодирование невозможно:",
            ["QueueDuplicateOutputMsg"] = "Пути вывода очереди конфликтуют. Кодирование невозможно:",
            ["QueueEncodingPendingMsg"] = "Защита от перезаписи очереди завершена. Выполнение очереди ещё не подключено.",
            ["AllFilteredOutMsg"] = "Все видео отфильтрованы фильтром длительности.",
            ["GbSuffix"] = " ГБ",
            ["MbSuffix"] = " МБ"
        };
        Data["de"] = new(Data["en"])
        {
            ["WarnTitle"] = "Kodierung",
            ["MissingUpstreamMsg"] = "Kein Upstream-Eingabepfad. Wählen Sie eine Videoquelle oder ein Skript für das gewählte Upstream-Tool.",
            ["ConfirmTitle"] = "Kodierungsbefehl",
            ["OverwriteTitle"] = "Ausgabe überschreiben",
            ["OverwriteMsg"] = "Existierende Dateien werden überschrieben",
            ["EncodedOutputLabel"] = "Encoder-Ausgabe",
            ["MuxOutputLabel"] = "Mux-Ausgabe",
            ["OverwriteTargetLabel"] = "{0}: {1} ({2})",
            ["LargestExistingSizeLabel"] = "Größte vorhandene Größe: {0}",
            ["ConfirmDelayLabel"] = "Bestätigung-Schaltfläche wird in {0} s freigeschaltet.",
            ["AdditionalOverwriteTargetsLabel"] = "...und {0} weitere Ziele.",
            ["QueueJsonMissingMsg"] = "Warteschlangen-JSON fehlt. Führen Sie die Quellen-Warteschlangenanalyse aus.",
            ["QueueJsonInvalidMsg"] = "Warteschlangen-JSON kann nicht gelesen/verarbeitet werden: {0}",
            ["QueueJsonNoEntriesMsg"] = "Warteschlangen-JSON enthält keine akzeptierten Quelleinträge.",
            ["QueueUnsupportedRouteMsg"] = "Warteschlangenkodierung unterstützt das gewählte Upstream-Tool nicht.",
            ["QueueSourceMissingMsg"] = "Warteschlangen-Quelldateien fehlen. Kodierung nicht möglich:",
            ["QueueDuplicateOutputMsg"] = "Warteschlangen-Ausgabepfade kollidieren. Kodierung nicht möglich:",
            ["QueueEncodingPendingMsg"] = "Überschreibschutz abgeschlossen. Wartenschlangenausführung noch nicht implementiert.",
            ["AllFilteredOutMsg"] = "Alle Videos wurden durch den Dauerfilter gefiltert.",
            ["GbSuffix"] = " GB",
            ["MbSuffix"] = " MB"
        };
        Data["ko"] = new(Data["en"])
        {
            ["WarnTitle"] = "인코딩",
            ["MissingUpstreamMsg"] = "업스트림 입력 경로가 없습니다. 선택한 업스트림 도구에 비디오 소스 또는 스크립트 소스를 지정했는지 확인하세요.",
            ["ConfirmTitle"] = "인코딩 명령",
            ["OverwriteTitle"] = "출력 덮어쓰기",
            ["OverwriteMsg"] = "이미 존재하는 파일이 덮어써집니다",
            ["EncodedOutputLabel"] = "인코더 출력",
            ["MuxOutputLabel"] = "먹스 출력",
            ["OverwriteTargetLabel"] = "{0}: {1} ({2})",
            ["LargestExistingSizeLabel"] = "가장 큰 기존 크기: {0}",
            ["ConfirmDelayLabel"] = "확인 버튼은 {0}초 후에 활성화됩니다.",
            ["AdditionalOverwriteTargetsLabel"] = "...외 {0}개 대상.",
            ["QueueJsonMissingMsg"] = "대기열 JSON이 없습니다. 대기열 인코딩을 시작하기 전에 소스 대기열 분석을 실행하세요.",
            ["QueueJsonInvalidMsg"] = "대기열 JSON을 읽거나 구문 분석할 수 없습니다: {0}",
            ["QueueJsonNoEntriesMsg"] = "대기열 JSON에 수락된 소스 항목이 없습니다.",
            ["QueueUnsupportedRouteMsg"] = "대기열 인코딩은 선택한 업스트림 도구를 지원하지 않습니다.",
            ["QueueSourceMissingMsg"] = "대기열 소스 파일이 없습니다. 인코딩을 계속할 수 없습니다:",
            ["QueueDuplicateOutputMsg"] = "대기열 출력 경로가 충돌합니다. 인코딩을 계속할 수 없습니다:",
            ["QueueEncodingPendingMsg"] = "대기열 덮어쓰기 보호가 완료되었습니다. 대기열 실행은 아직 연결되지 않았습니다.",
            ["AllFilteredOutMsg"] = "길이 필터로 모든 동영상이 걸러졌습니다.",
            ["GbSuffix"] = " GB",
            ["MbSuffix"] = " MB",
        };
        Data["pt-br"] = new(Data["en"])
        {
            ["WarnTitle"] = "Codificação",
            ["MissingUpstreamMsg"] = "Falta o caminho de entrada upstream. Certifique-se de que uma fonte de vídeo ou script está selecionada para a ferramenta upstream escolhida.",
            ["ConfirmTitle"] = "Comando de codificação",
            ["OverwriteTitle"] = "Sobrescrever saída",
            ["OverwriteMsg"] = "Arquivos já existentes serão sobrescritos",
            ["EncodedOutputLabel"] = "Saída do codificador",
            ["MuxOutputLabel"] = "Saída mux",
            ["OverwriteTargetLabel"] = "{0}: {1} ({2})",
            ["LargestExistingSizeLabel"] = "Maior tamanho existente: {0}",
            ["ConfirmDelayLabel"] = "O botão de confirmação será desbloqueado após {0} segundos.",
            ["AdditionalOverwriteTargetsLabel"] = "...e {0} outro(s) destino(s).",
            ["QueueJsonMissingMsg"] = "JSON da fila ausente. Execute a análise da fila de fonte antes de iniciar a codificação da fila.",
            ["QueueJsonInvalidMsg"] = "Não é possível ler ou analisar o JSON da fila: {0}",
            ["QueueJsonNoEntriesMsg"] = "O JSON da fila não contém entradas de fonte aceitas.",
            ["QueueUnsupportedRouteMsg"] = "A codificação em fila não suporta a ferramenta upstream selecionada.",
            ["QueueSourceMissingMsg"] = "Arquivo(s) de fonte da fila ausente(s). A codificação não pode continuar:",
            ["QueueDuplicateOutputMsg"] = "Os caminhos de saída da fila colidem. A codificação não pode continuar:",
            ["QueueEncodingPendingMsg"] = "Proteção contra sobrescrita da fila concluída. A execução da fila ainda não está conectada.",
            ["AllFilteredOutMsg"] = "Todos os vídeos foram filtrados pelo filtro de duração.",
            ["GbSuffix"] = " GB",
            ["MbSuffix"] = " MB"
        };
    }

    public string WarnTitle { get; }
    public string MissingUpstreamMsg { get; }
    public string ConfirmTitle { get; }
    public string OverwriteTitle { get; }
    public string OverwriteMsg { get; }
    public string EncodedOutputLabel { get; }
    public string MuxOutputLabel { get; }
    public string OverwriteTargetLabel { get; }
    public string LargestExistingSizeLabel { get; }
    public string ConfirmDelayLabel { get; }
    public string AdditionalOverwriteTargetsLabel { get; }
    public string QueueJsonMissingMsg { get; }
    public string QueueJsonInvalidMsg { get; }
    public string QueueJsonNoEntriesMsg { get; }
    public string QueueUnsupportedRouteMsg { get; }
    public string QueueSourceMissingMsg { get; }
    public string QueueDuplicateOutputMsg { get; }
    public string QueueEncodingPendingMsg { get; }
    public string AllFilteredOutMsg { get; }
    public string GbSuffix { get; }
    public string MbSuffix { get; }
    public StartEncCmdLangProvider(string languageCode) : base(languageCode, Data)
    {
        WarnTitle = this["WarnTitle"];
        MissingUpstreamMsg = this["MissingUpstreamMsg"];
        ConfirmTitle = this["ConfirmTitle"];
        OverwriteTitle = this["OverwriteTitle"];
        OverwriteMsg = this["OverwriteMsg"];
        EncodedOutputLabel = this["EncodedOutputLabel"];
        MuxOutputLabel = this["MuxOutputLabel"];
        OverwriteTargetLabel = this["OverwriteTargetLabel"];
        LargestExistingSizeLabel = this["LargestExistingSizeLabel"];
        ConfirmDelayLabel = this["ConfirmDelayLabel"];
        AdditionalOverwriteTargetsLabel = this["AdditionalOverwriteTargetsLabel"];
        QueueJsonMissingMsg = this["QueueJsonMissingMsg"];
        QueueJsonInvalidMsg = this["QueueJsonInvalidMsg"];
        QueueJsonNoEntriesMsg = this["QueueJsonNoEntriesMsg"];
        QueueUnsupportedRouteMsg = this["QueueUnsupportedRouteMsg"];
        QueueSourceMissingMsg = this["QueueSourceMissingMsg"];
        QueueDuplicateOutputMsg = this["QueueDuplicateOutputMsg"];
        QueueEncodingPendingMsg = this["QueueEncodingPendingMsg"];
        AllFilteredOutMsg = this["AllFilteredOutMsg"];
        GbSuffix = this["GbSuffix"];
        MbSuffix = this["MbSuffix"];
    }
}
