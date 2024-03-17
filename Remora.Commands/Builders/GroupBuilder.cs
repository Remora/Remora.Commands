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
using System.ComponentModel;
using System.Threading.Tasks;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Conditions;
using Remora.Commands.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Commands.Groups;
using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Remora.Commands.Builders;

/// <summary>
/// A builder class for creating <see cref="GroupNode"/>s.
/// </summary>
public class GroupBuilder : AbstractCommandBuilder<GroupBuilder>
{
    private List<Type?> _groupTypes;

    /// <summary>
    /// Gets the children of the group.
    /// </summary>
    internal List<OneOf<CommandBuilder, GroupBuilder>> Children { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupBuilder"/> class.
    /// </summary>
    /// <param name="parent">The parent this group belongs to, if any.</param>
    public GroupBuilder(GroupBuilder? parent = null)
        : base(parent)
    {
        this.Name = string.Empty;
        this.Children = new List<OneOf<CommandBuilder, GroupBuilder>>();
        this._groupTypes = new List<Type?>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupBuilder"/> class.
    /// </summary>
    /// <param name="treeBuilder">The registration builder.</param>
    public GroupBuilder(TreeRegistrationBuilder treeBuilder)
        : base(treeBuilder)
    {
        this.Name = string.Empty;
        this.Children = new List<OneOf<CommandBuilder, GroupBuilder>>();
        this._groupTypes = new List<Type?>();
    }

    /// <summary>
    /// Adds a command to the group.
    /// </summary>
    /// <returns>A <see cref="CommandBuilder"/> to build the command with.</returns>
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
        this.Children.Add(builder);

        return builder;
    }

    /// <summary>
    /// Returns the parent builder of the current group.
    /// </summary>
    /// <returns>The parent builder of the group, if applicable.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the group does not belong to a parent.</exception>
    /// <remarks>This method should only be called if the builder was instantiated from a call to <see cref="AddGroup"/>.</remarks>
    public GroupBuilder Complete()
    {
        if (Parent is null)
        {
            throw new InvalidOperationException("Cannot complete a group that has no parent.");
        }

        return Parent;
    }

    /// <summary>
    /// Returns the registration builder of the current group.
    /// </summary>
    /// <returns>The <see cref="TreeRegistrationBuilder"/> that created this builder, if applicable.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the group was not created via a registration builder.</exception>
    /// <remarks>This method should only be called if the builder was instnatiated from a call to <see cref="TreeRegistrationBuilder.CreateCommandGroup"/>.</remarks>
    public TreeRegistrationBuilder Finish()
    {
        if (TreeBuilder is null)
        {
            throw new InvalidOperationException("Cannot complete a group that has no parent.");
        }

        return TreeBuilder;
    }

    /// <summary>
    /// Constructs a <see cref="GroupBuilder"/> from a given module type.
    /// </summary>
    /// <param name="moduleType">The type of the module to construct from.</param>
    /// <param name="parent">The parent of the builder, if applicable.</param>
    /// <returns>The constructed builder.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a command method is marked with
    /// <see cref="CommandAttribute"/> but the return type of the command is not supported.</exception>
    public static GroupBuilder FromType(Type moduleType, GroupBuilder? parent = null)
    {
        var builder = new GroupBuilder(parent);

        builder._groupTypes = new List<Type?> { moduleType };

        var groupAttribute = moduleType.GetCustomAttribute<GroupAttribute>()!;

        builder.WithName(groupAttribute.Name);

        builder.AddAliases(groupAttribute.Aliases);

        var description = moduleType.GetCustomAttribute<DescriptionAttribute>();

        if (description is not null)
        {
            builder.WithDescription(description.Description);
        }

        moduleType.GetAttributesAndConditions(out var attributes, out var conditions);

        builder.Attributes.AddRange(attributes);
        builder.Conditions.AddRange(conditions);

        foreach (var childMethod in moduleType.GetMethods())
        {
            var commandAttribute = childMethod.GetCustomAttribute<CommandAttribute>();

            if (commandAttribute is null)
            {
                continue;
            }

            if (!childMethod.ReturnType.IsSupportedCommandReturnType())
            {
                throw new InvalidOperationException
                (
                    $"Methods marked as commands must return a {typeof(Task<>)} or {typeof(ValueTask<>)}, " +
                    $"containing a type that implements {typeof(IResult)}."
                );
            }

            var commandBuilder = CommandBuilder.FromMethod(builder, childMethod);
            builder.Children.Add(commandBuilder);
        }

        foreach (var childType in moduleType.GetNestedTypes())
        {
            if (!childType.TryGetGroupName(out _) || !childType.IsSubclassOf(typeof(CommandGroup)))
            {
                continue;
            }

            var childGroup = FromType(childType, builder);
            builder.Children.Add(childGroup);
        }

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
            this.Name,
            this.Aliases,
            this.Attributes,
            this.Conditions,
            this.Description
        );

        foreach (var child in Children)
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
