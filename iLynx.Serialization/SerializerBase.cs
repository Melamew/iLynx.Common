using System.IO;

namespace iLynx.Serialization
{
    public abstract class SerializerBase<T> : ISerializer<T>
    {
        public void Serialize(object item, Stream target)
        {
            Serialize((T) item, target);
        }

        public abstract T Deserialize(Stream source);
        public abstract void Serialize(T item, Stream target);
        public abstract int GetOutputSize(T item);

        object ISerializer.Deserialize(Stream source)
        {
            return Deserialize(source);
        }

        public int GetOutputSize(object item)
        {
            return GetOutputSize((T) item);
        }
    }
}