//
//  CommandBuilder.cs
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Attributes;
using Remora.Commands.Conditions;
using Remora.Commands.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Commands.Signatures;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Remora.Commands.Builders;

/// <summary>
/// A builder for commands, which exposes a fluent API.
/// </summary>
public class CommandBuilder
{
    /// <summary>
    /// Gets the parameters of the command.
    /// </summary>
    internal List<CommandParameterBuilder> Parameters { get; }

    /// <summary>
    /// Gets the description of the builder.
    /// </summary>
    internal string? Description { get; private set; }

    private readonly List<string> _aliases;
    private readonly List<Attribute> _attributes;

    private readonly List<ConditionAttribute> _conditions;

    private readonly GroupBuilder? _parent;
    private readonly TreeRegistrationBuilder? _treeBuilder;

    private string _name;
    private CommandInvocation? _invocation;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandBuilder"/> class.
    /// </summary>
    /// <param name="parent">The parent of the command.</param>
    public CommandBuilder(GroupBuilder? parent = null)
    {
        _name = string.Empty;
        _aliases = new();
        _attributes = new();
        Parameters = new();
        _conditions = new();

        _parent = parent;

        _parent?.Children.Add(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandBuilder"/> class.
    /// </summary>
    /// <param name="treeBuilder">The registration builder.</param>
    public CommandBuilder(TreeRegistrationBuilder treeBuilder)
        : this()
    {
        _treeBuilder = treeBuilder;
    }

    /// <summary>
    /// Sets the name of the command.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public CommandBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the description of the command.
    /// </summary>
    /// <param name="description">The description of the command.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public CommandBuilder WithDescription(string description)
    {
        Description = description;
        return this;
    }

    /// <summary>
    /// Adds an alias to the command.
    /// </summary>
    /// <param name="alias">The alias to add.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public CommandBuilder AddAlias(string alias)
    {
        _aliases.Add(alias);
        return this;
    }

    /// <summary>
    /// Adds multiple aliases to the command.
    /// </summary>
    /// <param name="aliases">The aliases to add.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public CommandBuilder AddAliases(IEnumerable<string> aliases)
    {
        _aliases.AddRange(aliases);
        return this;
    }

    /// <summary>
    /// Adds an attribute to the command. Conditions must be added via <see cref="AddCondition"/>.
    /// </summary>
    /// <param name="attribute">The attribute to add.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public CommandBuilder AddAttribute(Attribute attribute)
    {
        if (attribute is ConditionAttribute)
        {
            throw new InvalidOperationException("Conditions must be added via AddCondition.");
        }

        _attributes.Add(attribute);
        return this;
    }

    /// <summary>
    /// Adds a condition to the command.
    /// </summary>
    /// <param name="condition">The condition to add.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public CommandBuilder AddCondition(ConditionAttribute condition)
    {
        _conditions.Add(condition);
        return this;
    }

    /// <summary>
    /// Applies a function in which the command is invoked by.
    /// </summary>
    /// <param name="invokeFunc">The function to invoke the command, or the command itself.</param>
    /// <remarks>This method MUST be called before <see cref="Build"/>.</remarks>
    /// <returns>The current builder to chain calls with.</returns>
    public CommandBuilder WithInvocation(CommandInvocation invokeFunc)
    {
        _invocation = invokeFunc;
        return this;
    }

    /// <summary>
    /// Adds a new parameter to the command.
    /// </summary>
    /// <param name="type">The optional type of the parameter.</param>
    /// <returns>The parameter builder to build the parameter with.</returns>
    public CommandParameterBuilder AddParameter(Type? type = null)
    {
        var parameterBuilder = new CommandParameterBuilder(this, type);
        Parameters.Add(parameterBuilder);
        return parameterBuilder;
    }

    /// <summary>
    /// Creates a <see cref="CommandBuilder"/> from a method.
    /// </summary>
    /// <param name="parent">The parent builder, if applicable.</param>
    /// <param name="info">The method to extract from.</param>
    /// <returns>The builder.</returns>
    public static CommandBuilder FromMethod(GroupBuilder? parent, MethodInfo info)
    {
        var builder = new CommandBuilder(parent);

        var commandAttribute = info.GetCustomAttribute<CommandAttribute>()!;

        builder.WithName(commandAttribute.Name);
        builder.AddAliases(commandAttribute.Aliases);

        var descriptionAttribute = info.GetCustomAttribute<DescriptionAttribute>();

        if (descriptionAttribute is not null)
        {
            builder.WithDescription(descriptionAttribute.Description);
        }

        var parameters = info.GetParameters();
        var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

        builder.WithInvocation(CommandTreeBuilder.CreateDelegate(info, parameterTypes));

        foreach (var parameter in parameters)
        {
            var parameterBuilder = builder.AddParameter(parameter.ParameterType);
            parameterBuilder.WithName(parameter.Name!);

            var description = parameter.GetCustomAttribute<DescriptionAttribute>();
            if (description is not null)
            {
                parameterBuilder.WithDescription(description.Description);
            }

            if (parameter.HasDefaultValue)
            {
                parameterBuilder.WithDefaultValue(parameter.DefaultValue);
            }

            parameter.GetAttributesAndConditions(out var attributes, out var conditions);

            foreach (var attribute in attributes)
            {
                parameterBuilder.AddAttribute(attribute);
            }

            foreach (var condition in conditions)
            {
                parameterBuilder.AddCondition(condition);
            }

            var switchOrOptionAttribute = parameter.GetCustomAttribute<OptionAttribute>();

            if (switchOrOptionAttribute is SwitchAttribute sa)
            {
                if (parameter.ParameterType != typeof(bool))
                {
                    throw new InvalidOperationException("Switches must be of type bool.");
                }

                if (!parameter.HasDefaultValue)
                {
                    throw new InvalidOperationException("Switches must have a default value.");
                }

                parameterBuilder.IsSwitch((bool)parameter.DefaultValue!, sa.ShortName, sa.LongName);
            }
            else if (switchOrOptionAttribute is OptionAttribute oa)
            {
                parameterBuilder.IsOption(oa.ShortName, oa.LongName);
            }

            var greedyAttribute = parameter.GetCustomAttribute<GreedyAttribute>();

            if (greedyAttribute is not null)
            {
                parameterBuilder.IsGreedy();
            }
        }

        // Alternatively check if the builder is null? Expected case is from Remora.Commands invoking this,
        // in which case it's expected that a group builder is ALWAYS passed if the command is within a group.
        if (!info.DeclaringType!.TryGetGroupName(out _))
        {
            info.DeclaringType!.GetAttributesAndConditions(out var attributes, out var conditions);

            builder._attributes.AddRange(attributes);
            builder._conditions.AddRange(conditions);
        }

        info.GetAttributesAndConditions(out var methodAttributes, out var methodConditions);

        builder._attributes.AddRange(methodAttributes);
        builder._conditions.AddRange(methodConditions);

        return builder;
    }

    /// <summary>
    /// Builds the current <see cref="CommandBuilder"/> into a <see cref="CommandNode"/>.
    /// </summary>
    /// <param name="parent">The parrent of the constructed command.</param>
    /// <returns>The built <see cref="CommandNode"/>.</returns>
    public CommandNode Build(IParentNode parent)
    {
        if (_invocation is not { } invoke)
        {
            throw new InvalidOperationException("Cannot create a command without an entrypoint.");
        }

        var shape = CommandShape.FromBuilder(this);

        return new CommandNode
        (
            parent,
            _name,
            invoke,
            shape,
            _aliases,
            _attributes,
            _conditions
        );
    }

    /// <summary>
    /// Finishes building the command, and returns the group builder if applicable.
    /// This method should only be called if the instance was generated from <see cref="GroupBuilder.AddCommand"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the command builder was not associated with a group.</exception>
    /// <returns>The parent builder.</returns>
    public GroupBuilder Finish()
    {
        return _parent ?? throw new InvalidOperationException("The command builder was not attatched to a group.");
    }
}
