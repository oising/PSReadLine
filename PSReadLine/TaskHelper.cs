using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.PowerShell
{
    /// <summary>
    /// Shim for Task.Delay which is not available in .NET 4.0 client profile.
    /// </summary>
    public static class TaskHelper
    {
        private static readonly Task preCompletedTask;
        private static readonly List<Timer> rootedTimers; 

        static TaskHelper()
        {
            rootedTimers = new List<Timer>();
            var source = new TaskCompletionSource<bool>(false);
            source.TrySetResult(false);
            preCompletedTask = source.Task;
        }

        public static Task Delay(int dueTime, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new Task(() => { }, cancellationToken);
            }

            if (dueTime == 0)
            {
                return preCompletedTask;
            }

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            CancellationTokenRegistration ctr = new CancellationTokenRegistration();
            Timer timer = null;

            timer = new Timer(
                state =>
                {
                    ctr.Dispose();
                    timer.Dispose();
                    tcs.TrySetResult(true);
                    rootedTimers.Remove(timer);
                },
                null, (long) -1, -1);

            rootedTimers.Add(timer);

            if (cancellationToken.CanBeCanceled)
            {
                Action action = () =>
                {
                    timer.Dispose();
                    tcs.TrySetCanceled();
                    rootedTimers.Remove(timer);
                };
                ctr = cancellationToken.Register(action);
            }
            timer.Change(dueTime, -1);

            return tcs.Task;
        }
    }
}