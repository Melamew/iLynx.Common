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
        T Deserialize<T>(Stream source);

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <param name="target">The target.</param>
        void Serialize<T>(T item, Stream target);

        ///// <summary>
        ///// Adds the specified serializer as an 'override'
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="serializer"></param>
        //void AddOverride<T>(ISerializer<T> serializer);

        //ISerializer FindSerializer(Type type);

        //ISerializer<T> FindSerializer<T>();
    }
}