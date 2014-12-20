using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using iLynx.Common;
using iLynx.Serialization;
using iLynx.Serialization.Xml;

namespace iLynx.Configuration
{
    public interface IConfigSection
    {
        void ReadFrom(Stream stream);
        IEnumerable<IConfigurableValue> GetAll(string category = null);
        Dictionary<string, Dictionary<string, IConfigurableValue>> Categories { get; }
        void WriteTo(Stream target);
        IConfigurableValue this[string category, string key] { get; set; }
        bool Contains(string category, string key);
        IEnumerable<string> GetCagerories();
        string FileExtension { get; }
    }

    /// <summary>
    /// ValuesContainer
    /// </summary>
    public class ValuesContainer
    {
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }

        private IConfigurableValue[] values = new IConfigurableValue[0];

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public IConfigurableValue[] Values { get { return values; } set { values = value; } }
    }

    public abstract class ConfigSection : IConfigSection
    {
        public string FileExtension { get { return extension; } }
        public const string DefaultCategory = "Default";

        private readonly Dictionary<string, Dictionary<string, IConfigurableValue>> categories = new Dictionary<string, Dictionary<string, IConfigurableValue>>();
        private readonly string extension;

        protected ConfigSection(string extension)
        {
            this.extension = extension;
        }

        public Dictionary<string, Dictionary<string, IConfigurableValue>> Categories
        {
            get { return categories; }
        }

        /// <summary>
        /// Gets all as.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IConfigurableValue> GetAll(string category = null)
        {
            return Categories.Where(x => null == category || x.Key == category).SelectMany(c => c.Value.Select(x => x.Value));
        }

        private Dictionary<string, IConfigurableValue> GetCategory(string cat)
        {
            Dictionary<string, IConfigurableValue> result;
            return Categories.TryGetValue(cat, out result) ? result : null;
        }

        /// <summary>
        /// Gets or sets a property, attribute, or child element of this configuration element.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public IConfigurableValue this[string category, string key]
        {
            get
            {
                if (null == key) return null;
                if (null == category)
                    category = DefaultCategory;
                var cat = GetCategory(category);
                if (null == cat) return null;
                IConfigurableValue value;
                return cat.TryGetValue(key, out value) ? value : null;
            }
            set
            {
                if (null == key) return;
                if (null == category)
                {
                    category = DefaultCategory;
                    value.Category = category;
                }
                var cat = GetCategory(category);
                if (null == cat)
                {
                    cat = new Dictionary<string, IConfigurableValue>();
                    Categories.Add(category, cat);
                }
                if (cat.ContainsKey(key))
                    cat[key] = value;
                else
                    cat.Add(key, value);
            }
        }

        /// <summary>
        /// Determines whether [contains] [the specified category].
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified category]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(string category, string key)
        {
            key.GuardString("key");
            if (null == category) category = DefaultCategory;
            Dictionary<string, IConfigurableValue> sub;
            return Categories.TryGetValue(category, out sub) && sub.ContainsKey(key);
        }

        /// <summary>
        /// Gets the cagerories.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetCagerories()
        {
            return Categories.Keys;
        }

        protected abstract ISerializer<ValuesContainer> ContainerSerializer { get; }

        public void WriteTo(Stream target)
        {
            try
            {
                var serializer = ContainerSerializer;
                foreach (var container in Categories.Select(category => new ValuesContainer
                {
                    Category = category.Key,
                    Values = category.Value.Values.ToArray()
                }))
                    serializer.Serialize(container, target);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }

        public void ReadFrom(Stream source)
        {
            var serializer = ContainerSerializer;
            while (source.Position != source.Length)
            {
                var container = serializer.Deserialize(source);
                var category = container.Category;
                Dictionary<string, IConfigurableValue> existing;
                if (!Categories.TryGetValue(category, out existing))
                    Categories.Add(category, (existing = new Dictionary<string, IConfigurableValue>()));
                foreach (var val in container.Values.Where(x => null != x))
                {
                    if (existing.ContainsKey(val.Key))
                        existing[val.Key] = val;
                    else
                        existing.Add(val.Key, val);
                }
            }
        }
    }

    public class BinaryConfigSection : ConfigSection
    {
        public BinaryConfigSection()
            : base("bin")
        {
        }

        protected override ISerializer<ValuesContainer> ContainerSerializer
        {
            get { return BinarySerializerService.GetSerializer<ValuesContainer>(); }
        }
    }

    public class XmlConfigSection : ConfigSection
    {
        public XmlConfigSection()
            : base("xml")
        {

        }

        private class XmlSerializerWrapper : SerializerBase<ValuesContainer>
        {
            private readonly IXmlSerializer<ValuesContainer> xmlSerializer;
            public XmlSerializerWrapper()
            {
                xmlSerializer = XmlSerializerService.GetSerializer<ValuesContainer>();
            }
            public override int GetOutputSize(ValuesContainer item)
            {
                throw new NotSupportedException();
            }

            public override ValuesContainer Deserialize(Stream source)
            {
                using (var reader = XmlReader.Create(source, new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Fragment
                }))
                    return xmlSerializer.Deserialize(reader);
            }

            public override void Serialize(ValuesContainer item, Stream target)
            {
                using (var writer = XmlWriter.Create(target, new XmlWriterSettings
                {
                    ConformanceLevel = ConformanceLevel.Fragment
                }))
                    xmlSerializer.Serialize(item, writer);
            }
        }

        protected override ISerializer<ValuesContainer> ContainerSerializer
        {
            get { return new XmlSerializerWrapper(); }
        }
    }
}