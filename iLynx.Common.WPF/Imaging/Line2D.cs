namespace iLynx.Common.WPF.Imaging
{
    public struct Line2D
    {
        public Line2D(VectorD start, VectorD stop)
            : this()
        {
            Start = start;
            Stop = stop;
        }

        public bool Equals(Line2D other)
        {
            return Start.Equals(other.Start) && Stop.Equals(other.Stop);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Line2D && Equals((Line2D)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ Stop.GetHashCode();
            }
        }

        public VectorD Start { get; set; }
        public VectorD Stop { get; set; }

        public double Angle
        {
            get { return (Stop - Start).GetAngle(); }
        }

        public double Length
        {
            get { return (Stop - Start).Abs().Magnitude(); }
        }

        public static bool operator ==(Line2D x, Line2D y)
        {
            return x.Start == y.Start && x.Stop == y.Stop;
        }

        public static bool operator !=(Line2D x, Line2D y)
        {
            return x.Start != y.Start || x.Stop != y.Stop;
        }

        public bool FindIntersection(Line2D other, out VectorD intersection)
        {
            /* http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
             * Now there are five cases:
             * If r × s = 0 and (q − p) × r = 0, then the two lines are collinear. If in addition, either 0 ≤ (q − p) · r ≤ r · r or 0 ≤ (p − q) · s ≤ s · s, then the two lines are overlapping.
             * If r × s = 0 and (q − p) × r = 0, but neither 0 ≤ (q − p) · r ≤ r · r nor 0 ≤ (p − q) · s ≤ s · s, then the two lines are collinear but disjoint.
             * If r × s = 0 and (q − p) × r ≠ 0, then the two lines are parallel and non-intersecting.
             * If r × s ≠ 0 and 0 ≤ t ≤ 1 and 0 ≤ u ≤ 1, the two line segments meet at the point p + t r = q + u s.
             * Otherwise, the two line segments are not parallel but do not intersect.
             */

            // line1.Start = p
            // line2.Start = q

            var r = Stop - Start;
            var s = other.Stop - other.Start;
            var startPointVector = (other.Start - Start);

            var denom = r.Cross(s); // denom = r × s

            var firstScalar = startPointVector.Cross(s) / denom; // firstScalar = t
            var secondScalar = startPointVector.Cross(r) / denom; // secondScalar = u
            intersection = Start + (firstScalar * r);
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return denom != 0d && firstScalar >= 0d && firstScalar <= 1d && secondScalar >= 0d && secondScalar <= 1d;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }
    }
}