using System;

namespace Allors.Extra
{
    public static class StringExtensions
    {
        public static string Left(this string @this, int count)
        {
            if (@this.Length <= count)
            {
                return @this;
            }
            else
            {
                return @this.Substring(0, count);
            }
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }

    public static class BooleanExtensions
    {
        /// <summary>
        /// Returns "YES" if true, else "NO" 
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string YesOrNo(this bool? @this)
        {
            return @this == true
                ? "YES"
                : "NO";
        }

        /// <summary>
        /// Returns "YES" if true, else "NO" 
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string YesOrNo(this bool @this)
        {
            return @this == true
                ? "YES"
                : "NO";
        }
    }
}
