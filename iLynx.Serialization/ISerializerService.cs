using System;
using System.IO;

namespace iLynx.Serialization
{
    /// <summary>
    /// ISerializerService
    /// </summary>
    public interface ISerializerService
    {
        /// <summary>
        /// Deserializes the specified source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        T Deserialize<T>(Stream source) where T : new();

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <param name="target">The target.</param>
        void Serialize<T>(T item, Stream target) where T : new();

        /// <summary>
        /// Attempts to find a serializer for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ISerializer<T> GetSerializer<T>();

        /// <summary>
        /// Attempts to find a serializer for the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        ISerializer GetSerializer(Type type);
    }
}