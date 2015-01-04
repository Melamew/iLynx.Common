﻿using System;

namespace iLynx.Threading
{
    /// <summary>
    /// IWorker
    /// </summary>
    public interface IWorker
    {
        /// <summary>
        /// Executes the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        void Execute(object args = null);

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        void Abort();

        /// <summary>
        /// Waits this instance.
        /// </summary>
        void Wait();

        /// <summary>
        /// Waits the specified timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        void Wait(TimeSpan timeout);

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        Guid Id { get; }

        /// <summary>
        /// Occurs when [thread exit].
        /// </summary>
        event Action<Guid> ThreadExit;
    }

    /// <summary>
    /// IResultWorker
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IResultWorker<out TResult> : IWorker
    {
        /// <summary>
        /// Waits this instance.
        /// </summary>
        /// <returns></returns>
        new TResult Wait();

        /// <summary>
        /// Waits the specified timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        new TResult Wait(TimeSpan timeout);
    }

    /// <summary>
    /// IWorker
    /// </summary>
    /// <typeparam name="TParams">The type of the params.</typeparam>
    public interface IParameterizedWorker<in TParams> : IWorker
    {
        /// <summary>
        /// Executes the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        void Execute(TParams args);
    }

    /// <summary>
    /// IParameterizedResultWorker
    /// </summary>
    /// <typeparam name="TParams">The type of the params.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IParameterizedResultWorker<in TParams, out TResult> : IResultWorker<TResult>, IParameterizedWorker<TParams>
    {

    }
}