using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;

namespace BiliLite.Extensions
{
    /// <summary>
    /// WinUI3 只有 DispatcherQueue.TryEnqueue，没有 UWP CoreDispatcher 的 RunAsync；
    /// DependencyObject.Dispatcher 在 Windows App SDK 里恒为 null，因此统一走这里。
    /// </summary>
    public static class DispatcherQueueExtensions
    {
        public static Task RunAsync(this DispatcherQueue dispatcherQueue, DispatcherQueuePriority priority,
            Action action)
        {
            var completionSource = new TaskCompletionSource<bool>();
            if (dispatcherQueue == null)
            {
                completionSource.TrySetResult(false);
                return completionSource.Task;
            }

            var enqueued = dispatcherQueue.TryEnqueue(priority, () =>
            {
                try
                {
                    action();
                    completionSource.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    completionSource.TrySetException(ex);
                }
            });
            if (!enqueued)
            {
                completionSource.TrySetResult(false);
            }

            return completionSource.Task;
        }

        public static Task RunAsync(this DispatcherQueue dispatcherQueue, DispatcherQueuePriority priority,
            Func<Task> action)
        {
            var completionSource = new TaskCompletionSource<bool>();
            if (dispatcherQueue == null)
            {
                completionSource.TrySetResult(false);
                return completionSource.Task;
            }

            var enqueued = dispatcherQueue.TryEnqueue(priority, async () =>
            {
                try
                {
                    await action();
                    completionSource.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    completionSource.TrySetException(ex);
                }
            });
            if (!enqueued)
            {
                completionSource.TrySetResult(false);
            }

            return completionSource.Task;
        }
    }
}
