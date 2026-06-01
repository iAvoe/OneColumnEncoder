using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OneColumnEncoder.Components
{
    /// <summary>
    /// Dynamic heatmap component. Currently designed for mempry usage details
    /// </summary>
    public class HeatmapGrid : Control
    {
        private const int DefaultColumns = 32;
        private const int DefaultRows = 16;
        private const int DefaultCellSize = 16;

        private static readonly Color[] Viridis =
        [
            Color.FromRgb(0x44, 0x01, 0x54),
            Color.FromRgb(0x48, 0x28, 0x78),
            Color.FromRgb(0x3E, 0x4A, 0x89),
            Color.FromRgb(0x31, 0x68, 0x8E),
            Color.FromRgb(0x21, 0x8F, 0x8B),
            Color.FromRgb(0x35, 0xB7, 0x79),
            Color.FromRgb(0x8F, 0xD7, 0x44),
            Color.FromRgb(0xDC, 0xE3, 0x19),
            Color.FromRgb(0xFD, 0xE7, 0x25),
        ];

        private Image? _image;
        private WriteableBitmap? _bitmap;

        static HeatmapGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HeatmapGrid),
                new FrameworkPropertyMetadata(typeof(HeatmapGrid)));
        }

        public int Columns
        {
            get => (int)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(int), typeof(HeatmapGrid),
                new PropertyMetadata(DefaultColumns, OnDimensionsChanged));

        public int Rows
        {
            get => (int)GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register(nameof(Rows), typeof(int), typeof(HeatmapGrid),
                new PropertyMetadata(DefaultRows, OnDimensionsChanged));

        public int CellSize
        {
            get => (int)GetValue(CellSizeProperty);
            set => SetValue(CellSizeProperty, value);
        }
        public static readonly DependencyProperty CellSizeProperty =
            DependencyProperty.Register(nameof(CellSize), typeof(int), typeof(HeatmapGrid),
                new PropertyMetadata(DefaultCellSize, OnDimensionsChanged));

        private static void OnDimensionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HeatmapGrid)d).RebuildBitmap();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _image = GetTemplateChild("PART_Image") as Image;

            RebuildBitmap();
        }

        private void RebuildBitmap()
        {
            if (_image is null) return;
            int w = Columns * CellSize;
            int h = Rows * CellSize;
            _bitmap = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
            _image.Source = _bitmap;
            Clear();
        }

        public void Clear()
        {
            if (_bitmap is null) return;
            int bg = 0xFF << 24;
            _bitmap.Lock();
            unsafe
            {
                int stride = _bitmap.BackBufferStride;
                int bpp = 4;
                byte* scan0 = (byte*)_bitmap.BackBuffer;
                int pixelCount = _bitmap.PixelWidth * _bitmap.PixelHeight;
                for (int i = 0; i < pixelCount; i++)
                {
                    *(int*)(scan0 + i * bpp) = bg;
                }
            }
            _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
            _bitmap.Unlock();
        }

        public void UpdateCell(int col, int row, double value)
        {
            if (_bitmap is null) return;
            if (col < 0 || col >= Columns || row < 0 || row >= Rows) return;
            if (value < 0) value = 0;
            if (value > 1) value = 1;

            Color c = InterpolateViridis(value);
            int px = col * CellSize;
            int py = row * CellSize;

            _bitmap.Lock();
            unsafe
            {
                int stride = _bitmap.BackBufferStride;
                int bpp = 4;
                byte* scan0 = (byte*)_bitmap.BackBuffer;
                for (int dy = 0; dy < CellSize; dy++)
                {
                    for (int dx = 0; dx < CellSize; dx++)
                    {
                        int offset = (py + dy) * stride + (px + dx) * bpp;
                        scan0[offset + 0] = c.B;
                        scan0[offset + 1] = c.G;
                        scan0[offset + 2] = c.R;
                        scan0[offset + 3] = c.A;
                    }
                }
            }
            _bitmap.AddDirtyRect(new Int32Rect(px, py, CellSize, CellSize));
            _bitmap.Unlock();
        }

        public void UpdateAll(double[,] values)
        {
            if (_bitmap is null) return;
            int rowsSrc = values.GetLength(0);
            int colsSrc = values.GetLength(1);

            _bitmap.Lock();
            unsafe
            {
                int stride = _bitmap.BackBufferStride;
                int bpp = 4;
                byte* scan0 = (byte*)_bitmap.BackBuffer;

                for (int row = 0; row < Rows; row++)
                {
                    for (int col = 0; col < Columns; col++)
                    {
                        double v = (row < rowsSrc && col < colsSrc)
                            ? values[row, col]
                            : 0;
                        if (v < 0) v = 0;
                        if (v > 1) v = 1;
                        Color c = InterpolateViridis(v);

                        int px = col * CellSize;
                        int py = row * CellSize;

                        for (int dy = 0; dy < CellSize; dy++)
                        {
                            for (int dx = 0; dx < CellSize; dx++)
                            {
                                int offset = (py + dy) * stride + (px + dx) * bpp;
                                scan0[offset + 0] = c.B;
                                scan0[offset + 1] = c.G;
                                scan0[offset + 2] = c.R;
                                scan0[offset + 3] = c.A;
                            }
                        }
                    }
                }
            }
            _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
            _bitmap.Unlock();
        }

        private static Color InterpolateViridis(double t)
        {
            int count = Viridis.Length;
            double segment = 1.0 / (count - 1);
            int idx = (int)(t / segment);
            if (idx >= count - 1) return Viridis[count - 1];

            double frac = (t - idx * segment) / segment;
            byte r = (byte)(Viridis[idx].R + (Viridis[idx + 1].R - Viridis[idx].R) * frac);
            byte g = (byte)(Viridis[idx].G + (Viridis[idx + 1].G - Viridis[idx].G) * frac);
            byte b = (byte)(Viridis[idx].B + (Viridis[idx + 1].B - Viridis[idx].B) * frac);
            return Color.FromRgb(r, g, b);
        }
    }
}
