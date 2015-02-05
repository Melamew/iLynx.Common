using System.Windows.Media;

namespace iLynx.Common.Pixels
{
    public interface IPalette<T>
    {
        void RemoveValue(T sampleValue);
        void MapValue(T sampleValue, byte[] colour);
        T MinValue { get; }
        T MaxValue { get; }
        void MapValue(T sampleValue, byte a, byte r, byte g, byte b);
        void MapValue(T sampleValue, int colour);
        void MapValue(T sampleValue, Color colour);
        int GetColour(T sampleValue);
        byte[] GetColourBytes(T sampleValue);
    }
}