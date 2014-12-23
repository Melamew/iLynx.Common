using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

    public class CategoriesContainer
    {
        public ValuesContainer[] Sections { get; set; }
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

        protected abstract ISerializer<CategoriesContainer> ContainerSerializer { get; }

        public void WriteTo(Stream target)
        {
            try
            {
                var serializer = ContainerSerializer;
                var container = new CategoriesContainer
                                {
                                    Sections = Categories.Select(category => new ValuesContainer
                                                                             {
                                                                                 Category = category.Key,
                                                                                 Values = category.Value.Values.ToArray()
                                                                             }).ToArray(),
                                };
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
                foreach (var categoryValues in container.Sections)
                    categories.Add(categoryValues.Category, categoryValues.Values.ToDictionary(x => x.Key, x => x));
            }
        }
    }

    public class BinaryConfigSection : ConfigSection
    {
        public BinaryConfigSection()
            : base("bin")
        {
        }

        protected override ISerializer<CategoriesContainer> ContainerSerializer
        {
            get { return BinarySerializerService.GetSerializer<CategoriesContainer>(); }
        }
    }

    public class XmlConfigSection : ConfigSection
    {
        public XmlConfigSection()
            : base("xml")
        {

        }

        private class XmlSerializerWrapper : SerializerBase<CategoriesContainer>
        {
            private readonly IXmlSerializer<CategoriesContainer> xmlSerializer;
            public XmlSerializerWrapper()
            {
                xmlSerializer = XmlSerializerService.GetSerializer<CategoriesContainer>();
            }
            public override int GetOutputSize(CategoriesContainer item)
            {
                throw new NotSupportedException();
            }

            public override CategoriesContainer Deserialize(Stream source)
            {
                using (var reader = XmlReader.Create(source, new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Fragment,
                }))
                    return xmlSerializer.Deserialize(reader);
            }

            public override void Serialize(CategoriesContainer item, Stream target)
            {
                using (var writer = XmlWriter.Create(target, new XmlWriterSettings
                {
                    ConformanceLevel = ConformanceLevel.Fragment,
                    WriteEndDocumentOnClose = false,
                    OmitXmlDeclaration = true,
                    Indent = true,


                }))
                    xmlSerializer.Serialize(item, writer);
            }
        }

        protected override ISerializer<CategoriesContainer> ContainerSerializer
        {
            get { return new XmlSerializerWrapper(); }
        }
    }
}