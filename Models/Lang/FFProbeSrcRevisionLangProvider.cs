namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Localized strings for ffprobe source revision JSON-parsing exceptions.
/// </summary>
public class FFProbeSrcRevisionLangProvider(string languageCode) : LangProviderBase(languageCode, Data)
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["SrcRevise.NoEntriesInQueue"] = "No ffprobe JSON entries found in queue file.",
            ["SrcRevise.FailedToParseBatch"] = "Failed to parse BatchRawJson.",
            ["SrcRevise.NoEntriesInBatch"] = "No ffprobe JSON entries found in BatchRawJson.",
            ["SrcRevise.MissingEntriesArray"] = "JSON root is missing 'Entries' array.",
            ["SrcRevise.RevisedJsonNull"] = "Revised ffprobe JSON resolved to null.",
            ["SrcRevise.JsonRootNotObject"] = "JSON root is not an object.",
        },
        ["zh-cn"] = new()
        {
            ["SrcRevise.NoEntriesInQueue"] = "队列文件中未找到 ffprobe JSON 条目。",
            ["SrcRevise.FailedToParseBatch"] = "解析 BatchRawJson 失败。",
            ["SrcRevise.NoEntriesInBatch"] = "BatchRawJson 中未找到 ffprobe JSON 条目。",
            ["SrcRevise.MissingEntriesArray"] = "JSON 根节点缺少 \"Entries\" 数组。",
            ["SrcRevise.RevisedJsonNull"] = "修正后的 ffprobe JSON 结果为 null。",
            ["SrcRevise.JsonRootNotObject"] = "JSON 根节点不是对象。",
        },
        ["zh-tw"] = new()
        {
            ["SrcRevise.NoEntriesInQueue"] = "佇列檔案中未找到 ffprobe JSON 條目。",
            ["SrcRevise.FailedToParseBatch"] = "解析 BatchRawJson 失敗。",
            ["SrcRevise.NoEntriesInBatch"] = "BatchRawJson 中未找到 ffprobe JSON 條目。",
            ["SrcRevise.MissingEntriesArray"] = "JSON 根節點缺少 \"Entries\" 陣列。",
            ["SrcRevise.RevisedJsonNull"] = "修正後的 ffprobe JSON 結果為 null。",
            ["SrcRevise.JsonRootNotObject"] = "JSON 根節點不是物件。",
        },
        ["fr"] = new()
        {
            ["SrcRevise.NoEntriesInQueue"] = "Aucune entrée ffprobe JSON trouvée dans le fichier queue.",
            ["SrcRevise.FailedToParseBatch"] = "Échec de l'analyse de BatchRawJson.",
            ["SrcRevise.NoEntriesInBatch"] = "Aucune entrée ffprobe JSON trouvée dans BatchRawJson.",
            ["SrcRevise.MissingEntriesArray"] = "La racine JSON ne contient pas de tableau « Entries ».",
            ["SrcRevise.RevisedJsonNull"] = "Le ffprobe JSON révisé est résolu à null.",
            ["SrcRevise.JsonRootNotObject"] = "La racine JSON n'est pas un objet.",
        },
        ["es"] = new()
        {
            ["SrcRevise.NoEntriesInQueue"] = "No se encontraron entradas ffprobe JSON en el archivo de cola.",
            ["SrcRevise.FailedToParseBatch"] = "Error al analizar BatchRawJson.",
            ["SrcRevise.NoEntriesInBatch"] = "No se encontraron entradas ffprobe JSON en BatchRawJson.",
            ["SrcRevise.MissingEntriesArray"] = "La raíz JSON carece del arreglo « Entries ».",
            ["SrcRevise.RevisedJsonNull"] = "El ffprobe JSON revisado resultó en null.",
            ["SrcRevise.JsonRootNotObject"] = "La raíz JSON no es un objeto.",
        },
        ["ja"] = new()
        {
            ["SrcRevise.NoEntriesInQueue"] = "キューファイルに ffprobe JSON エントリが見つかりません。",
            ["SrcRevise.FailedToParseBatch"] = "BatchRawJson の解析に失敗しました。",
            ["SrcRevise.NoEntriesInBatch"] = "BatchRawJson に ffprobe JSON エントリが見つかりません。",
            ["SrcRevise.MissingEntriesArray"] = "JSON ルートに「Entries」配列がありません。",
            ["SrcRevise.RevisedJsonNull"] = "修正後の ffprobe JSON が null になりました。",
            ["SrcRevise.JsonRootNotObject"] = "JSON ルートがオブジェクトではありません。",
        },
        ["ru"] = new()
        {
            ["SrcRevise.NoEntriesInQueue"] = "В файле очереди не найдены записи ffprobe JSON.",
            ["SrcRevise.FailedToParseBatch"] = "Не удалось разобрать BatchRawJson.",
            ["SrcRevise.NoEntriesInBatch"] = "В BatchRawJson не найдены записи ffprobe JSON.",
            ["SrcRevise.MissingEntriesArray"] = "В корне JSON отсутствует массив «Entries».",
            ["SrcRevise.RevisedJsonNull"] = "Исправленный ffprobe JSON равен null.",
            ["SrcRevise.JsonRootNotObject"] = "Корень JSON не является объектом.",
        },
        ["de"] = new()
        {
            ["SrcRevise.NoEntriesInQueue"] = "Keine ffprobe-JSON-Einträge in der Warteschlangendatei gefunden.",
            ["SrcRevise.FailedToParseBatch"] = "BatchRawJson konnte nicht analysiert werden.",
            ["SrcRevise.NoEntriesInBatch"] = "Keine ffprobe-JSON-Einträge in BatchRawJson gefunden.",
            ["SrcRevise.MissingEntriesArray"] = "Dem JSON-Root fehlt das Feld „Entries“.",
            ["SrcRevise.RevisedJsonNull"] = "Das korrigierte ffprobe-JSON ist null.",
            ["SrcRevise.JsonRootNotObject"] = "JSON-Root ist kein Objekt.",
        },
        ["ko"] = new()
        {
            ["SrcRevise.NoEntriesInQueue"] = "큐 파일에서 ffprobe JSON 항목을 찾을 수 없습니다.",
            ["SrcRevise.FailedToParseBatch"] = "BatchRawJson 파싱에 실패했습니다.",
            ["SrcRevise.NoEntriesInBatch"] = "BatchRawJson에서 ffprobe JSON 항목을 찾을 수 없습니다.",
            ["SrcRevise.MissingEntriesArray"] = "JSON 루트에 \"Entries\" 배열이 없습니다.",
            ["SrcRevise.RevisedJsonNull"] = "수정된 ffprobe JSON이 null로 반환되었습니다.",
            ["SrcRevise.JsonRootNotObject"] = "JSON 루트가 객체가 아닙니다.",
        },
        ["pt-br"] = new()
        {
            ["SrcRevise.NoEntriesInQueue"] = "Nenhuma entrada ffprobe JSON encontrada no arquivo de fila.",
            ["SrcRevise.FailedToParseBatch"] = "Falha ao analisar BatchRawJson.",
            ["SrcRevise.NoEntriesInBatch"] = "Nenhuma entrada ffprobe JSON encontrada em BatchRawJson.",
            ["SrcRevise.MissingEntriesArray"] = "A raiz JSON não possui o array « Entries ».",
            ["SrcRevise.RevisedJsonNull"] = "O ffprobe JSON revisado resultou em null.",
            ["SrcRevise.JsonRootNotObject"] = "A raiz JSON não é um objeto.",
        },
    };

    public string NoEntriesInQueue => this["SrcRevise.NoEntriesInQueue"];
    public string FailedToParseBatch => this["SrcRevise.FailedToParseBatch"];
    public string NoEntriesInBatch => this["SrcRevise.NoEntriesInBatch"];
    public string MissingEntriesArray => this["SrcRevise.MissingEntriesArray"];
    public string RevisedJsonNull => this["SrcRevise.RevisedJsonNull"];
    public string JsonRootNotObject => this["SrcRevise.JsonRootNotObject"];

    public static FFProbeSrcRevisionLangProvider Current => new(UILangProvider.Current.LanguageCode);
}
