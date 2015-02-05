using System;
using iLynx.Common;

namespace iLynx.Configuration
{
    /// <summary>
    ///     A simple configurable value (Uses the builtin ConfigurationManager (
    ///     <see
    ///         cref="System.Configuration.ConfigurationManager" />
    ///     ))
    /// </summary>
    public interface IConfigurableValue
    {
        /// <summary>
        ///     Gets the value of this <see cref="IConfigurableValue" />
        /// </summary>
        object Value { get; set; }

        /// <summary>
        ///     Gets the key of this <see cref="IConfigurableValue" />
        /// </summary>
        string Key { get; }

        /// <summary>
        ///     Gets a value indicating whether or not the value of this <see cref="IConfigurableValue" /> has been saved to disk
        /// </summary>
        bool IsStored { get; }

        /// <summary>
        ///     Attempts to store this value in the configuration file
        /// </summary>
        void Store();

        /// <summary>
        ///     This event is fired whenever the value of this <see cref="IConfigurableValue" /> has changed
        /// </summary>
        event EventHandler<ValueChangedEventArgs<object>> ValueChanged;

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        string Category { get; set; }
    }

    /// <summary>
    /// IConfigurableValue{T}
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConfigurableValue<T> : IConfigurableValue
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        new T Value { get; set; }

        /// <summary>
        /// Raised whenever the value of this ConfigurableValue changes.
        /// </summary>
        new event EventHandler<ValueChangedEventArgs<T>> ValueChanged;
    }
}