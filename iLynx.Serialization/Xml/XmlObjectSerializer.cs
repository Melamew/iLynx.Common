using System;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using iLynx.Common;

namespace iLynx.Serialization.Xml
{
    public class XmlObjectSerializer<T> : XmlObjectSerializerBase<T>
    {
        private readonly string xmlFriendlyName = typeof(T).Name.Replace('`', '.');
        public XmlObjectSerializer()
            : base(XmlSerializerService.GetSerializer)
        {
        }

        public override T Deserialize(XmlReader reader)
        {
            var target = Activator.CreateInstance(typeof(T));
            reader.SkipToElement(xmlFriendlyName);
            if (reader.IsEmptyElement) return (T)target;
            reader.ReadStartElement(xmlFriendlyName);
            try
            {
                foreach (var member in Graph)
                {
                    reader.SkipToElement(member.Member.Name);
                    try
                    {
                        IXmlSerializer serializer;
                        var typeName = reader.GetAttribute("Type");
                        if (null != typeName)
                        {
                            var memberType = Type.GetType(typeName);
                            if (null == memberType)
                                continue;
                            serializer = XmlSerializerService.GetSerializer(memberType);
                        }
                        else serializer = member.TypeSerializer;
                        reader.ReadStartElement(member.Member.Name);
                        var value = serializer.Deserialize(reader);
                        try
                        {
                            if (value is NullType)
                                member.SetValue(ref target, null);
                            else
                                member.SetValue(ref target, value);
                        }
                        catch (Exception e)
                        {
                            PostQuit(e, MethodBase.GetCurrentMethod());
                            break;
                        }
                    }
                    finally
                    {
                        CompleteRead(reader);
                    }
                }
                return (T)target;
            }
            finally { CompleteRead(reader); }
        }

        private void CompleteRead(XmlReader reader)
        {
            if (reader.IsEmptyElement)
                reader.Skip();
            else
                reader.ReadEndElement();
        }

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="target">The target.</param>
        public override void Serialize(T item, XmlWriter target)
        {
            target.WriteStartElement(xmlFriendlyName);
            try
            {
                foreach (var member in Graph)
                {
                    target.WriteStartElement(member.Member.Name);
                    try
                    {
                        IXmlSerializer serializer;
                        var value = member.IsDelegate ? null : member.GetValue(item);
                        if (null == value || member.IsUntyped)
                        {
                            value = value ?? new NullType();
                            var type = value.GetType();
                            serializer = XmlSerializerService.GetSerializer(type);
                            WriteType(target, type);
                        }
                        else
                            serializer = member.TypeSerializer;

                        try
                        {
                            serializer.Serialize(value, target);
                        }
                        catch (Exception e)
                        {
                            PostQuit(e, MethodBase.GetCurrentMethod());
                            break;
                        }
                    }
                    finally { target.WriteEndElement(); }
                }
            }
            finally { target.WriteEndElement(); }
        }

        private static void WriteType(XmlWriter writer, Type type)
        {
            writer.WriteAttributeString("Type", type.AssemblyQualifiedName ?? Type.Missing.ToString());
        }
    }
}