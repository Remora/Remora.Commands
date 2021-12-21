//
//  ICommandArgumentInfo.cs
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

namespace Remora.Commands.CommandInformation
{
    /// <summary>
    /// Contains information about a specific command argument.
    /// </summary>
    public interface ICommandArgumentInfo
    {
        /// <summary>
        /// Gets the name of the argument.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the argument's description, if any.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Gets the position of the argument.
        /// </summary>
        int Position { get; }

        /// <summary>
        /// Gets the type of the argument.
        /// </summary>
        Type ArgumentType { get; }

        /// <summary>
        /// Gets a value indicating whether the argument is optional.
        /// </summary>
        bool IsOptional { get; }

        /// <summary>
        /// Gets a value indicating whether a default value is present.
        /// </summary>
        bool HasDefaultValue { get; }

        /// <summary>
        /// Gets the provided default value, if present.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws when <see cref="HasDefaultValue"/> is false.</exception>
        object? DefaultValue { get; }
    }
}
