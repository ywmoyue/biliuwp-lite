using System.Diagnostics;
using Windows.ApplicationModel.Background;
using BiliLite.Extensions.Notifications;

namespace BackgroundTasks
{
    public sealed class TileFeedBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("================ debug to updating tiles  ================");

            // Get a deferral, to prevent the task from closing prematurely
            // while asynchronous code is still running.
            var deferral = taskInstance.GetDeferral();

            NotificationShowExtensions.Tile();

            // Inform the system that the task is finished.
            deferral.Complete();
        }
    }
}