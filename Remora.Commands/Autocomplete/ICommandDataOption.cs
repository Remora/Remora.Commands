//
//  ICommandDataOption.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) Jarl Gullberg
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

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Remora.Commands.Autocomplete;

/// <summary>
/// Represents a named option and its value.
/// </summary>
/// <typeparam name="T">The underlying type of the data option.</typeparam>
[PublicAPI]
public interface ICommandDataOption<out T>
{
    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the type of this option.
    /// </summary>
    OptionType OptionType { get; }

    /// <summary>
    /// Gets the value of the option.
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Gets the options supplied to the subcommand or subgroup.
    /// </summary>
    IReadOnlyList<ICommandDataOption<T>> Options { get; }

    /// <summary>
    /// Gets a value indicating whether the option is currently focused.
    /// </summary>
    bool? IsFocused { get; }
}
