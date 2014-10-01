using System;
using System.Windows;
using System.Windows.Threading;

namespace iLynx.Common.WPF
{
    public class WPFApplicationDispatcher : IDispatcher
    {
        /// <summary>
        /// Gets the dispatcher.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Can not retrieve Dispatcher</exception>
        private static Dispatcher GetDispatcher()
        {
            if (null == Application.Current) throw new InvalidOperationException("Can not retrieve Dispatcher");
            if (null == Application.Current.Dispatcher) throw new InvalidOperationException("Wat!?");
            return Application.Current.Dispatcher;
        }

        /// <summary>
        /// Gets the dispatcher.
        /// </summary>
        /// <value>
        /// The dispatcher.
        /// </value>
        private Dispatcher Dispatcher { get { return GetDispatcher(); } }

        public bool CheckAccess()
        {
            return Dispatcher.CheckAccess();
        }

        /// <summary>
        /// Invokes the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public object Invoke(Delegate method, TimeSpan timeout, params object[] args)
        {
            return Dispatcher.Invoke(method, timeout, args);
        }

        /// <summary>
        /// Begins the invoke.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="args">The args.</param>
        public void BeginInvoke(Delegate method, params object[] args)
        {
            Dispatcher.BeginInvoke(method, args);
        }

        /// <summary>
        /// Invokes the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        public void Invoke(Action action)
        {
            Dispatcher.Invoke(action);
        }

        /// <summary>
        /// Invokes the specified priority.
        /// </summary>
        /// <param name="priority">The priority.</param>
        /// <param name="action">The action.</param>
        public void Invoke(DispatcherPriority priority, Action action)
        {
            Dispatcher.Invoke(priority, action);
        }

        /// <summary>
        /// Invokes the specified action.
        /// </summary>
        /// <typeparam name="TParam">The type of the param.</typeparam>
        /// <param name="action">The action.</param>
        /// <param name="param">The param.</param>
        public void Invoke<TParam>(Action<TParam> action, TParam param)
        {
            Dispatcher.Invoke(action, param);
        }

        public void InvokeIfRequired<TParam>(Action<TParam> action, TParam param)
        {
            if (!Dispatcher.CheckAccess())
                Invoke(action, param);
            else
                action.Invoke(param);
        }

        public void InvokeIfRequired(Action action)
        {
            if (!Dispatcher.CheckAccess())
                Invoke(action);
            else
                action.Invoke();
        }

        /// <summary>
        /// Invokes the specified func.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The func.</param>
        /// <returns></returns>
        public TResult Invoke<TResult>(Func<TResult> func)
        {
            return Dispatcher.Invoke(func);
        }
    }
}
