using System;
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
        void RemapValue(T oldValue, T newValue);
        int GetColour(T sampleValue);
        byte[] GetColourBytes(T sampleValue);
        IPalette<T> AsFrozen();
        Tuple<T, Color>[] GetMap();
        void FromMap(Tuple<T, Color>[] values);
        bool Contains(T value);
    }
}