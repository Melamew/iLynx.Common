using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using iLynx.Common;

namespace iLynx.Serialization.Xml
{
    public abstract class XmlObjectSerializerBase<T> : XmlSerializerBase<T>
    {
        protected readonly IEnumerable<SerializationInfo<IXmlSerializer>> Graph;

        protected XmlObjectSerializerBase(Func<Type, IXmlSerializer> getSerializerCallback)
        {
            if (typeof(T) == Type.Missing.GetType()) throw new NotSupportedException("Missing Types are currently not supported");
            Graph = typeof(T).BuildObjectGraph(getSerializerCallback).Values;
        }

        /// <summary>
        /// Posts the quit.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="m">The m.</param>
        protected virtual void PostQuit(Exception e, MethodBase m)
        {
            this.LogError("{0}: {1}", e, m);
            this.LogCritical("Last Error was unrecoverable. Giving up");
        }
    }

    public abstract class XmlSerializerBase<T> : IXmlSerializer<T>
    {
        object IXmlSerializer.Deserialize(XmlReader reader)
        {
            return Deserialize(reader);
        }

        public abstract void Serialize(T item, XmlWriter writer);

        public abstract T Deserialize(XmlReader reader);

        public void Serialize(object item, XmlWriter writer)
        {
            Serialize((T) item, writer);
        }
    }
}