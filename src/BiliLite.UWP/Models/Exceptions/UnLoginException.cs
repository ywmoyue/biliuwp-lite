using System;

namespace BiliLite.Models.Exceptions;

public class UnLoginException : Exception
{
    public UnLoginException() : base("未登录")
    {
    }
}