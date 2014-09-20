using System;

namespace iLynx.Common.Threading
{
    public interface ITimerService
    {
        /// <summary>
        /// Starts a new timer with the specified timeout and interval and returns the Id of the newly created timer
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="timeout"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        int StartNew(Action callback, int timeout, int interval);

        /// <summary>
        /// Stops the timer with the specified Id
        /// </summary>
        /// <param name="id"></param>
        void Stop(int id);

        /// <summary>
        /// Changes the timeout and interval of the timer with the specified id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newTimeout"></param>
        /// <param name="newInterval"></param>
        void Change(int id, int newTimeout, int newInterval);
    }
}
