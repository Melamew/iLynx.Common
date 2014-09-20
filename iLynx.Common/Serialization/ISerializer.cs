using System.IO;

namespace iLynx.Common.Serialization
{
    /// <summary>
    /// ISerializer
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="target">The target.</param>
        void Serialize(object item, Stream target);

        /// <summary>
        /// Deserializes the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        object Deserialize(Stream source);

        /// <summary>
        /// Gets the expected number of bytes that this serializer would output for the specified item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        int GetOutputSize(object item);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISerializer<T> : ISerializer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        new T Deserialize(Stream source);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="target"></param>
        void Serialize(T item, Stream target);

        /// <summary>
        /// Gets the expected number of bytes that this serializer would output for the specified item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        int GetOutputSize(T item);
    }
}
