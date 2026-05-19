using System.Windows;
using System.Windows.Media;

namespace OneColumnEncoder.Helpers;

internal static class SvgIconProvider
{
    public static ImageSource GlobeWarning { get; }
    public static ImageSource GlobeError { get; }
    public static ImageSource Troubleshoot { get; }

    private static Brush Brush(string hex) =>
        new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)!);

    private static void Add(DrawingGroup g, string path, Brush brush) =>
        g.Children.Add(new GeometryDrawing(brush, null, Geometry.Parse(path)));

    private static void Add(DrawingGroup g, Geometry geometry, Brush brush) =>
        g.Children.Add(new GeometryDrawing(brush, null, geometry));

    static SvgIconProvider()
    {
        var gray999 = Brush("#999");
        var graya3 = Brush("#a3a3a3");
        var orange = Brush("#F68819");
        var white = Brush("#f2f2f2");
        var red = Brush("#95000E");
        var blue1 = Brush("#198ab3");
        var blue2 = Brush("#32d4f5");

        var w = new DrawingGroup();
        Add(w, "M9.3,14.752c0-.154-.613-.154-.613,0a1.656,1.656,0,0,1-1.636,1.8h3.882A1.655,1.655,0,0,1,9.3,14.752Z", gray999);
        Add(w, "M13.335,1.1a.571.571,0,0,1,.807,0l.026.028A8.32,8.32,0,0,1,2.432,12.858a.57.57,0,0,1-.049-.8l.025-.026h0A.571.571,0,0,1,3.19,12,7.178,7.178,0,0,0,13.312,1.882.573.573,0,0,1,13.335,1.1Z", graya3);
        Add(w, "M11.946,17.5h-5.9a.476.476,0,0,1-.476-.476h0a.476.476,0,0,1,.476-.476h5.9a.476.476,0,0,1,.475.476h0A.476.476,0,0,1,11.946,17.5Z", graya3);
        Add(w, new EllipseGeometry(new Point(7.871, 6.563), 6.063, 6.063), orange);
        Add(w, "M4.15,9.515h7.562a.293.293,0,0,0,.251-.443L8.183,2.717a.293.293,0,0,0-.5,0L3.9,9.072A.293.293,0,0,0,4.15,9.515Z", white);
        Add(w, "M8.254,7.571H7.608a.146.146,0,0,1-.152-.134L7.387,4.491a.144.144,0,0,1,.151-.14h.786a.144.144,0,0,1,.151.14L8.406,7.437A.146.146,0,0,1,8.254,7.571Z", orange);
        Add(w, new EllipseGeometry(new Point(7.931, 8.451), 0.516, 0.516), orange);
        GlobeWarning = new DrawingImage(w);

        var e = new DrawingGroup();
        Add(e, "M9.3,14.752c0-.154-.613-.154-.613,0a1.656,1.656,0,0,1-1.636,1.8h3.882A1.655,1.655,0,0,1,9.3,14.752Z", gray999);
        Add(e, "M13.335,1.1a.571.571,0,0,1,.807,0l.026.028A8.32,8.32,0,0,1,2.432,12.858a.57.57,0,0,1-.049-.8l.025-.026h0A.571.571,0,0,1,3.19,12,7.178,7.178,0,0,0,13.312,1.882.573.573,0,0,1,13.335,1.1Z", graya3);
        Add(e, "M11.946,17.5h-5.9a.476.476,0,0,1-.476-.476h0a.476.476,0,0,1,.476-.476h5.9a.476.476,0,0,1,.475.476h0A.476.476,0,0,1,11.946,17.5Z", graya3);
        Add(e, new EllipseGeometry(new Point(7.871, 6.563), 6.063, 6.063), red);
        Add(e, "M8.391,8.105H7.256a.255.255,0,0,1-.265-.236L6.869,2.705a.254.254,0,0,1,.266-.246H8.512a.254.254,0,0,1,.266.246L8.656,7.869A.255.255,0,0,1,8.391,8.105Z", white);
        Add(e, new EllipseGeometry(new Point(7.823, 9.647), 0.905, 0.905), white);
        GlobeError = new DrawingImage(e);

        var t = new DrawingGroup();
        Add(t, "M14.37,4.17l.08.07a.14.14,0,0,0,.19,0l.74-.62,1.34-2.21a.14.14,0,0,0,0-.17L16.49,1a.14.14,0,0,0-.17,0L14.21,2.43l-.6.77a.14.14,0,0,0,0,.18l.08.08L9.33,8,8.43,7l-1,1A1.81,1.81,0,0,1,7,9.51a1.57,1.57,0,0,1-1.37.5L1.34,14.36a.27.27,0,0,0,0,.39l2.08,2.16a.27.27,0,0,0,.4,0L8,12.55a1.72,1.72,0,0,1,.5-1.44,1.6,1.6,0,0,1,1.37-.5l1-1L10,8.64Z", blue1);
        Add(t, "M16.1,14.07l-1-1.07L8.42,6.15h0l-.33-.36a3.72,3.72,0,0,0-.87-3.64,3.43,3.43,0,0,0-3.09-1,.15.15,0,0,0-.08.24L5.79,3.16a.15.15,0,0,1,0,.14L5.37,5.11a.14.14,0,0,1-.1.1L3.49,5.7a.14.14,0,0,1-.14,0L1.66,3.91A.14.14,0,0,0,1.42,4a3.63,3.63,0,0,0,1,3.18,3.37,3.37,0,0,0,3.37.9l.06.07.38.39h0l7.61,8a1.58,1.58,0,0,0,2.22.08l.08-.08a1.68,1.68,0,0,0,.47-1.2A1.85,1.85,0,0,0,16.1,14.07Z", blue2);
        Troubleshoot = new DrawingImage(t);
    }
}
