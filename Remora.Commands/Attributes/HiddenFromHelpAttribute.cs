//
//  HiddenFromHelpAttribute.cs
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

namespace Remora.Commands.Attributes
{
    /// <summary>
    /// Marks a command or group as being hidden from the help service.
    /// </summary>
    /// <remarks>
    /// This attribute represents a request to be hidden. The implementing
    /// service may or may not respect this request.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class HiddenFromHelpAttribute : Attribute
    {
        /// <summary>
        /// Gets a comment provided by the developer, if any.
        /// </summary>
        public string? Comment { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HiddenFromHelpAttribute"/> class.
        /// </summary>
        /// <param name="comment">A comment describing why this is hidden or any relevant notes. May be displayed by the help service.</param>
        public HiddenFromHelpAttribute(string? comment = null)
        {
            Comment = comment;
        }
    }
}
