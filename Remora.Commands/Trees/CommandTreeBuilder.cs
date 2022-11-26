//
//  CommandTreeBuilder.cs
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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Remora.Commands.Attributes;
using Remora.Commands.Conditions;
using Remora.Commands.Extensions;
using Remora.Commands.Groups;
using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Remora.Commands.Trees;

/// <summary>
/// Builds command trees from input command group types.
/// </summary>
[PublicAPI]
public class CommandTreeBuilder
{
    private readonly List<Type> _registeredModuleTypes = new();

    /// <summary>
    /// Registers a module type with the builder.
    /// </summary>
    /// <typeparam name="TModule">The module type.</typeparam>
    public void RegisterModule<TModule>() where TModule : CommandGroup
    {
        RegisterModule(typeof(TModule));
    }

    /// <summary>
    /// Registers a module type with the builder.
    /// </summary>
    /// <param name="commandModule">The module type.</param>
    public void RegisterModule(Type commandModule)
    {
        if (!_registeredModuleTypes.Contains(commandModule))
        {
            _registeredModuleTypes.Add(commandModule);
        }
    }

    /// <summary>
    /// Builds a command tree from the registered types.
    /// </summary>
    /// <returns>The command tree.</returns>
    public CommandTree Build()
    {
        var rootChildren = new List<IChildNode>();
        var rootNode = new RootNode(rootChildren);
        rootChildren.AddRange(ToChildNodes(_registeredModuleTypes, rootNode));

        return new CommandTree(rootNode);
    }

    /// <summary>
    /// Parses the given list of module types into a set of child nodes.
    /// </summary>
    /// <remarks>
    /// Child nodes can be either <see cref="GroupNode"/> or <see cref="CommandNode"/> instances, where methods in
    /// the types that have been marked as commands produce command nodes, and nested classes produce group nodes.
    ///
    /// If a nested class does not have a <see cref="GroupAttribute"/>, its subtypes and methods are instead
    /// parented to the containing type.
    /// </remarks>
    /// <param name="moduleTypes">The module types.</param>
    /// <param name="parent">The parent node. For the first invocation, this will be the root node.</param>
    /// <returns>The new children of the parent.</returns>
    private IEnumerable<IChildNode> ToChildNodes(IEnumerable<Type> moduleTypes, IParentNode parent)
    {
        IEnumerable<IGrouping<string, Type>> groups = moduleTypes.GroupBy
        (
            mt => mt.TryGetGroupName(out var name) ? name : string.Empty
        );

        foreach (var group in groups)
        {
            if (group.Key == string.Empty)
            {
                // Nest these commands and groups directly under the parent
                foreach (var groupType in group)
                {
                    var subgroups = groupType.GetNestedTypes().Where(t => typeof(CommandGroup).IsAssignableFrom(t));

                    // Extract submodules and commands
                    foreach (var child in GetModuleCommands(groupType, parent))
                    {
                        yield return child;
                    }

                    foreach (var child in ToChildNodes(subgroups, parent))
                    {
                        yield return child;
                    }
                }
            }
            else
            {
                // Nest these commands and groups under a subgroup
                var groupChildren = new List<IChildNode>();
                var groupAliases = new List<string>();

                // Pick out the first custom description
                var description = group
                    .Select(t => t.GetDescriptionOrDefault("MARKER"))
                    .Distinct()
                    .FirstOrDefault(d => d != "MARKER");

                description ??= Constants.DefaultDescription;

                var attributes = group.SelectMany(t => t.GetCustomAttributes<Attribute>()
                                                        .Where(att => !typeof(ConditionAttribute).IsAssignableFrom(att.GetType())))
                                      .ToArray();

                var conditions = group.SelectMany(t => t.GetCustomAttributes<ConditionAttribute>()).ToArray();

                var groupNode = new GroupNode(group.ToArray(), groupChildren, parent, group.Key, groupAliases, attributes, conditions, description);

                foreach (var groupType in group)
                {
                    // Union the aliases of the groups under this key
                    var groupAttribute = groupType.GetCustomAttribute<GroupAttribute>();
                    if (groupAttribute is null)
                    {
                        throw new InvalidOperationException();
                    }

                    foreach (var alias in groupAttribute.Aliases)
                    {
                        if (groupAliases.Contains(alias))
                        {
                            continue;
                        }

                        groupAliases.Add(alias);
                    }

                    var subgroups = groupType.GetNestedTypes().Where(t => typeof(CommandGroup).IsAssignableFrom(t));

                    // Extract submodules and commands
                    groupChildren.AddRange(GetModuleCommands(groupType, groupNode));
                    groupChildren.AddRange(ToChildNodes(subgroups, groupNode));
                }

                yield return groupNode;
            }
        }
    }

    /// <summary>
    /// Parses a set of command nodes from the given type.
    /// </summary>
    /// <param name="moduleType">The module type.</param>
    /// <param name="parent">The parent node.</param>
    /// <returns>A set of command nodes.</returns>
    private IEnumerable<CommandNode> GetModuleCommands(Type moduleType, IParentNode parent)
    {
        var methods = moduleType.GetMethods();
        foreach (var method in methods)
        {
            var commandAttribute = method.GetCustomAttribute<CommandAttribute>();
            if (commandAttribute is null)
            {
                continue;
            }

            if (!method.ReturnType.IsSupportedCommandReturnType())
            {
                throw new InvalidOperationException
                (
                    $"Methods marked as commands must return a {typeof(Task<>)} or {typeof(ValueTask<>)}, " +
                    $"containing a type that implements {typeof(IResult)}."
                );
            }

            yield return new CommandNode
            (
                parent,
                commandAttribute.Name,
                moduleType,
                method,
                commandAttribute.Aliases
            );
        }
    }
}
