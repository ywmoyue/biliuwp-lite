using System;

namespace BiliLite.Models.Functions
{
    public static class ShortcutFunctionFactory
    {
        public static IShortcutFunction Map(ShortcutFunctionModel model)
        {
            var type = Type.GetType(model.TypeName);
            var instance = Activator.CreateInstance(type);
            if (instance is IShortcutFunction shortcutFunction)
            {
                shortcutFunction.Keys = model.Keys;
                shortcutFunction.NeedKeyUp = model.NeedKeyUp;
                return shortcutFunction;
            }

            return null;
        }
    }
}
