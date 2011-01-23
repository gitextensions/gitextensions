using System;
using System.Threading;

namespace GitUI
{
    static public class AsyncHelpers
    {
        /// <summary>
        /// Does something on threadpool, executes continuation on current sync context thread, executes onError if the async request fails.
        /// There does probably exist something like this in the .NET library, but I could not find it. //cocytus
        /// </summary>
        /// <typeparam name="T">Result to be passed from doMe to continueWith</typeparam>
        /// <param name="doMe">The stuff we want to do. Should return whatever continueWith expects.</param>
        /// <param name="continueWith">Do this on original sync context.</param>
        /// <param name="onError">Do this on original sync context if doMe barfs.</param>
        public static void DoAsync<T>(Func<T> doMe, Action<T> continueWith, Action<Exception> onError)
        {
            var syncContext = SynchronizationContext.Current;

            Action a = () =>
            {
                T res;
                try
                {
                    res = doMe();
                }
                catch (Exception ex)
                {
                    SendOrPostCallback cbe = exp => onError((Exception)exp);
                    syncContext.Post(cbe, ex);
                    return;
                }

                SendOrPostCallback cb = tres => continueWith((T)tres);
                syncContext.Post(cb, res);
            };
            a.BeginInvoke(null, null);
        }
    }
}
