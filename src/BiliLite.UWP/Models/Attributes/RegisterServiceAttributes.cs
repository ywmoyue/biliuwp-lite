using System;

namespace BiliLite.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RegisterSingletonUIServiceAttribute : Attribute
    {
        public Type SuperType { get; set; }

        public RegisterSingletonUIServiceAttribute() { }

        public RegisterSingletonUIServiceAttribute(Type superType)
        {
            SuperType = superType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RegisterSingletonServiceAttribute : Attribute
    {
        public Type SuperType { get; set; }

        public RegisterSingletonServiceAttribute() { }

        public RegisterSingletonServiceAttribute(Type superType)
        {
            SuperType = superType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RegisterTransientServiceAttribute : Attribute
    {
        public Type SuperType { get; set; }

        public RegisterTransientServiceAttribute() { }

        public RegisterTransientServiceAttribute(Type type)
        {
            SuperType = type;
        }
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