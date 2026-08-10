namespace OneColumnEncoder.Models.Lang;

public class SrcReviserLangProvider(string languageCode) : LangProviderBase(languageCode, Data)
{
    public const string WindowTitle = "1cenc Source Reviser";
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["SrcReviser.Description"] = "Set the actual post-filter resolution to overwrite into ffprobe analysis",
            ["SrcReviser.SettingsHeader"] = "New Resolution",
            ["SrcReviser.WidthLabel"] = "Width",
            ["SrcReviser.HeightLabel"] = "Height",
            ["SrcReviser.CurrentLabel"] = "Current analysis",
            ["SrcReviser.SuggestedLabel"] = "Res. shrink slider",
            ["SrcReviser.UnknownResolution"] = "Unknown",
            ["SrcReviser.ResolutionFormat"] = "{0}x{1}",
            ["SrcReviser.Confirm"] = "Update ffprobe JSON",
            ["SrcReviser.InvalidInput"] = "Resolution must use integer dimensions from 1 to 65535",
            ["SrcReviser.EvenResolutionHint"] = "Resolution width and height must be even numbers",
            ["SrcReviser.NoFfprobeJson"] = "No ffprobe JSON is available for the current source. Run source analysis first",
            ["SrcReviser.UpdateFailed"] = "Failed to update ffprobe JSON: {0}",
        },
        ["zh-cn"] = new()
        {
            ["SrcReviser.Description"] = "填写滤镜处理后的实际分辨率以覆盖 ffprobe JSON",
            ["SrcReviser.SettingsHeader"] = "新分辨率",
            ["SrcReviser.WidthLabel"] = "宽度",
            ["SrcReviser.HeightLabel"] = "高度",
            ["SrcReviser.CurrentLabel"] = "当前分析值",
            ["SrcReviser.SuggestedLabel"] = "分辨率缩小滑条",
            ["SrcReviser.UnknownResolution"] = "未知",
            ["SrcReviser.ResolutionFormat"] = "{0}x{1}",
            ["SrcReviser.Confirm"] = "更新 ffprobe JSON",
            ["SrcReviser.InvalidInput"] = "分辨率必须是 1 到 65535 之间的整数宽高。",
            ["SrcReviser.EvenResolutionHint"] = "分辨率的宽度和高度必须是偶数。",
            ["SrcReviser.NoFfprobeJson"] = "当前视频源没有可用的 ffprobe JSON。请先运行视频源分析。",
            ["SrcReviser.UpdateFailed"] = "更新 ffprobe JSON 失败：{0}",
        },
        ["zh-tw"] = new()
        {
            ["SrcReviser.Description"] = "填寫濾鏡處理後的實際解析度以覆蓋 ffprobe JSON",
            ["SrcReviser.SettingsHeader"] = "新解析度",
            ["SrcReviser.WidthLabel"] = "寬度",
            ["SrcReviser.HeightLabel"] = "高度",
            ["SrcReviser.CurrentLabel"] = "當前分析值",
            ["SrcReviser.SuggestedLabel"] = "解析度縮小滑條",
            ["SrcReviser.UnknownResolution"] = "未知",
            ["SrcReviser.ResolutionFormat"] = "{0}x{1}",
            ["SrcReviser.Confirm"] = "更新 ffprobe JSON",
            ["SrcReviser.InvalidInput"] = "解析度必須是 1 到 65535 之間的整數寬高。",
            ["SrcReviser.EvenResolutionHint"] = "解析度的寬度和高度必須是偶數。",
            ["SrcReviser.NoFfprobeJson"] = "當前影片源沒有可用的 ffprobe JSON。請先運行影片源分析。",
            ["SrcReviser.UpdateFailed"] = "更新 ffprobe JSON 失敗：{0}",
        },
    };

    static SrcReviserLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["SrcReviser.Description"] = "Définissez la résolution réelle après filtrage à réécrire dans l'analyse ffprobe.",
            ["SrcReviser.SettingsHeader"] = "Nouvelle résolution",
            ["SrcReviser.WidthLabel"] = "Largeur",
            ["SrcReviser.HeightLabel"] = "Hauteur",
            ["SrcReviser.CurrentLabel"] = "Analyse actuelle",
            ["SrcReviser.SuggestedLabel"] = "Curseur de réduction",
            ["SrcReviser.Confirm"] = "Mettre à jour ffprobe JSON",
            ["SrcReviser.InvalidInput"] = "La résolution doit utiliser des dimensions entières de 1 à 65535",
            ["SrcReviser.EvenResolutionHint"] = "La largeur et la hauteur de la résolution doivent être des nombres pairs",
            ["SrcReviser.NoFfprobeJson"] = "Aucun JSON ffprobe n'est disponible pour la source actuelle. Lancez d'abord l'analyse de la source",
            ["SrcReviser.UpdateFailed"] = "Échec de la mise à jour du JSON ffprobe : {0}",
        };
        Data["es"] = new(Data["en"])
        {
            ["SrcReviser.Description"] = "Defina la resolución real después del filtro para sobrescribir el análisis de ffprobe.",
            ["SrcReviser.SettingsHeader"] = "Nueva resolución",
            ["SrcReviser.WidthLabel"] = "Ancho",
            ["SrcReviser.HeightLabel"] = "Alto",
            ["SrcReviser.CurrentLabel"] = "Análisis actual",
            ["SrcReviser.SuggestedLabel"] = "Ctrl. de reducción",
            ["SrcReviser.Confirm"] = "Actualizar ffprobe JSON",
            ["SrcReviser.InvalidInput"] = "La resolución debe usar dimensiones enteras de 1 a 65535",
            ["SrcReviser.EvenResolutionHint"] = "El ancho y el alto de la resolución deben ser números pares",
            ["SrcReviser.NoFfprobeJson"] = "No hay JSON de ffprobe disponible para la fuente actual. Ejecute primero el análisis de fuente",
            ["SrcReviser.UpdateFailed"] = "Error al actualizar JSON de ffprobe: {0}",
        };
        Data["ja"] = new(Data["en"])
        {
            ["SrcReviser.Description"] = "フィルタ後の実際の解像度を設定し、ffprobe の解析結果を上書きします。",
            ["SrcReviser.SettingsHeader"] = "新しい解像度",
            ["SrcReviser.WidthLabel"] = "幅",
            ["SrcReviser.HeightLabel"] = "高さ",
            ["SrcReviser.CurrentLabel"] = "現在の解析値",
            ["SrcReviser.SuggestedLabel"] = "解像度縮小スライダー",
            ["SrcReviser.Confirm"] = "ffprobe JSON 更新",
            ["SrcReviser.InvalidInput"] = "解像度は 1 から 65535 までの整数の幅と高さで指定してください。",
            ["SrcReviser.EvenResolutionHint"] = "解像度の幅と高さは偶数である必要があります。",
            ["SrcReviser.NoFfprobeJson"] = "現在のソースに利用可能な ffprobe JSON がありません。先にソース解析を実行してください。",
            ["SrcReviser.UpdateFailed"] = "ffprobe JSON の更新に失敗しました: {0}",
        };
        Data["ru"] = new(Data["en"])
        {
            ["SrcReviser.Description"] = "Задайте фактическое разрешение после фильтра, чтобы перезаписать анализ ffprobe.",
            ["SrcReviser.SettingsHeader"] = "Новое разрешение",
            ["SrcReviser.WidthLabel"] = "Ширина",
            ["SrcReviser.HeightLabel"] = "Высота",
            ["SrcReviser.CurrentLabel"] = "Текущий анализ",
            ["SrcReviser.SuggestedLabel"] = "Ползунок уменьшения",
            ["SrcReviser.Confirm"] = "Обновить ffprobe JSON",
            ["SrcReviser.InvalidInput"] = "Разрешение должно состоять из целых размеров от 1 до 65535",
            ["SrcReviser.EvenResolutionHint"] = "Ширина и высота разрешения должны быть чётными числами",
            ["SrcReviser.NoFfprobeJson"] = "Для текущего источника нет доступного JSON ffprobe. Сначала запустите анализ источника",
            ["SrcReviser.UpdateFailed"] = "Не удалось обновить JSON ffprobe: {0}",
        };
        Data["de"] = new(Data["en"])
        {
            ["SrcReviser.Description"] = "Tatsächliche Auflösung nach Filtern eingeben, um ffprobe-Analyse zu überschreiben",
            ["SrcReviser.SettingsHeader"] = "Neue Auflösung",
            ["SrcReviser.WidthLabel"] = "Breite",
            ["SrcReviser.HeightLabel"] = "Höhe",
            ["SrcReviser.CurrentLabel"] = "Aktuelle Analyse",
            ["SrcReviser.SuggestedLabel"] = "Verkleinerungs-Schieberegler",
            ["SrcReviser.InvalidInput"] = "Auflösung muss ganzzahlige Abmessungen von 1 bis 65535 verwenden",
            ["SrcReviser.EvenResolutionHint"] = "Breite und Höhe der Auflösung müssen gerade Zahlen sein",
            ["SrcReviser.NoFfprobeJson"] = "Kein ffprobe-JSON für die aktuelle Quelle verfügbar. Zuerst Quellanalyse ausführen",
            ["SrcReviser.UpdateFailed"] = "ffprobe-JSON-Update fehlgeschlagen: {0}",
        };
        Data["ko"] = new(Data["en"])
        {
            ["SrcReviser.Description"] = "ffprobe 분석에 덮어쓸 필터 적용 후의 실제 해상도를 설정합니다",
            ["SrcReviser.SettingsHeader"] = "새 해상도",
            ["SrcReviser.WidthLabel"] = "너비",
            ["SrcReviser.HeightLabel"] = "높이",
            ["SrcReviser.CurrentLabel"] = "현재 분석 값",
            ["SrcReviser.SuggestedLabel"] = "해상도 축소 슬라이더",
            ["SrcReviser.UnknownResolution"] = "알 수 없음",
            ["SrcReviser.ResolutionFormat"] = "{0}x{1}",
            ["SrcReviser.Confirm"] = "ffprobe JSON 업데이트",
            ["SrcReviser.InvalidInput"] = "해상도는 1~65535 사이의 정수 치수여야 합니다",
            ["SrcReviser.EvenResolutionHint"] = "해상도의 너비와 높이는 짝수여야 합니다",
            ["SrcReviser.NoFfprobeJson"] = "현재 소스에 사용할 수 있는 ffprobe JSON이 없습니다. 먼저 소스 분석을 실행하세요",
            ["SrcReviser.UpdateFailed"] = "ffprobe JSON 업데이트 실패: {0}",
        };
        Data["pt-br"] = new(Data["en"])
        {
            ["SrcReviser.Description"] = "Defina a resolução real após o filtro para sobrescrever na análise ffprobe",
            ["SrcReviser.SettingsHeader"] = "Nova resolução",
            ["SrcReviser.WidthLabel"] = "Largura",
            ["SrcReviser.HeightLabel"] = "Altura",
            ["SrcReviser.CurrentLabel"] = "Análise atual",
            ["SrcReviser.SuggestedLabel"] = "Controle de redução",
            ["SrcReviser.Confirm"] = "Atualizar ffprobe JSON",
            ["SrcReviser.InvalidInput"] = "A resolução deve usar dimensões inteiras de 1 a 65535",
            ["SrcReviser.EvenResolutionHint"] = "A largura e altura da resolução devem ser números pares",
            ["SrcReviser.NoFfprobeJson"] = "Nenhum ffprobe JSON disponível para a fonte atual. Execute a análise da fonte primeiro",
            ["SrcReviser.UpdateFailed"] = "Falha ao atualizar ffprobe JSON: {0}",
        };
    }

    public static SrcReviserLangProvider Current => new(UILangProvider.Current.LanguageCode);
}
