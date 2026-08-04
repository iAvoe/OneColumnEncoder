namespace OneColumnEncoder.Models.Lang;

public class SourceReviserLangProvider
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["SourceReviser.Description"] = "Set the actual post-filter resolution to overwrite into ffprobe analysis",
            ["SourceReviser.SettingsHeader"] = "New Resolution",
            ["SourceReviser.WidthLabel"] = "Width",
            ["SourceReviser.HeightLabel"] = "Height",
            ["SourceReviser.CurrentLabel"] = "Current analysis",
            ["SourceReviser.SuggestedLabel"] = "Res. shrink slider",
            ["SourceReviser.UnknownResolution"] = "Unknown",
            ["SourceReviser.ResolutionFormat"] = "{0}x{1}",
            ["SourceReviser.Cancel"] = "Cancel",
            ["SourceReviser.Confirm"] = "Update ffprobe JSON",
            ["SourceReviser.InvalidInput"] = "Resolution must use integer dimensions from 1 to 65535",
            ["SourceReviser.EvenResolutionHint"] = "Resolution width and height must be even numbers",
            ["SourceReviser.NoFfprobeJson"] = "No ffprobe JSON is available for the current source. Run source analysis first",
            ["SourceReviser.UpdateFailed"] = "Failed to update ffprobe JSON: {0}",
        },
        ["zh-cn"] = new()
        {
            ["SourceReviser.Description"] = "填写滤镜处理后的实际分辨率以覆盖 ffprobe JSON",
            ["SourceReviser.SettingsHeader"] = "新分辨率",
            ["SourceReviser.WidthLabel"] = "宽度",
            ["SourceReviser.HeightLabel"] = "高度",
            ["SourceReviser.CurrentLabel"] = "当前分析值",
            ["SourceReviser.SuggestedLabel"] = "分辨率缩小滑条",
            ["SourceReviser.UnknownResolution"] = "未知",
            ["SourceReviser.ResolutionFormat"] = "{0}x{1}",
            ["SourceReviser.Cancel"] = "取消",
            ["SourceReviser.Confirm"] = "更新 ffprobe JSON",
            ["SourceReviser.InvalidInput"] = "分辨率必须是 1 到 65535 之间的整数宽高。",
            ["SourceReviser.EvenResolutionHint"] = "分辨率的宽度和高度必须是偶数。",
            ["SourceReviser.NoFfprobeJson"] = "当前视频源没有可用的 ffprobe JSON。请先运行视频源分析。",
            ["SourceReviser.UpdateFailed"] = "更新 ffprobe JSON 失败：{0}",
        },
        ["zh-tw"] = new()
        {
            ["SourceReviser.Description"] = "填寫濾鏡處理後的實際解析度以覆蓋 ffprobe JSON",
            ["SourceReviser.SettingsHeader"] = "新解析度",
            ["SourceReviser.WidthLabel"] = "寬度",
            ["SourceReviser.HeightLabel"] = "高度",
            ["SourceReviser.CurrentLabel"] = "當前分析值",
            ["SourceReviser.SuggestedLabel"] = "解析度縮小滑條",
            ["SourceReviser.UnknownResolution"] = "未知",
            ["SourceReviser.ResolutionFormat"] = "{0}x{1}",
            ["SourceReviser.Cancel"] = "取消",
            ["SourceReviser.Confirm"] = "更新 ffprobe JSON",
            ["SourceReviser.InvalidInput"] = "解析度必須是 1 到 65535 之間的整數寬高。",
            ["SourceReviser.EvenResolutionHint"] = "解析度的寬度和高度必須是偶數。",
            ["SourceReviser.NoFfprobeJson"] = "當前影片源沒有可用的 ffprobe JSON。請先運行影片源分析。",
            ["SourceReviser.UpdateFailed"] = "更新 ffprobe JSON 失敗：{0}",
        },
    };

    static SourceReviserLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["SourceReviser.Description"] = "Définissez la résolution réelle après filtrage à réécrire dans l'analyse ffprobe.",
            ["SourceReviser.SettingsHeader"] = "Nouvelle résolution",
            ["SourceReviser.WidthLabel"] = "Largeur",
            ["SourceReviser.HeightLabel"] = "Hauteur",
            ["SourceReviser.CurrentLabel"] = "Analyse actuelle",
            ["SourceReviser.SuggestedLabel"] = "Curseur de réduction",
            ["SourceReviser.Cancel"] = "Annuler",
            ["SourceReviser.Confirm"] = "Mettre à jour ffprobe JSON",
            ["SourceReviser.InvalidInput"] = "La résolution doit utiliser des dimensions entières de 1 à 65535",
            ["SourceReviser.EvenResolutionHint"] = "La largeur et la hauteur de la résolution doivent être des nombres pairs",
            ["SourceReviser.NoFfprobeJson"] = "Aucun JSON ffprobe n'est disponible pour la source actuelle. Lancez d'abord l'analyse de la source",
            ["SourceReviser.UpdateFailed"] = "Échec de la mise à jour du JSON ffprobe : {0}",
        };
        Data["es"] = new(Data["en"])
        {
            ["SourceReviser.Description"] = "Defina la resolución real después del filtro para sobrescribir el análisis de ffprobe.",
            ["SourceReviser.SettingsHeader"] = "Nueva resolución",
            ["SourceReviser.WidthLabel"] = "Ancho",
            ["SourceReviser.HeightLabel"] = "Alto",
            ["SourceReviser.CurrentLabel"] = "Análisis actual",
            ["SourceReviser.SuggestedLabel"] = "Ctrl. de reducción",
            ["SourceReviser.Cancel"] = "Cancelar",
            ["SourceReviser.Confirm"] = "Actualizar ffprobe JSON",
            ["SourceReviser.InvalidInput"] = "La resolución debe usar dimensiones enteras de 1 a 65535",
            ["SourceReviser.EvenResolutionHint"] = "El ancho y el alto de la resolución deben ser números pares",
            ["SourceReviser.NoFfprobeJson"] = "No hay JSON de ffprobe disponible para la fuente actual. Ejecute primero el análisis de fuente",
            ["SourceReviser.UpdateFailed"] = "Error al actualizar JSON de ffprobe: {0}",
        };
        Data["ja"] = new(Data["en"])
        {
            ["SourceReviser.Description"] = "フィルタ後の実際の解像度を設定し、ffprobe の解析結果を上書きします。",
            ["SourceReviser.SettingsHeader"] = "新しい解像度",
            ["SourceReviser.WidthLabel"] = "幅",
            ["SourceReviser.HeightLabel"] = "高さ",
            ["SourceReviser.CurrentLabel"] = "現在の解析値",
            ["SourceReviser.SuggestedLabel"] = "解像度縮小スライダー",
            ["SourceReviser.Cancel"] = "キャンセル",
            ["SourceReviser.Confirm"] = "ffprobe JSON 更新",
            ["SourceReviser.InvalidInput"] = "解像度は 1 から 65535 までの整数の幅と高さで指定してください。",
            ["SourceReviser.EvenResolutionHint"] = "解像度の幅と高さは偶数である必要があります。",
            ["SourceReviser.NoFfprobeJson"] = "現在のソースに利用可能な ffprobe JSON がありません。先にソース解析を実行してください。",
            ["SourceReviser.UpdateFailed"] = "ffprobe JSON の更新に失敗しました: {0}",
        };
        Data["ru"] = new(Data["en"])
        {
            ["SourceReviser.Description"] = "Задайте фактическое разрешение после фильтра, чтобы перезаписать анализ ffprobe.",
            ["SourceReviser.SettingsHeader"] = "Новое разрешение",
            ["SourceReviser.WidthLabel"] = "Ширина",
            ["SourceReviser.HeightLabel"] = "Высота",
            ["SourceReviser.CurrentLabel"] = "Текущий анализ",
            ["SourceReviser.SuggestedLabel"] = "Ползунок уменьшения",
            ["SourceReviser.Cancel"] = "Отмена",
            ["SourceReviser.Confirm"] = "Обновить ffprobe JSON",
            ["SourceReviser.InvalidInput"] = "Разрешение должно состоять из целых размеров от 1 до 65535",
            ["SourceReviser.EvenResolutionHint"] = "Ширина и высота разрешения должны быть чётными числами",
            ["SourceReviser.NoFfprobeJson"] = "Для текущего источника нет доступного JSON ffprobe. Сначала запустите анализ источника",
            ["SourceReviser.UpdateFailed"] = "Не удалось обновить JSON ffprobe: {0}",
        };
    }

    private readonly Dictionary<string, string> _d;
    public static SourceReviserLangProvider Current => new(UILangProvider.Current.LanguageCode);
    public string LanguageCode { get; }
    public string this[string key] => _d.TryGetValue(key, out var value)
        ? value
        : Data["en"].TryGetValue(key, out var enVal) ? enVal : key;

    public SourceReviserLangProvider(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];
    }
}
