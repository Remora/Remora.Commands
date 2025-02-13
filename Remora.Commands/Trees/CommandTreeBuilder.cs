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
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Builders;
using Remora.Commands.Conditions;
using Remora.Commands.Extensions;
using Remora.Commands.Groups;
using Remora.Commands.Signatures;
using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Remora.Commands.Trees;

/// <summary>
/// Builds command trees from input command group types.
/// </summary>
[PublicAPI]
public class CommandTreeBuilder
{
    private readonly List<Type> _registeredModuleTypes = [];
    private readonly List<OneOf<CommandBuilder, GroupBuilder>> _registeredBuilders = [];

    private static readonly MethodInfo _getServiceMethodInfo = typeof(IServiceProvider).GetMethod
    (
        nameof(IServiceProvider.GetService),
        BindingFlags.Instance | BindingFlags.Public
    )!;

    private static readonly MethodInfo _setCancellationTokenMethodInfo = typeof(CommandGroup).GetMethod
    (
        nameof(CommandGroup.SetCancellationToken),
        BindingFlags.Instance | BindingFlags.NonPublic
    )!;

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
    /// Registers a command with the builder.
    /// </summary>
    /// <param name="builder">The builder to register.</param>
    public void RegisterNodeBuilder(OneOf<CommandBuilder, GroupBuilder> builder)
    {
        _registeredBuilders.Add(builder);
    }

    /// <summary>
    /// Builds a command tree from the registered types.
    /// </summary>
    /// <returns>The command tree.</returns>
    public CommandTree Build()
    {
        var rootChildren = new List<IChildNode>();
        var rootNode = new RootNode(rootChildren);

        var builtCommands = _registeredBuilders.Select(rb => rb.Match(cb => cb.Build(rootNode), gb => (IChildNode)gb.Build(rootNode)));
        var topLevelCommands = BindDynamicCommands(ToChildNodes(_registeredModuleTypes, rootNode).ToArray(), builtCommands.ToList(), rootNode);

        rootChildren.AddRange(topLevelCommands);

        return new CommandTree(rootNode);
    }

    /// <summary>
    /// Recursively binds dynamic commands (constructed from builders) to their compile-time type counterparts if they
    /// exist.
    /// </summary>
    /// <param name="nodes">The nodes to bind to.</param>
    /// <param name="values">The values to bind.</param>
    /// <param name="root">The root node to bind groups and commands to.</param>
    /// <returns>Any nodes that could not be bound, and thusly should be added directly to the root as-is.</returns>
    private IEnumerable<IChildNode> BindDynamicCommands
    (
        IReadOnlyList<IChildNode> nodes,
        List<IChildNode> values,
        IParentNode root
    )
    {
        if (!values.Any())
        {
            foreach (var node in nodes)
            {
                yield return node;
            }
            yield break;
        }

        for (var i = values.Count - 1; i >= 0; i--)
        {
            var current = values[i];

            // Return top-level commands (non-group nodes) as-is.
            if (current is IParentNode)
            {
                continue;
            }

            values.RemoveAt(i);
            yield return current;
        }

        if (values.Count is 0)
        {
            yield break;
        }

        var groups = nodes.OfType<GroupNode>()
                           .Concat(values.Cast<GroupNode>())
                           .GroupBy(g => g.Key)
                           .Select(g => MergeRecursively(g.ToArray(), root));

        foreach (var group in groups)
        {
            yield return group;
        }
    }

