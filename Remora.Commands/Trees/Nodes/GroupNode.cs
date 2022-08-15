//
//  GroupNode.cs
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

namespace Remora.Commands.Trees.Nodes;

/// <summary>
/// Represents a command group.
/// </summary>
/// <remarks>
/// Command groups may contain either other groups for deeper nesting, or leaf nodes in the form of commands.
/// </remarks>
[PublicAPI]
public class GroupNode : IParentNode, IChildNode
{
    /// <summary>
    /// Gets a list of the <see cref="Groups.CommandGroup"/> types that make up this group.
    /// </summary>
    public IReadOnlyList<Type> GroupTypes { get; }

    /// <inheritdoc/>
    public IReadOnlyList<IChildNode> Children { get; }

    /// <inheritdoc/>
    public IParentNode Parent { get; }

    /// <inheritdoc/>
    /// <remarks>
    /// This key represents the name of the group, from which command prefixes are formed.
    /// </remarks>
    public string Key { get; }

    /// <inheritdoc />
    public IReadOnlyList<string> Aliases { get; }

    /// <summary>
    /// Gets a user-configured description of the group.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupNode"/> class.
    /// </summary>
    /// <param name="groupTypes">The <see cref="Groups.CommandGroup"/> types that make up this group.</param>
    /// <param name="children">The child nodes of the group node.</param>
    /// <param name="parent">The parent of the group node.</param>
    /// <param name="key">The key value for the group node.</param>
    /// <param name="aliases">Additional key aliases, if any.</param>
    /// <param name="description">The description of the group.</param>
    public GroupNode
    (
        IReadOnlyList<Type> groupTypes,
        IReadOnlyList<IChildNode> children,
        IParentNode parent,
        string key,
        IReadOnlyList<string>? aliases = null,
        string? description = null
    )
    {
        this.GroupTypes = groupTypes;
        this.Children = children;
        this.Parent = parent;
        this.Key = key;
        this.Aliases = aliases ?? Array.Empty<string>();
        this.Description = description ?? Constants.DefaultDescription;
    }
}
