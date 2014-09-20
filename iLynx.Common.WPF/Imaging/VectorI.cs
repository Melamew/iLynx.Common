using System;

namespace iLynx.Common.WPF.Imaging
{
    public struct VectorI
    {
        public readonly int X;
        public readonly int Y;
        public VectorI(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int Magnitude()
        {
            return (int)Math.Round(Math.Sqrt((X * X) + (Y * Y)));
        }

        public static implicit operator VectorD(VectorI v)
        {
            return new VectorD(v.X, v.Y);
        }

        public static VectorI operator *(VectorI v, int scalar)
        {
            return new VectorI(v.X * scalar, v.Y * scalar);
        }

        public static VectorI operator /(VectorI v, int scalar)
        {
            return new VectorI(v.X / scalar, v.Y / scalar);
        }

        public static VectorI operator +(VectorI v1, VectorI v2)
        {
            return new VectorI(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static VectorI operator +(VectorI v1, VectorD v2)
        {
            return new VectorI(v1.X + (int)Math.Round(v2.X), v1.Y + (int)Math.Round(v2.Y));
        }

        public static VectorI operator -(VectorI v1, VectorI v2)
        {
            return new VectorI(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static VectorI operator -(VectorI v1, VectorD v2)
        {
            return new VectorI(v1.X - (int)Math.Round(v2.X), v1.Y - (int)Math.Round(v2.Y));
        }
    }
}