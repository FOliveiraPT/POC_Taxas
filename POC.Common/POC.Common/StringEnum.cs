using System.Reflection;

namespace POC.Common
{
    using System;
    using System.Linq;

    public static class StringEnum
    {
        public static string GetStringValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            FieldInfo fi = type.GetField(value.ToString());
            var attrs = fi.GetCustomAttributes(typeof(StringValueAttribute),
                                       false) as StringValueAttribute[];
            if (attrs.Any())
            {
                output = attrs[0].Value;
            }

            return output;
        }
    }
}