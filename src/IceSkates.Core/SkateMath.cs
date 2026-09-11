namespace IceSkates.Core;

internal static class SkateMath
{
    public static float Clamp(float value, float min, float max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }
}
