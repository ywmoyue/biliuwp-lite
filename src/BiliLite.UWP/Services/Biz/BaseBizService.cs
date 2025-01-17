using BiliLite.Extensions;
using BiliLite.Models.Common;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BiliLite.Models.Exceptions;

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
                Notify.ShowMessageToast(ex.Message);
                return;
            }
            if (ex.IsNetworkError())
            {
                _logger.Error("请检查你的网络连接", ex);
                Notify.ShowMessageToast("请检查你的网络连接");
            }
            else
            {
                var type = new StackTrace().GetFrame(1).GetMethod().ReflectedType;
                _logger.Log(ex.Message, LogType.Error, ex, methodName, type.Name);
                Notify.ShowMessageToast("出现了一个未处理错误，已记录");
            }
        }
    }
}
