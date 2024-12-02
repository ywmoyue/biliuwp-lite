using System;

namespace BiliLite.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RegisterSingletonServiceAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RegisterTransientServiceAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RegisterSingletonViewModelAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RegisterTransientViewModelAttribute : Attribute
    {

    }
}
