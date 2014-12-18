using System;
using System.IO;
using iLynx.Common;

namespace iLynx.Serialization
{
    public abstract class SerializerServiceBase : ISerializerService
    {
        /// <summary>
        /// The namespace GUID used by this serializer
        /// </summary>
        public static readonly Guid SerializerNamespace = new Guid("6CB4F946-4563-4458-938A-56E5DAB8640F");

        public abstract void AddOverride<T>(ISerializer<T> serializer);
        
        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <param name="target">The target.</param>
        public virtual void Serialize<T>(T item, Stream target) where T : new()
        {
            var serializer = FindSerializer<T>();
            if (null == serializer)
            {
                OnGetSerializerError<T>();
                return;
            }
            serializer.Serialize(item, target);
        }
        
        /// <summary>
        /// Deserializes the specified source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public virtual T Deserialize<T>(Stream source) where T : new()
        {
            var serializer = FindSerializer<T>();
            if (null != serializer) return serializer.Deserialize(source);
            OnGetSerializerError<T>();
            return default(T);
        }

        protected virtual void OnGetSerializerError<T>()
        {
            this.LogError("Something went wrong... Tried to get Serializer for Type: {0}, but was unsuccesful", typeof(T));
        }

        public abstract ISerializer FindSerializer(Type type);
        public abstract ISerializer<T> FindSerializer<T>();
    }
}