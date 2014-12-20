using System;
using System.Reflection;
using System.Xml;

namespace iLynx.Serialization.Xml
{
    public class XmlObjectSerializer<T> : XmlObjectSerializerBase<T>
    {
        public XmlObjectSerializer()
            : base(XmlSerializerService.GetSerializer)
        {
        }

        public override T Deserialize(XmlReader reader)
        {
            var target = Activator.CreateInstance(typeof(T));
            reader.Read();
            try
            {
                foreach (var member in Graph)
                {
                    reader.Read();
                    try
                    {
                        IXmlSerializer serializer;
                        if (ShouldReadType(reader))
                        {
                            var memberType = ReadType(reader);
                            if (null == memberType)
                                continue;
                            serializer = XmlSerializerService.GetSerializer(memberType);
                        }
                        else serializer = member.TypeSerializer;
                        reader.ReadStartElement();
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
                    finally { reader.Read(); }
                }
                return (T)target;
            }
            finally { reader.Read(); }
        }

        private static bool ShouldReadType(XmlReader reader)
        {
            var attribute = reader.GetAttribute("T");
            return null != attribute && bool.Parse(attribute);
        }

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="target">The target.</param>
        public override void Serialize(T item, XmlWriter target)
        {
            target.WriteStartElement("A" + Guid.NewGuid().ToString("N"));
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
                            target.WriteAttributeString("T", true.ToString());// Indicate that we need to read the type when we deserialize.
                            WriteType(target, type);
                        }
                        else
                        {
                            target.WriteAttributeString("T", false.ToString());// Indicate that we do NOT need to read the type when we deserialize.
                            serializer = member.TypeSerializer;
                        }

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

        private static Type ReadType(XmlReader reader)
        {
            var typeName = reader.GetAttribute("Type");
            return null == typeName ? typeof(NullType) : Type.GetType(typeName);
        }
    }
}