using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Windows;

namespace iLynx.Common.WPF.Imaging
{
    public struct VectorD
    {
        public bool Equals(VectorD other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is VectorD && Equals((VectorD)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public static implicit operator VectorD(Point p)
        {
            return new VectorD(p.X, p.Y);
        }

        /// <summary>
        /// The X
        /// </summary>
        public readonly double X;

        /// <summary>
        /// The Y
        /// </summary>
        public readonly double Y;

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorD" /> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public VectorD(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Froms the distance.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <param name="distance">The distance.</param>
        /// <returns></returns>
        public static VectorD FromDistance(double angle, double distance)
        {
            return new VectorD(Math.Sin(angle) * distance, Math.Cos(angle) * distance);
        }

        private const double Deg2Rad = Math.PI / 180d;

        /// <summary>
        /// Rotates the degrees.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorD RotateDegrees(double angle)
        {
            return RotateRadians(angle * Deg2Rad);
        }

        /// <summary>
        /// Gets the angle.
        /// </summary>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetAngle()
        {
            return Math.Atan(Y / X);
        }

        /// <summary>
        /// Rotates to.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorD RotateTo(double angle)
        {
            var current = GetAngle();
            var delta = angle - current;
            return RotateDegrees(delta);
        }

        /// <summary>
        /// Rotates to zero.
        /// </summary>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorD RotateToZero()
        {
            return RotateDegrees(-GetAngle());
        }

        /// <summary>
        /// Rotates the radians.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorD RotateRadians(double angle)
        {
            // 2D Rotation
            // ---------------------------   -----   ----------------------------------
            // | Cos(angle), -Sin(angle) |   | X |   | X*Cos(angle) - Y*Sin(angle)    |
            // |                         | * |   | = |                                |
            // | Sin(angle), Cos(angle)  |   | Y |   | X*Sin(angle) + Y*Cos(angle)    |
            // ---------------------------   -----   ----------------------------------
            return new VectorD(
                (Math.Cos(angle) * X) - (Math.Sin(angle) * Y),
                (Math.Cos(angle) * Y) + (Math.Sin(angle) * X)
                );
        }

        /// <summary>
        /// Rounds this instance.
        /// </summary>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorI Round()
        {
            return new VectorI((int)Math.Round(X), (int)Math.Round(Y));
        }

        /// <summary>
        /// Floors this instance.
        /// </summary>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorI Floor()
        {
            return new VectorI((int)Math.Floor(X), (int)Math.Floor(Y));
        }

        /// <summary>
        /// Ceilings this instance.
        /// </summary>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorI Ceiling()
        {
            return new VectorI((int)Math.Ceiling(X), (int)Math.Ceiling(Y));
        }

        /// <summary>
        /// Scales the by vector.
        /// </summary>
        /// <param name="scale">The scale.</param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorD Scale(VectorD scale)
        {
            return new VectorD(X * scale.X, Y * scale.Y);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(@"{0:F2},{1:F2}", X, Y);
        }

        /// <summary>
        /// Gets the magnitude of this vector
        /// </summary>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Magnitude()
        {
            return Math.Sqrt((X * X) + (Y * Y));
        }

        /// <summary>
        /// Computes the cross product of this vector and another.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Cross(VectorD other)
        {
            return X * other.Y - Y * other.X;
        }

        #region Operator Overloads

        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(VectorD v1, VectorD v2)
        {
            return Math.Abs(v1.X - v2.X) < double.Epsilon && Math.Abs(v1.Y - v2.Y) < double.Epsilon;
        }

        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(VectorD v1, VectorD v2)
        {
            return !(v1 == v2);
        }

        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator *(VectorD v, double scalar)
        {
            return new VectorD(v.X * scalar, v.Y * scalar);
        }

        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator *(double scalar, VectorD v)
        {
            return v * scalar;
        }

        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator /(VectorD v, double scalar)
        {
            return new VectorD(v.X / scalar, v.Y / scalar);
        }

        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator /(double scalar, VectorD v)
        {
            return new VectorD(scalar / v.X, scalar / v.Y);
        }

        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator +(VectorD v1, VectorD v2)
        {
            return new VectorD(v1.X + v2.X, v1.Y + v2.Y);
        }

        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator +(VectorD v1, VectorI v2)
        {
            return new VectorD(v1.X + v2.X, v1.Y + v2.Y);
        }

        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator -(VectorD v1, VectorD v2)
        {
            return new VectorD(v1.X - v2.X, v1.Y - v2.Y);
        }

        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator -(VectorD v1, VectorI v2)
        {
            return new VectorD(v1.X - v2.X, v1.Y - v2.Y);
        }
        #endregion

        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorD Abs()
        {
            return new VectorD(Math.Abs(X), Math.Abs(Y));
        }

        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNaN()
        {
            return double.IsNaN(X) || double.IsNaN(Y);
        }

        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorD Normalize()
        {
            var mag = Magnitude();
            return new VectorD(X / mag, Y / mag);
        }
    }
}