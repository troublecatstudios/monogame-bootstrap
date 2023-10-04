using System.ComponentModel;
using System.Reflection;

namespace Troublecat.Utilities;

public static class EnumerationExtensions {
    public static string GetDescription<T>(this T enumerationValue) where T : Enum {
        Type type = enumerationValue.GetType();

        // Tries to find a DescriptionAttribute for a potential friendly name
        // for the enum
        MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
        if (memberInfo.Length > 0) {
            if (memberInfo[0].GetCustomAttribute(typeof(DescriptionAttribute), false) is DescriptionAttribute attr) {
                // Pull out the description value
                return attr.Description;
            }
        }

        // If we have no description attribute, just return the ToString of the enum
        return enumerationValue.ToString();
    }
}
