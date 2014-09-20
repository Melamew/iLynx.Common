using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using iLynx.Common.Serialization;

namespace iLynx.Common.Configuration
{
    public class BinaryConfigSection
    {
        public const string DefaultCategory = "Default";

        private readonly Dictionary<string, Dictionary<string, ConfigurableValue>> categories = new Dictionary<string, Dictionary<string, ConfigurableValue>>();

        public Dictionary<string, Dictionary<string, ConfigurableValue>> Categories
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

        /// <summary>
        /// Reads XML from the configuration file.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void ReadFrom(Stream stream)
        {
            var serializer = new NaiveSerializer<ValuesContainer>(RuntimeCommon.DefaultLogger);
            while (stream.Position != stream.Length)
            {
                var container = serializer.Deserialize(stream);
                var category = container.Category;
                Dictionary<string, ConfigurableValue> existing;
                if (!categories.TryGetValue(category, out existing))
                    categories.Add(category, (existing = new Dictionary<string, ConfigurableValue>()));
                foreach (var val in container.Values)
                {
                    if (existing.ContainsKey(val.Key))
                        existing[val.Key] = val;
                    else
                        existing.Add(val.Key, val);
                }
            }
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

            private ConfigurableValue[] values = new ConfigurableValue[0];

            /// <summary>
            /// Gets or sets the values.
            /// </summary>
            /// <value>
            /// The values.
            /// </value>
            public ConfigurableValue[] Values { get { return values; } set { values = value; } }
        }

        /// <summary>
        /// Writes the outer tags of this configuration element to the configuration file when implemented in a derived class.
        /// </summary>
        /// <returns>
        /// true if writing was successful; otherwise, false.
        /// </returns>
        public void WriteTo(Stream target)
        {
            try
            {
                var serializer = new NaiveSerializer<ValuesContainer>(RuntimeCommon.DefaultLogger);
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

        private Dictionary<string, ConfigurableValue> GetCategory(string cat)
        {
            Dictionary<string, ConfigurableValue> result;
            return Categories.TryGetValue(cat, out result) ? result : null;
        }

        /// <summary>
        /// Gets or sets a property, attribute, or child element of this configuration element.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public ConfigurableValue this[string category, string key]
        {
            get
            {
                if (null == key) return null;
                if (null == category)
                    category = DefaultCategory;
                var cat = GetCategory(category);
                if (null == cat) return null;
                ConfigurableValue value;
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
                    cat = new Dictionary<string, ConfigurableValue>();
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
            Dictionary<string, ConfigurableValue> sub;
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
    }
}