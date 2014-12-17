﻿using System.Configuration;

namespace iLynx.Configuration
{
    /// <summary>
    ///     A simple configurable value (Uses the builtin ConfigurationManager (
    ///     <see
    ///         cref="System.Configuration.ConfigurationManager" />
    ///     ))
    /// </summary>
    /// <typeparam name="T">The type of value to store</typeparam>
    public class ExeConfigValue<T> : ConfigurableValue, IConfigurableValue<T>
    {
        public ExeConfigValue()
        {
        }

        public ExeConfigValue(string key, T defaultValue, string category = null)
            : base(key, defaultValue, category)
        {
            if (!Equals(default(T), Value)) return;
            Value = defaultValue;
            Store();
        }

        /// <summary>
        ///     Simply retrieves the value of the specified <see cref="ExeConfigValue{T}" />
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static implicit operator T(ExeConfigValue<T> val)
        {
            return val.Value;
        }

        /// <summary>
        ///     Simply returns <see cref="ExeConfigValue{T}.ToString()" />
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static explicit operator string(ExeConfigValue<T> val)
        {
            return val.ToString();
        }

        /// <summary>
        /// Gets or Sets the value of this <see cref="ExeConfigValue{T}" />
        /// </summary>
        public new T Value
        {
            get { return (T)base.Value; }
            set
            {
                if (Equals(base.Value, value)) return;
                base.Value = value;
            }
        }
    }
}