using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;

namespace BiliLite.Player
{
    /// <summary>
    /// WinUI3 的 DispatcherQueue 只有 TryEnqueue，没有 UWP CoreDispatcher 的 RunAsync。
    /// 播放器相关代码（重构后沿用 UWP 写法）统一走这里的适配，保持 await 语义。
    /// </summary>
    public static class DispatcherQueueExtensions
    {
        public static Task RunAsync(this DispatcherQueue dispatcherQueue, DispatcherQueuePriority priority,
            Action action)
        {
            var completionSource = new TaskCompletionSource<bool>();
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
