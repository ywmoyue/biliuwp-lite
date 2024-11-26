using System;
using Windows.ApplicationModel.Background;

namespace BiliLite.Extensions.Notifications
{
    public class NotificationRegisterExtensions
    {
        /// <summary>
        /// 单次执行后台任务
        /// </summary>
        /// <param name="taskName"></param>
        public static async void BackgroundTask(string taskName)
        {
            BackgroundExecutionManager.RemoveAccess();
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if (backgroundAccessStatus == BackgroundAccessStatus.AllowedSubjectToSystemPolicy ||
                backgroundAccessStatus == BackgroundAccessStatus.AlwaysAllowed)
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == taskName)
                    {
                        task.Value.Unregister(true);
                    }
                }

                var taskBuilder = new BackgroundTaskBuilder()
                {
                    Name = taskName,
                };
                var trigger = new ApplicationTrigger();
                taskBuilder.SetTrigger(trigger);
                taskBuilder.Register();
                await trigger.RequestAsync();
            }
        }

        /// <summary>
        /// 注册进程内后台任务
        /// </summary>
        /// <param name="taskName"></param>
        /// <param name="trigger"></param>
        public static async void BackgroundTask(string taskName, IBackgroundTrigger trigger)
        {
            BackgroundExecutionManager.RemoveAccess();
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if (backgroundAccessStatus == BackgroundAccessStatus.AllowedSubjectToSystemPolicy ||
                backgroundAccessStatus == BackgroundAccessStatus.AlwaysAllowed)
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == taskName)
                    {
                        task.Value.Unregister(true);
                    }
                }

                var taskBuilder = new BackgroundTaskBuilder()
                {
                    Name = taskName,
                };
                taskBuilder.SetTrigger(trigger);
                taskBuilder.Register();
            }
        }

        /// <summary>
        /// （不可用）注册进程外后台任务
        /// </summary>
        /// <param name="taskName"></param>
        /// <param name="taskEntryPoint"></param>
        /// <param name="trigger"></param>
        public static async void BackgroundTask(string taskName, string taskEntryPoint, IBackgroundTrigger trigger)
        {
            BackgroundExecutionManager.RemoveAccess();
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if (backgroundAccessStatus == BackgroundAccessStatus.AllowedSubjectToSystemPolicy ||
                backgroundAccessStatus == BackgroundAccessStatus.AlwaysAllowed)
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == taskName)
                    {
                        task.Value.Unregister(true);
                    }
                }

                
                var taskBuilder = new BackgroundTaskBuilder()
                {
                    Name = taskName,
                    TaskEntryPoint = taskEntryPoint
                };
                taskBuilder.SetTrigger(trigger);
                taskBuilder.Register();
            }
        }
    }
}