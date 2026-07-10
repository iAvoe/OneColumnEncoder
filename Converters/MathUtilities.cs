namespace OneColumnEncoder.Converters;

public static class MathUtilities
{
    public static long GreatestCommonDivisor(long a, long b)
    {
        while (b != 0)
        {
            long t = a % b;
            a = b;
            b = t;
        }

        return a == 0 ? 1 : a;
    }
}
