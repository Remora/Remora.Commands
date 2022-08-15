//
//  GroupAttribute.cs
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

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Remora.Commands.Attributes;

/// <summary>
/// Represents the name of a command group.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public class GroupAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the group.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the aliases for the group.
    /// </summary>
    public IReadOnlyList<string> Aliases { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <param name="aliases">Additional aliases for the group.</param>
    public GroupAttribute(string name, params string[] aliases)
    {
        this.Name = name;
        this.Aliases = aliases;
    }
}
