//
//  AbstractCommandBuilder.cs
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
using Remora.Commands.Conditions;
using Remora.Commands.DependencyInjection;

namespace Remora.Commands.Builders;

/// <summary>
/// An abstract builder class with shared functionality/data between commands and groups.
/// </summary>
/// <typeparam name="TSelf">The type of the builder.</typeparam>
public abstract class AbstractCommandBuilder<TSelf> where TSelf : AbstractCommandBuilder<TSelf>
{
    /// <summary>
    /// Gets or sets the description of the group or command.
    /// </summary>
    public string? Description { get; protected set; }

    /// <summary>
    /// Gets the associated list of aliases.
    /// </summary>
    protected List<string> Aliases { get; }

    /// <summary>
    /// Gets the associated list of attributes.
    /// </summary>
    protected List<Attribute> Attributes { get; }

    /// <summary>
    /// Gets the associated list of conditions.
    /// </summary>
    protected List<ConditionAttribute> Conditions { get; }

    /// <summary>
    /// Gets the associated parent node if any.
    /// </summary>
    protected GroupBuilder? Parent { get; }

    /// <summary>
    /// Gets the associated <see cref="TreeRegistrationBuilder"/> for the command or group.
    /// </summary>
    protected TreeRegistrationBuilder? TreeBuilder { get; }

    /// <summary>
    /// Gets or sets the name of the command or group.
    /// </summary>
    protected string Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractCommandBuilder{TSelf}"/> class.
    /// </summary>
    /// <param name="treeBuilder">The registration builder.</param>
    protected AbstractCommandBuilder(TreeRegistrationBuilder treeBuilder)
        : this()
    {
        this.TreeBuilder = treeBuilder;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractCommandBuilder{TSelf}"/> class.
    /// </summary>
    /// <param name="parent">The parent of the builder.</param>
    protected AbstractCommandBuilder(GroupBuilder? parent = null)
    {
        this.Name = string.Empty;
        this.Aliases = new();
        this.Attributes = new();
        this.Conditions = new();
        this.Parent = parent;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractCommandBuilder{TSelf}"/> class.
    /// </summary>
    private AbstractCommandBuilder()
    {
        this.Aliases = new();
        this.Attributes = new();
        this.Conditions = new();
        this.Name = string.Empty;
    }

    /// <summary>
    /// Sets the name of the builder.
    /// </summary>
    /// <param name="name">The name of the builder.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public TSelf WithName(string name)
    {
        this.Name = name;
        return (TSelf)this;
    }

    /// <summary>
    /// Sets the description of the builder.
    /// </summary>
    /// <param name="description">The description of the builder.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public TSelf WithDescription(string description)
    {
        this.Description = description;
        return (TSelf)this;
    }

    /// <summary>
    /// Adds an alias to the builder.
    /// </summary>
    /// <param name="alias">The alias to add.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public TSelf AddAlias(string alias)
    {
        this.Aliases.Add(alias);
        return (TSelf)this;
    }

    /// <summary>
    /// Adds multiple aliases to the builder.
    /// </summary>
    /// <param name="aliases">The aliases to add.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public TSelf AddAliases(IEnumerable<string> aliases)
    {
        this.Aliases.AddRange(aliases);
        return (TSelf)this;
    }

    /// <summary>
    /// Adds an attribute to the builder. Conditions must be added via <see cref="AddCondition"/>.
    /// </summary>
    /// <param name="attribute">The attribute to add.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public TSelf AddAttribute(Attribute attribute)
    {
        if (attribute is ConditionAttribute)
        {
            throw new InvalidOperationException("Conditions must be added via AddCondition.");
        }

        this.Attributes.Add(attribute);
        return (TSelf)this;
    }

    /// <summary>
    /// Adds a condition to the builder.
    /// </summary>
    /// <param name="condition">The condition to add.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public TSelf AddCondition(ConditionAttribute condition)
    {
        this.Conditions.Add(condition);
        return (TSelf)this;
    }
}
