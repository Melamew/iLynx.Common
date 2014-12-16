using System;
using System.Diagnostics;
using iLynx.Common;

namespace iLynx.Configuration
{
    /// <summary>
    /// ConfigurableValue
    /// </summary>
    public class ConfigurableValue : IConfigurableValue
    {
        private object value;
        private string category;

        public ConfigurableValue() { }

        /// <summary>
        /// Initializes a new instance of <see cref="ExeConfigValue{T}" /> and attempts to load it's value from the
        /// <para />
        /// configuration manager (using the specified <see cref="IValueConverter{TValue}" /> to convert the retrieved string to a
        /// type
        /// </summary>
        /// <param name="key">The key of the value to look for</param>
        /// <param name="defaultValue">The default value to use if the value could not be retrieved from the configuration file</param>
        /// <param name="category">The category.</param>
        public ConfigurableValue(string key, object defaultValue, string category = null)
        {
            Key = key;
            this.category = category;
            try
            {
                Load(category, key, defaultValue);
            }
            catch
            {
                value = defaultValue;
                Store();
            }
        }

        /// <summary>
        /// Loads the specified category.
        /// </summary>
        /// <param name="cat">The category.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        private void Load(string cat, string key, object defaultValue)
        {
            if (!ExeConfig.ConfigurableValuesSection.Contains(cat, key))
            {
                value = defaultValue;
                Store();
            }
            else
            {
                var val = ExeConfig.ConfigurableValuesSection[cat, key];
                if (null == val)
                {
                    Store();
                    return;
                }
                value = val.Value;
                category = val.Category;
            }
        }

        /// <summary>
        /// Gets the key of this <see cref="IConfigurableValue" />
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///     Gets or Sets the value of this <see cref="ExeConfigValue{T}" />
        /// </summary>
        public object Value
        {
            get { return value; }
            set
            {
                var old = this.value;
                this.value = value;
                OnValueChanged(old, this.value);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether or not the value of this <see cref="ExeConfigValue{T}" /> has been saved to disk
        /// </summary>
        public bool IsStored
        {
            get
            {
                var exists = ExeConfig.ConfigurableValuesSection.Contains(category, Key);
                if (!exists)
                    return false;
                var stored = ExeConfig.ConfigurableValuesSection[category, Key];
                return stored != null && stored.Value == Value;
            }
        }

        /// <summary>
        ///     Attempts to store this value in the configuration file
        /// </summary>
        public void Store()
        {
            try
            {
                ExeConfig.ConfigurableValuesSection[category, Key] = this;
                ExeConfig.Save();
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format("Unable to store current value: {0}", e));
            }
        }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category
        {
            get { return category; }
            set { category = value; }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            try { return value.ToString(); }
            catch
            {
                return base.ToString();
            }
        }

        /// <summary>
        /// Called when [value changed].
        /// </summary>
        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
            if (ValueChanged != null)
                ValueChanged(this, new ValueChangedEventArgs<object>(oldValue, newValue));
        }

        /// <summary>
        ///     Fired whenever the value part of this <see cref="ExeConfigValue{T}" /> has changed
        /// </summary>
        public event EventHandler<ValueChangedEventArgs<object>>  ValueChanged;
    }
}