    private GroupNode MergeRecursively(IReadOnlyList<IChildNode> children, IParentNode parent)
    {
        var childNodes = new List<IChildNode>();
        var groupNodesFromChildren = children.OfType<GroupNode>().ToArray();

        var name = groupNodesFromChildren.Select(g => g.Key).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? string.Empty;
        var description = groupNodesFromChildren.Select(g => g.Description).FirstOrDefault(d => !string.IsNullOrWhiteSpace(d)) ?? Constants.DefaultDescription;

        var group = new GroupNode
        (
            groupNodesFromChildren.SelectMany(t => t.GroupTypes).ToArray(),
            childNodes,
            parent,
            name,
            groupNodesFromChildren.SelectMany(g => g.Aliases).ToArray(),
            groupNodesFromChildren.SelectMany(g => g.Attributes).ToArray(),
            groupNodesFromChildren.SelectMany(g => g.Conditions).ToArray(),
            description
        );

        var mutableChildren = children.SelectMany(n => n is GroupNode gn ? gn.Children : [n]).ToList();

        for (var i = children.Count - 1; i >= 0; i--)
        {
            var child = children[i];

            if (child is not GroupNode cgn)
            {
                childNodes.Add(child);
                mutableChildren.RemoveAt(i);
                continue;
            }

            // Parity with ToChildNodes; if the group's name is empty, or
            // shouldn't be merged, just nest it under the parent.
            if (!string.IsNullOrWhiteSpace(cgn.Key) && name == child.Key)
            {
                continue;
            }

            childNodes.AddRange(cgn.Children);
            mutableChildren.RemoveAt(i);
        }

        var groups = mutableChildren.GroupBy(g => g.Key);

        foreach (var subgroup in groups)
        {
            if (subgroup.Count() is 1)
            {
                childNodes.Add(subgroup.Single());
            }
            else
            {
                childNodes.Add(MergeRecursively(subgroup.ToArray(), group));
            }
        }

        return group;
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

                var attributes = group.SelectMany(t => t.GetCustomAttributes(true).Cast<Attribute>().Where(att => att is not ConditionAttribute)).ToArray();
                var conditions = group.SelectMany(t => t.GetCustomAttributes<ConditionAttribute>()).ToArray();

                if (group.First().DeclaringType is { } parentType && !parentType.TryGetGroupName(out _))
                {
                    // If the group is being hoisted, take the attributes of the parent type(s).
                    ExtractExtraAttributes(parentType, out var extraAttributes, out var extraConditions);

                    attributes = extraAttributes.Concat(attributes).ToArray();
                    conditions = extraConditions.Concat(conditions).ToArray();
                }

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
    /// Extracts attributes and conditions from the given type and its parent types.
    /// </summary>
    /// <param name="parentType">The type to begin extracting attributes from.</param>
    /// <param name="attributes">The extracted attributes, in descending order.</param>
    /// <param name="conditions">The extracted conditions, in descending order.</param>
    private static void ExtractExtraAttributes(Type parentType, out IEnumerable<Attribute> attributes, out IEnumerable<ConditionAttribute> conditions)
    {
        var parentGroupType = parentType;

        var extraAttributes = new List<Attribute>();
        var extraConditions = new List<ConditionAttribute>();

        attributes = extraAttributes;
        conditions = extraConditions;

        do
        {
            if (parentGroupType.TryGetGroupName(out _))
            {
                break;
            }

            parentGroupType.GetAttributesAndConditions(out var parentAttributes, out var parentConditions);

            extraAttributes.AddRange(parentAttributes.Reverse());
            extraConditions.AddRange(parentConditions.Reverse());
        }
        while ((parentGroupType = parentGroupType!.DeclaringType) is not null);

        // These are inserted in reverse order as we traverse up the
        // inheritance tree, so re-reversing the list gives us all attributes
        // in the correct order, *decescending* down the tree, effectively.
        extraAttributes.Reverse();
        extraConditions.Reverse();
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
        var isInUnnamedGroup = !moduleType.TryGetGroupName(out var gn) || gn == string.Empty;

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

            method.GetAttributesAndConditions(out var attributes, out var conditions);

            if (isInUnnamedGroup)
            {
                // If the group is being hoisted, take the attributes of the parent type(s).
                ExtractExtraAttributes(moduleType, out var extraAttributes, out var extraConditions);

                attributes = extraAttributes.Concat(attributes).ToArray();
                conditions = extraConditions.Concat(conditions).ToArray();
            }

            yield return new CommandNode
            (
                parent,
                commandAttribute.Name,
                CreateDelegate(method),
                CommandShape.FromMethod(method),
                commandAttribute.Aliases,
                attributes,
                conditions
            );
        }
    }

