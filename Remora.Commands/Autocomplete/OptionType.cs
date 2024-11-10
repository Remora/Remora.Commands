//
//  OptionType.cs
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

using JetBrains.Annotations;

namespace Remora.Commands.Autocomplete;

/// <summary>
/// Defines a base type for an OptionType.
/// </summary>
[PublicAPI]
public sealed class OptionType(string name)
{
    /// <summary>
    /// Gets an option type representing a SubCommand.
    /// </summary>
    public static readonly OptionType SubCommand = new(nameof(SubCommand));

    /// <summary>
    /// Gets an option type representing a subgroup.
    /// </summary>
    public static readonly OptionType SubCommandGroup = new(nameof(SubCommandGroup));

    /// <summary>
    /// Gets an option type representing a <see cref="string"/>.
    /// </summary>
    public static readonly OptionType StringOption = new(nameof(StringOption));

    /// <summary>
    /// Gets an option type representing a <see cref="int"/>.
    /// </summary>
    public static readonly OptionType IntegerType = new(nameof(IntegerType));

    /// <summary>
    /// Gets an option type representing a <see cref="bool"/>.
    /// </summary>
    public static readonly OptionType BooleanType = new(nameof(BooleanType));

    /// <summary>
    /// Gets an option type representing a floating-point integer (<see cref="float"/>).
    /// </summary>
    public static readonly OptionType FloatType = new(nameof(FloatType));

    /// <summary>
    /// Gets an option type representing a floating-point integer with <see cref="double"/> precision.
    /// </summary>
    public static readonly OptionType DoubleType = new(nameof(DoubleType));
}
