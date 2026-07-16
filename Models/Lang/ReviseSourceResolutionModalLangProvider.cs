namespace OneColumnEncoder.Models.Lang;

public class ReviseSourceResolutionModalLangProvider
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["ReviseSourceResolution.Title"] = "1cenc Source Resolution",
            ["ReviseSourceResolution.Description"] = "Set the actual post-filter resolution to write into ffprobe JSON. The default value is copied from Filter Scribe's resolution shrink generator",
            ["ReviseSourceResolution.SettingsHeader"] = "New Resolution",
            ["ReviseSourceResolution.WidthLabel"] = "Width",
            ["ReviseSourceResolution.HeightLabel"] = "Height",
            ["ReviseSourceResolution.CurrentLabel"] = "Current analysis",
            ["ReviseSourceResolution.SuggestedLabel"] = "Res. shrink slider",
            ["ReviseSourceResolution.UnknownResolution"] = "Unknown",
            ["ReviseSourceResolution.ResolutionFormat"] = "{0}x{1}",
            ["ReviseSourceResolution.Cancel"] = "Cancel",
            ["ReviseSourceResolution.Confirm"] = "Update ffprobe JSON",
            ["ReviseSourceResolution.InvalidInput"] = "Resolution must use integer dimensions from 1 to 65535.",
            ["ReviseSourceResolution.EvenResolutionHint"] = "Resolution width and height must be even numbers.",
            ["ReviseSourceResolution.NoFfprobeJson"] = "No ffprobe JSON is available for the current source. Run source analysis first.",
            ["ReviseSourceResolution.UpdateFailed"] = "Failed to update ffprobe JSON: {0}",
        },
        ["zh-cn"] = new()
        {
            ["ReviseSourceResolution.Title"] = "1cenc 修订源分辨率",
            ["ReviseSourceResolution.Description"] = "填写滤镜处理后的实际分辨率并写入 ffprobe JSON。默认值来自滤镜编辑器的分辨率缩小值生成器。",
            ["ReviseSourceResolution.SettingsHeader"] = "新分辨率",
            ["ReviseSourceResolution.WidthLabel"] = "宽度",
            ["ReviseSourceResolution.HeightLabel"] = "高度",
            ["ReviseSourceResolution.CurrentLabel"] = "当前分析值",
            ["ReviseSourceResolution.SuggestedLabel"] = "分辨率缩小滑条",
            ["ReviseSourceResolution.UnknownResolution"] = "未知",
            ["ReviseSourceResolution.ResolutionFormat"] = "{0}x{1}",
            ["ReviseSourceResolution.Cancel"] = "取消",
            ["ReviseSourceResolution.Confirm"] = "更新 ffprobe JSON",
            ["ReviseSourceResolution.InvalidInput"] = "分辨率必须是 1 到 65535 之间的整数宽高。",
            ["ReviseSourceResolution.EvenResolutionHint"] = "分辨率的宽度和高度必须是偶数。",
            ["ReviseSourceResolution.NoFfprobeJson"] = "当前视频源没有可用的 ffprobe JSON。请先运行视频源分析。",
            ["ReviseSourceResolution.UpdateFailed"] = "更新 ffprobe JSON 失败：{0}",
        },
        ["zh-tw"] = new()
        {
            ["ReviseSourceResolution.Title"] = "1cenc 修訂源分辨率",
            ["ReviseSourceResolution.Description"] = "填寫濾鏡處理後的實際分辨率並寫入 ffprobe JSON。預設值來自濾鏡編輯器的分辨率縮小值生成器。",
            ["ReviseSourceResolution.SettingsHeader"] = "新分辨率",
            ["ReviseSourceResolution.WidthLabel"] = "寬度",
            ["ReviseSourceResolution.HeightLabel"] = "高度",
            ["ReviseSourceResolution.CurrentLabel"] = "目前分析值",
            ["ReviseSourceResolution.SuggestedLabel"] = "分辨率縮小滑條",
            ["ReviseSourceResolution.UnknownResolution"] = "未知",
            ["ReviseSourceResolution.ResolutionFormat"] = "{0}x{1}",
            ["ReviseSourceResolution.Cancel"] = "取消",
            ["ReviseSourceResolution.Confirm"] = "更新 ffprobe JSON",
            ["ReviseSourceResolution.InvalidInput"] = "分辨率必須是 1 到 65535 之間的整數寬高。",
            ["ReviseSourceResolution.EvenResolutionHint"] = "分辨率的寬度和高度必須是偶數。",
            ["ReviseSourceResolution.NoFfprobeJson"] = "目前影片源沒有可用的 ffprobe JSON。請先執行影片源分析。",
            ["ReviseSourceResolution.UpdateFailed"] = "更新 ffprobe JSON 失敗：{0}",
        },
    };

    static ReviseSourceResolutionModalLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["ReviseSourceResolution.Title"] = "1cenc Résolution source",
            ["ReviseSourceResolution.Description"] = "Définissez la résolution réelle après filtre à écrire dans le JSON ffprobe. La valeur par défaut vient du générateur de réduction de résolution.",
            ["ReviseSourceResolution.SettingsHeader"] = "Nouvelle résolution",
            ["ReviseSourceResolution.WidthLabel"] = "Largeur",
            ["ReviseSourceResolution.HeightLabel"] = "Hauteur",
            ["ReviseSourceResolution.CurrentLabel"] = "Analyse actuelle",
            ["ReviseSourceResolution.SuggestedLabel"] = "Curseur de réduction",
            ["ReviseSourceResolution.UnknownResolution"] = "Inconnue",
            ["ReviseSourceResolution.ResolutionFormat"] = "{0}x{1}",
            ["ReviseSourceResolution.Cancel"] = "Annuler",
            ["ReviseSourceResolution.Confirm"] = "Mettre à jour ffprobe JSON",
            ["ReviseSourceResolution.InvalidInput"] = "La résolution doit utiliser des dimensions entières de 1 à 65535.",
            ["ReviseSourceResolution.EvenResolutionHint"] = "La largeur et la hauteur de la résolution doivent être des nombres pairs.",
            ["ReviseSourceResolution.NoFfprobeJson"] = "Aucun JSON ffprobe n'est disponible pour la source actuelle. Lancez d'abord l'analyse de la source.",
            ["ReviseSourceResolution.UpdateFailed"] = "Échec de la mise à jour du JSON ffprobe : {0}",
        };
        Data["es"] = new(Data["en"])
        {
            ["ReviseSourceResolution.Title"] = "1cenc Resolución fuente",
            ["ReviseSourceResolution.Description"] = "Defina la resolución real después del filtro para escribirla en el JSON de ffprobe. El valor predeterminado viene del generador de reducción de resolución.",
            ["ReviseSourceResolution.SettingsHeader"] = "Nueva resolución",
            ["ReviseSourceResolution.WidthLabel"] = "Ancho",
            ["ReviseSourceResolution.HeightLabel"] = "Alto",
            ["ReviseSourceResolution.CurrentLabel"] = "Análisis actual",
            ["ReviseSourceResolution.SuggestedLabel"] = "Ctrl. de reducción",
            ["ReviseSourceResolution.UnknownResolution"] = "Desconocida",
            ["ReviseSourceResolution.ResolutionFormat"] = "{0}x{1}",
            ["ReviseSourceResolution.Cancel"] = "Cancelar",
            ["ReviseSourceResolution.Confirm"] = "Actualizar ffprobe JSON",
            ["ReviseSourceResolution.InvalidInput"] = "La resolución debe usar dimensiones enteras de 1 a 65535.",
            ["ReviseSourceResolution.EvenResolutionHint"] = "El ancho y el alto de la resolución deben ser números pares.",
            ["ReviseSourceResolution.NoFfprobeJson"] = "No hay JSON de ffprobe disponible para la fuente actual. Ejecute primero el análisis de fuente.",
            ["ReviseSourceResolution.UpdateFailed"] = "Error al actualizar JSON de ffprobe: {0}",
        };
        Data["ja"] = new(Data["en"])
        {
            ["ReviseSourceResolution.Title"] = "1cenc ソース解像度",
            ["ReviseSourceResolution.Description"] = "フィルタ後の実際の解像度を ffprobe JSON に書き込みます。初期値は解像度縮小ジェネレータからコピーされます。",
            ["ReviseSourceResolution.SettingsHeader"] = "新しい解像度",
            ["ReviseSourceResolution.WidthLabel"] = "幅",
            ["ReviseSourceResolution.HeightLabel"] = "高さ",
            ["ReviseSourceResolution.CurrentLabel"] = "現在の解析値",
            ["ReviseSourceResolution.SuggestedLabel"] = "解像度縮小スライダー",
            ["ReviseSourceResolution.UnknownResolution"] = "不明",
            ["ReviseSourceResolution.ResolutionFormat"] = "{0}x{1}",
            ["ReviseSourceResolution.Cancel"] = "キャンセル",
            ["ReviseSourceResolution.Confirm"] = "ffprobe JSON 更新",
            ["ReviseSourceResolution.InvalidInput"] = "解像度は 1 から 65535 までの整数の幅と高さで指定してください。",
            ["ReviseSourceResolution.EvenResolutionHint"] = "解像度の幅と高さは偶数である必要があります。",
            ["ReviseSourceResolution.NoFfprobeJson"] = "現在のソースに利用可能な ffprobe JSON がありません。先にソース解析を実行してください。",
            ["ReviseSourceResolution.UpdateFailed"] = "ffprobe JSON の更新に失敗しました: {0}",
        };
        Data["ru"] = new(Data["en"])
        {
            ["ReviseSourceResolution.Title"] = "1cenc Разрешение источника",
            ["ReviseSourceResolution.Description"] = "Задайте фактическое разрешение после фильтра для записи в JSON ffprobe. Значение по умолчанию берется из генератора уменьшения разрешения.",
            ["ReviseSourceResolution.SettingsHeader"] = "Новое разрешение",
            ["ReviseSourceResolution.WidthLabel"] = "Ширина",
            ["ReviseSourceResolution.HeightLabel"] = "Высота",
            ["ReviseSourceResolution.CurrentLabel"] = "Текущий анализ",
            ["ReviseSourceResolution.SuggestedLabel"] = "Ползунок уменьшения",
            ["ReviseSourceResolution.UnknownResolution"] = "Неизвестно",
            ["ReviseSourceResolution.ResolutionFormat"] = "{0}x{1}",
            ["ReviseSourceResolution.Cancel"] = "Отмена",
            ["ReviseSourceResolution.Confirm"] = "Обновить ffprobe JSON",
            ["ReviseSourceResolution.InvalidInput"] = "Разрешение должно состоять из целых размеров от 1 до 65535.",
            ["ReviseSourceResolution.EvenResolutionHint"] = "Ширина и высота разрешения должны быть четными числами.",
            ["ReviseSourceResolution.NoFfprobeJson"] = "Для текущего источника нет доступного JSON ffprobe. Сначала запустите анализ источника.",
            ["ReviseSourceResolution.UpdateFailed"] = "Не удалось обновить JSON ffprobe: {0}",
        };
    }

    private readonly Dictionary<string, string> _d;

    public static ReviseSourceResolutionModalLangProvider Current => new(UILangProvider.Current.LanguageCode);
    public string LanguageCode { get; }
    public string this[string key] => _d.TryGetValue(key, out var value) ? value : key;

    public ReviseSourceResolutionModalLangProvider(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];
    }
}