    /// <summary>
    /// Creates a command invocation delegate from the supplied parameters.
    /// </summary>
    /// <param name="method">The method that will be invoked.</param>
    /// <returns>The created command invocation.</returns>
    internal static CommandInvocation CreateDelegate(MethodInfo method)
    {
        // Get the object from the container
        var serviceProvider = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");

        var instance = Expression.Call(serviceProvider, _getServiceMethodInfo, Expression.Constant(method.DeclaringType));

        var parameters = Expression.Parameter(typeof(object?[]), "parameters");

        var methodParameters = method.GetParameters();
        var orderedArgumentTypes = methodParameters.Select((t, n) => (t, n))
                                                .OrderBy(tn => tn.t.GetCustomAttribute<OptionAttribute>() is null)
                                                .Select((t, nn) => (t.t, nn))
                                                .ToArray();

        // Create the arguments
        var arguments = new Expression[orderedArgumentTypes.Length];
        for (var i = 0; i < orderedArgumentTypes.Length; i++)
        {
            var argumentType = methodParameters[i].ParameterType;
            var argumentIndex = orderedArgumentTypes.First(tn => tn.t == methodParameters[i]).nn;
            var argument = Expression.ArrayIndex(parameters, Expression.Constant(argumentIndex));
            arguments[i] = Expression.Convert(argument, argumentType);
        }

        var castedInstance = Expression.Convert(instance, method.DeclaringType!);

        var call = Expression.Call(castedInstance, method, arguments);

        // Convert the result to a ValueTask<IResult>
        call = CoerceToValueTask(call);

        var ct = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var block = Expression.Block
        (
            Expression.Call(castedInstance, _setCancellationTokenMethodInfo, ct),
            call
        );

        // Compile the expression
        var lambda = Expression.Lambda<CommandInvocation>(block, serviceProvider, parameters, ct);

        return lambda.Compile();
    }

    /// <summary>
    /// Coerces the static result type of an expression to a <see cref="ValueTask{IResult}"/>.
    /// <list type="bullet">
    /// <item>If the type is <see cref="ValueTask{T}"/>, returns the expression as-is</item>
    /// <item>If the type is <see cref="Task{T}"/>, returns an expression wrapping the Task in a <see cref="ValueTask{IResult}"/></item>
    /// <item>Otherwise, throws <see cref="InvalidOperationException"/></item>
    /// </list>
    /// </summary>
    /// <param name="expression">The input expression.</param>
    /// <returns>The new expression.</returns>
    /// <exception cref="InvalidOperationException">If the type of <paramref name="expression"/> is not wrappable.</exception>
    public static MethodCallExpression CoerceToValueTask(Expression expression)
    {
        var expressionType = expression.Type;

        MethodCallExpression invokerExpr;
        if (expressionType.IsConstructedGenericType && expressionType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            invokerExpr = Expression.Call(_toResultValueTaskInfo.MakeGenericMethod(expressionType.GetGenericArguments()[0]), expression);
        }
        else if (expressionType.IsConstructedGenericType && expressionType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            invokerExpr = Expression.Call(_toResultTaskInfo.MakeGenericMethod(expressionType.GetGenericArguments()[0]), expression);
        }
        else
        {
            throw new InvalidOperationException($"{nameof(CoerceToValueTask)} expression must be {nameof(Task<IResult>)} or {nameof(ValueTask<IResult>)}");
        }

        return invokerExpr;
    }

    private static async ValueTask<IResult> ToResultValueTask<T>(ValueTask<T> task) where T : IResult
        => await task;

    private static async ValueTask<IResult> ToResultTask<T>(Task<T> task) where T : IResult
        => await task;

    private static readonly MethodInfo _toResultValueTaskInfo
        = typeof(CommandTreeBuilder).GetMethod(nameof(ToResultValueTask), BindingFlags.Static | BindingFlags.NonPublic)
          ?? throw new InvalidOperationException($"Did not find {nameof(ToResultValueTask)}");

    private static readonly MethodInfo _toResultTaskInfo
        = typeof(CommandTreeBuilder).GetMethod(nameof(ToResultTask), BindingFlags.Static | BindingFlags.NonPublic)
          ?? throw new InvalidOperationException($"Did not find {nameof(ToResultTask)}");
}
