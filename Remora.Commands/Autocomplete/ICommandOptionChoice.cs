//
//  ICommandOptionChoice.cs
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

namespace Remora.Commands.Autocomplete;

/// <summary>
/// Represents a choice available to a user.
/// </summary>
/// <typeparam name="T">The underlying type of the choice.</typeparam>
public interface ICommandOptionChoice<out T>
{
    /// <summary>
    /// Gets the name of the choice.
    /// </summary>
    /// <remarks>This can be up to 100 characters.</remarks>
    string Name { get; }

    /// <summary>
    /// Gets the value of the choice.
    /// </summary>
    T Value { get; }
}
