using System.Globalization;

namespace Sct.Extensions
{
    public static class TypeExtensions
    {
        ///<summary>
        ///Returns the name of the type, including generic arguments.
        ///Object is replaced with dynamic.
        ///</summary>
        ///<param name="type">(this) The type to generate a name for</param>
        ///<returns>The name of the type, including generics</returns>
        public static string GenericName(this Type type)
        {
            if (type.IsGenericType)
            {
                string genericArguments = type.GetGenericArguments()
                    .Select(GenericName)
                    .Aggregate((x1, x2) => $"{x1}, {x2}");
                return $"{type.Name.Substring(0, type.Name.IndexOf('`'))}"
                    + $"<{genericArguments}>";
            }
            return type.Name.Replace("Object", "dynamic", ignoreCase: true, culture: CultureInfo.InvariantCulture);
        }
    }
}
