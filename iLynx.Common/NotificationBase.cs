using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace iLynx.Common
{
    /// <summary>
    /// NotificationBase
    /// </summary>
    public abstract class NotificationBase : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        protected virtual void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var handler = PropertyChanged;
            if (null == handler) return;
            var property = RuntimeHelper.GetPropertyName(propertyExpression);
            handler(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            var handler = PropertyChanged;
            if (null == handler) return;
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        ~NotificationBase()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
