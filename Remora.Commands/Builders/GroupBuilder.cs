//
//  GroupBuilder.cs
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
using OneOf;
using Remora.Commands.Conditions;
using Remora.Commands.Trees.Nodes;

namespace Remora.Commands.Builders;

/// <summary>
/// A builder class for creating <see cref="GroupNode"/>s.
/// </summary>
public class GroupBuilder
{
    private readonly GroupBuilder? _parent;
    private readonly List<string> _groupAliases;
    private readonly List<Attribute> _groupAttributes;
    private readonly List<ConditionAttribute> _groupConditions;
    private readonly List<OneOf<CommandBuilder, GroupBuilder>> _children;

    private string _name;
    private string? _description;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupBuilder"/> class.
    /// </summary>
    /// <param name="parent">The parent this group belongs to, if any.</param>
    public GroupBuilder(GroupBuilder? parent = null)
    {
        _name = string.Empty;
        _parent = parent;
        _groupAliases = new List<string>();
        _groupAttributes = new List<Attribute>();
        _groupConditions = new List<ConditionAttribute>();
        _children = new List<OneOf<CommandBuilder, GroupBuilder>>();
    }

    /// <summary>
    /// Sets the name of the group.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public GroupBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the description of the group.
    /// </summary>
    /// <param name="description">The description of the group.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public GroupBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Adds an alias to the group.
    /// </summary>
    /// <param name="alias">The alias to add.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public GroupBuilder AddAlias(string alias)
    {
        _groupAliases.Add(alias);
        return this;
    }

    /// <summary>
    /// Adds multiple aliases to the group.
    /// </summary>
    /// <param name="aliases">The aliases to add.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public GroupBuilder AddAliases(IEnumerable<string> aliases)
    {
        _groupAliases.AddRange(aliases);
        return this;
    }

    /// <summary>
    /// Adds an attribute to the group. Conditions should be added via <see cref="AddCondition"/>.
    /// </summary>
    /// <param name="attribute">The attribute to add.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public GroupBuilder AddAttribute(Attribute attribute)
    {
        if (attribute is ConditionAttribute)
        {
            throw new InvalidOperationException("Conditions should be added via AddCondition.");
        }

        _groupAttributes.Add(attribute);
        return this;
    }

    /// <summary>
    /// Adds a condition to the group.
    /// </summary>
    /// <param name="condition">The condition to add.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public GroupBuilder AddCondition(ConditionAttribute condition)
    {
        _groupConditions.Add(condition);
        return this;
    }

    /// <summary>
    /// Adds a command to the group.
    /// </summary>
    /// <returns>A <see cref="CommandBuilder"> to build the command with</returns>.
    public CommandBuilder AddCommand()
    {
        return new CommandBuilder(this);
    }

    /// <summary>
    /// Adds another group to the group builder.
    /// </summary>
    /// <returns>The current builder to chain calls with.</returns>
    public GroupBuilder AddGroup()
    {
        var builder = new GroupBuilder(this);
        _children.Add(OneOf<CommandBuilder, GroupBuilder>.FromT1(builder)); // new() would also work.

        return builder;
    }

    /// <summary>
    /// Recursively builds the current <see cref="GroupBuilder"/> into a <see cref="GroupNode"/>.
    /// </summary>
    /// <param name="parent">The parent of the group.</param>
    /// <returns>The built group node.</returns>
    public GroupNode Build(IParentNode parent)
    {
        var children = new List<IChildNode>();

        var node = new GroupNode
        (
         Type.EmptyTypes,
         children,
         parent,
         _name,
         _groupAliases,
         _groupAttributes,
         _groupConditions,
         _description
        );

        foreach (var child in _children)
        {
            children.Add(child.Match<IChildNode>
            (
                commandBuilder => commandBuilder.Build(node),
                groupBuilder => groupBuilder.Build(node)
            ));
        }

        return node;
    }
}
