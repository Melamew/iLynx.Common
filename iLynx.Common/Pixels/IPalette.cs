namespace iLynx.Common.Pixels
{
    public interface IPalette
    {
        void RemoveValue(double sampleValue);
        void MapValue(double sampleValue, byte[] colour);
        double MinValue { get; }
        double MaxValue { get; }
        void MapValue(double sampleValue, byte a, byte r, byte g, byte b);
        void MapValue(double sampleValue, int colour);
        int GetColour(double sampleValue);
    }
}