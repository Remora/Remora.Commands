//
//  CustomAttributeProviderExtensions.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Remora.Commands.Extensions
{
    /// <summary>
    /// Defines extension methods for the <see cref="ICustomAttributeProvider"/> interface.
    /// </summary>
    public static class CustomAttributeProviderExtensions
    {
        /// <summary>
        /// Gets the first instance of a custom attribute of the given type from the given attribute provider.
        /// </summary>
        /// <param name="attributeProvider">The attribute provider.</param>
        /// <param name="inherit">Whether inherited attributes should be considered.</param>
        /// <typeparam name="TAttribute">The attribute type.</typeparam>
        /// <returns>The found attribute, or null.</returns>
        public static TAttribute? GetCustomAttribute<TAttribute>
        (
            this ICustomAttributeProvider attributeProvider,
            bool inherit = false
        )
            where TAttribute : Attribute
        {
            return attributeProvider.GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault() as TAttribute;
        }

        /// <summary>
        /// Gets a user-configured description of the given attribute provider. The description is taken from an
        /// instance of the <see cref="DescriptionAttribute"/>.
        /// </summary>
        /// <param name="attributeProvider">The attribute provider.</param>
        /// <param name="defaultDescription">The default description to use if no attribute can be found.</param>
        /// <returns>The description.</returns>
        public static string GetDescriptionOrDefault
        (
            this ICustomAttributeProvider attributeProvider,
            string defaultDescription = "No description set."
        )
        {
            var descriptionAttribute = attributeProvider.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute is null)
            {
                return defaultDescription;
            }

            return string.IsNullOrWhiteSpace(descriptionAttribute.Description)
                ? defaultDescription
                : descriptionAttribute.Description;
        }
    }
}
