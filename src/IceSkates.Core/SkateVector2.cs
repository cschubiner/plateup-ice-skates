using System;

namespace IceSkates.Core;

public readonly struct SkateVector2 : IEquatable<SkateVector2>
{
    public static readonly SkateVector2 Zero = new SkateVector2(0f, 0f);

    public SkateVector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float X { get; }
    public float Y { get; }
    public float Magnitude => (float)Math.Sqrt((X * X) + (Y * Y));

    public SkateVector2 Normalized()
    {
        float magnitude = Magnitude;
        return magnitude <= 0.0001f ? Zero : new SkateVector2(X / magnitude, Y / magnitude);
    }

    public SkateVector2 ClampMagnitude(float maxMagnitude)
    {
        if (maxMagnitude <= 0f)
        {
            return Zero;
        }

        float magnitude = Magnitude;
        return magnitude <= maxMagnitude ? this : Normalized() * maxMagnitude;
    }

    public static SkateVector2 Lerp(SkateVector2 from, SkateVector2 to, float t)
    {
        t = SkateMath.Clamp(t, 0f, 1f);
        return new SkateVector2(from.X + ((to.X - from.X) * t), from.Y + ((to.Y - from.Y) * t));
    }

    public static SkateVector2 operator +(SkateVector2 left, SkateVector2 right) => new SkateVector2(left.X + right.X, left.Y + right.Y);
    public static SkateVector2 operator -(SkateVector2 left, SkateVector2 right) => new SkateVector2(left.X - right.X, left.Y - right.Y);
    public static SkateVector2 operator *(SkateVector2 value, float scalar) => new SkateVector2(value.X * scalar, value.Y * scalar);
    public static SkateVector2 operator *(float scalar, SkateVector2 value) => value * scalar;

    public bool Equals(SkateVector2 other) => X.Equals(other.X) && Y.Equals(other.Y);
    public override bool Equals(object? obj) => obj is SkateVector2 other && Equals(other);
    public override int GetHashCode() => (X.GetHashCode() * 397) ^ Y.GetHashCode();
    public override string ToString() => $"({X:0.###}, {Y:0.###})";
}
