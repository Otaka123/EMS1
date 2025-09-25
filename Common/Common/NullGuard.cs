using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Common
{
    public static class NullGuard
    {

        /// <summary>
        /// Throws an ArgumentNullException if the string is null,
        /// or ArgumentException if the string is empty or whitespace.
        /// </summary>
       public static string ThrowIfNullOrEmpty(
       this string value,
       [CallerArgumentExpression("value")] string paramName = "")
             {
            if (value is null)
                throw new ArgumentNullException(paramName, $"{paramName} cannot be null.");

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{paramName} cannot be empty or whitespace.", paramName);

            return value;
        }
        /// <summary>
        /// Throws an ArgumentNullException if the object is null.
        /// </summary>
        public static T ThrowIfNull<T>(this T value, string paramName) where T : class
            {
                if (value is null)
                    throw new ArgumentNullException(paramName, $"{paramName} cannot be null.");

                return value;
            }

            /// <summary>
            /// Throws an ArgumentNullException if the collection is null,
            /// or ArgumentException if the collection is empty.
            /// </summary>
            public static IEnumerable<T> ThrowIfNullOrEmpty<T>(this IEnumerable<T> value, string paramName)
            {
                if (value is null)
                    throw new ArgumentNullException(paramName, $"{paramName} cannot be null.");

                if (!value.Any())
                    throw new ArgumentException($"{paramName} cannot be empty.", paramName);

                return value;
            }
        

    }


}
