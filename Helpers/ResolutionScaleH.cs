using System.Collections.Generic;

namespace OneColumnEncoder.Helpers;

public static class ResolutionScaleH
{
    private const int MinDimension = 16;

    public static (int width, int height) ComputeTargetDimensions(int sourceWidth, int sourceHeight, int scalePercent)
    {
        double ratio = scalePercent / 100.0;
        int w = (int)double.Ceiling(sourceWidth * ratio);
        int h = (int)double.Ceiling(sourceHeight * ratio);
        return (EnsureValid(w), EnsureValid(h));
    }

    public static int EnsureEven(int value) => value % 2 == 0 ? value : value + 1;

    public static int EnsureMin16(int value) => value < MinDimension ? MinDimension : value;

    public static int EnsureValid(int value) => EnsureEven(EnsureMin16(value));

    public static bool IsScaleApplicable(int width, int height) => width > MinDimension || height > MinDimension;

    public static List<string> GenerateTickLabels(int min, int max, int count)
    {
        var labels = new List<string>(count);
        for (int i = 0; i < count; i++)
        {
            int val = min + (max - min) * i / (count - 1);
            labels.Add($"{val}%");
        }
        return labels;
    }
}
