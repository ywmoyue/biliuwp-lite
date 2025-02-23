using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Exceptions;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BiliLite.Services.Biz
{
    public abstract class BaseBizService
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public virtual void HandleError(Exception ex, [CallerMemberName] string methodName = null)
        {
            if (ex is CustomizedErrorException)
            {
                _logger.Error(ex.Message, ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                return;
            }
            if (ex.IsNetworkError())
            {
                _logger.Error("请检查你的网络连接", ex);
                NotificationShowExtensions.ShowMessageToast("请检查你的网络连接");
            }
            else
            {
                var type = new StackTrace().GetFrame(1).GetMethod().ReflectedType;
                _logger.Log(ex.Message, LogType.Error, ex, methodName, type.Name);
                NotificationShowExtensions.ShowMessageToast("出现了一个未处理错误，已记录");
            }
        }
    }
}
