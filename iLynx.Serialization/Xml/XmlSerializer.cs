using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace iLynx.Serialization.Xml
{
    public class XmlSerializerService : SerializerServiceBase
    {
        private static readonly Dictionary<Type, XmlSerializerBase> Overrides = new Dictionary<Type, XmlSerializerBase>();

        public override void Serialize<T>(T item, Stream target)
        {
            var serializer = GetSerializer<T>();
            using (var writer = XmlWriter.Create(target))
            {
                writer.WriteStartElement(typeof (T).Name);
                serializer.Serialize(item, writer);
                writer.WriteEndElement();
            }
        }
        
        public override T Deserialize<T>(Stream source)
        {
            var serializer = GetSerializer<T>();
            using (var reader = XmlReader.Create(source))
            {
                reader.ReadStartElement(typeof(T).Name);
                var result = serializer.Deserialize(reader);
                reader.ReadEndElement();
                return result;
            }
        }

        private static bool TryGetTypeSerializer(Type type, out XmlSerializerBase serializer)
        {
            var result = false;
            serializer = null;
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
                result = Overrides.TryGetValue(type.GetGenericTypeDefinition(), out serializer);

            return result || Overrides.TryGetValue(type, out serializer);
        }


        private static XmlSerializerBase CreateArraySerializer(Type arrayType)
        {
            return null;
            //if (arrayType.IsUnTypedArray())
            //    return new BinaryPrimitives.UnTypedArraySerializer(arrayType);
            //return new BinaryPrimitives.ArraySerializer(arrayType);
        }

        private static XmlSerializerBase CreateSerializer(Type oType)
        {
            if (null == oType.GetConstructor(Type.EmptyTypes))
                return null;
            var instantiationMethod = typeof(XmlSerializerService).GetMethod("TryInstantiate",
                BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(oType);
            return (XmlSerializerBase)instantiationMethod.Invoke(null, null);
        }

        public static XmlSerializerBase GetSerializer(Type type)
        {
            XmlSerializerBase ser;
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);
            lock (Overrides)
            {
                if (TryGetTypeSerializer(type, out ser)) return ser;
                ser = type.IsArray ? CreateArraySerializer(type) : CreateSerializer(type);
                Overrides.Add(type, ser);
            }
            return ser;
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static XmlSerializerBase<T> GetSerializer<T>()
        {
            return (XmlSerializerBase<T>)GetSerializer(typeof(T));
        }

        public override void AddOverride<T>(ISerializer<T> serializer)
        {
            var xmlSerializer = serializer as XmlSerializerBase<T>;
            if (null == xmlSerializer) throw new InvalidOperationException("The specified serializer must derrive from XmlSerializerBsae in order to be used in this serializer");
            lock (Overrides)
            {
                var type = typeof(T);
                if (type.IsGenericType && !type.IsGenericTypeDefinition)
                    type = type.GetGenericTypeDefinition();

                if (!Overrides.ContainsKey(type))
                    Overrides.Add(type, xmlSerializer);
            }
        }

        public override ISerializer FindSerializer(Type type)
        {
            return GetSerializer(type);
        }

        public override ISerializer<T> FindSerializer<T>()
        {
            return GetSerializer<T>();
        }

        public static XmlSerializerBase TryInstantiate<T>() where T : new()
        {
            try
            {
                return new XmlObjectSerializer<T>();
            }
            catch (Exception)
            {
                //LogException(e, MethodBase.GetCurrentMethod());
                return null;
            }
        }
    }

    public abstract class XmlSerializerBase : ISerializer
    {
        public abstract void Serialize(object item, Stream target);
        public abstract object Deserialize(Stream source);
        public abstract int GetOutputSize(object item);
    }

    public abstract class XmlSerializerBase<T> : XmlSerializerBase, ISerializer<T>
    {
        public override object Deserialize(Stream source)
        {
            using (var reader = XmlReader.Create(source))
                return Deserialize(reader);
        }

        T ISerializer<T>.Deserialize(Stream source)
        {
            return (T) Deserialize(source);
        }

        public void Serialize(T item, Stream target)
        {
            using (var writer = XmlWriter.Create(target))
                Serialize(item, writer);
        }

        public int GetOutputSize(T item)
        {
            throw new NotImplementedException();
        }

        public abstract T Deserialize(XmlReader reader);
        public abstract void Serialize(T item, XmlWriter target);
    }

    public class XmlObjectSerializer<T> : XmlSerializerBase<T>
    {
        public override void Serialize(object item, Stream target)
        {
            throw new NotImplementedException();
        }

        public override int GetOutputSize(object item)
        {
            throw new NotImplementedException();
        }

        public override T Deserialize(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public override void Serialize(T item, XmlWriter target)
        {
            throw new NotImplementedException();
        }
    }
}
