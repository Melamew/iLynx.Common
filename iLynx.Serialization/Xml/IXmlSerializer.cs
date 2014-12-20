using System.Xml;

namespace iLynx.Serialization.Xml
{
    public interface IXmlSerializer
    {
        object Deserialize(XmlReader reader);
        void Serialize(object item, XmlWriter writer);
    }

    public interface IXmlSerializer<T> : IXmlSerializer
    {
        new T Deserialize(XmlReader reader);
        void Serialize(T item, XmlWriter writer);
    }
}
