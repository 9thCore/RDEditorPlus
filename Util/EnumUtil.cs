using System;

namespace RDEditorPlus.Util
{
    public static class EnumUtil
    {
        // This sucks
        // https://github.com/dotnet/runtime/discussions/81264
        public static T GetAttributeOfType<T>(this Enum enumVal)
            where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }
    }
}
