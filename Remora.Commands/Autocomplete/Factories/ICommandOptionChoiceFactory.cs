//
//  ICommandOptionChoiceFactory.cs
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
using System.Globalization;

namespace Remora.Commands.Autocomplete.Factories;

/// <summary>
/// Represents a factory that constructs instances of <see cref="ICommandOptionChoice{T}"/>.
/// </summary>
/// <typeparam name="TUnderlyingType">The underlying type of the <see cref="ICommandOptionChoice{T}"/>.</typeparam>
public interface ICommandOptionChoiceFactory<in TUnderlyingType>
{
    /// <summary>
    /// Creates a new instance of the <see cref="TCommandOptionChoice"/> class.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value of the option.</param>
    /// <param name="translations">A set of translated values mapped to their corresponding culture.</param>
    /// <typeparam name="TCommandOptionChoice">The implementation of <see cref="ICommandOptionChoice{T}"/>.</typeparam>
    /// <returns>A new instance of the <see cref="TCommandOptionChoice"/> class.</returns>
    TCommandOptionChoice Create<TCommandOptionChoice>
    (
        string name,
        TUnderlyingType value,
        IReadOnlyDictionary<CultureInfo, string>? translations
    )
        where TCommandOptionChoice : class, ICommandOptionChoice<TUnderlyingType>;
}
