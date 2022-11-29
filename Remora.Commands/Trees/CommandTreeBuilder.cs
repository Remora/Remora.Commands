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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly List<Type> _registeredModuleTypes = new();
    private readonly List<OneOf<CommandBuilder, GroupBuilder>> _registeredBuilders = new();

    private static readonly MethodInfo GetServiceMethodInfo = GetMethodInfo<Func<IServiceProvider, Type, object?>>((p,   t) => p.GetService(t));
    private static readonly MethodInfo SetCancellationTokenMethodInfo = GetMethodInfo<Action<CommandGroup, CancellationToken>>((g, c) => g.SetCancellationToken(c));

    /// <summary>
    /// Gets a <see cref="MethodInfo"/> of the given type.
    /// </summary>
    /// <param name="expression">The expression to retrive the method info from.</param>
    /// <typeparam name="T">The delegate type.</typeparam>
    /// <returns>The <see cref="MethodInfo"/> of the called method.</returns>
    private static MethodInfo GetMethodInfo<T>(Expression<T> expression)
    {
        return ((MethodCallExpression)expression.Body).Method;
    }

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

        var builtCommands = _registeredBuilders.Select(rb => rb.Match(cb => cb.Build(rootNode), gb => (IChildNode)gb.Build(rootNode)));
        rootChildren.AddRange(builtCommands);

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
        while ((parentGroupType = parentGroupType!.DeclaringType!) is not null);

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
                CreateDelegate(method, method.GetParameters().Select(p => p.ParameterType).ToArray()),
                CommandShape.FromMethod(method),
                commandAttribute.Aliases,
                attributes,
                conditions
            );
        }
    }

    private static Func<IServiceProvider, object?[], CancellationToken, ValueTask<IResult>> CreateDelegate(MethodInfo method, Type[] argumentTypes)
    {
        // Get the object from the container
        var serviceProvider = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");

        var instance = Expression.Call(serviceProvider, GetServiceMethodInfo, Expression.Constant(method.DeclaringType));

        var parameters = Expression.Parameter(typeof(object?[]), "parameters");

        // Create the arguments
        var arguments = new Expression[argumentTypes.Length];
        for (var i = 0; i < argumentTypes.Length; i++)
        {
            var argumentType = argumentTypes[i];
            var argument = Expression.ArrayIndex(parameters, Expression.Constant(i));
            arguments[i] = Expression.Convert(argument, argumentType);
        }

        var castedInstance = Expression.Convert(instance, method.DeclaringType!);

        var call = Expression.Call(castedInstance, method, arguments);

        // Convert the result to a ValueTask<IResult>
        call = (MethodCallExpression)CoerceToValueTask(call);

        var ct = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var block = Expression.Block
        (
            Expression.Call(castedInstance, SetCancellationTokenMethodInfo, ct),
            call
        );

        // Compile the expression
        var lambda = Expression.Lambda<Func<IServiceProvider, object?[], CancellationToken, ValueTask<IResult>>>(block, serviceProvider, parameters, ct);

        return lambda.Compile();
    }

    /// <summary>
    /// Coerces the static result type of an expression to a <see cref="ValueTask"/>.
    /// <list type="bullet">
    /// <item>If the type is <see cref="ValueTask"/>, returns the expression as-is</item>
    /// <item>If the type is <see cref="Task"/>, returns an expression wrappiung the Task in a <see cref="ValueTask"/></item>
    /// <item>If the type is <c>void</c>, returns <see cref="ValueTask.CompletedTask"/></item>
    /// <item>Otherwise, throws <see cref="InvalidOperationException"/></item>
    /// </list>
    /// </summary>
    /// <param name="expression">The input expression.</param>
    /// <param name="expressionType">The type of the expression; defaults to <see cref="Expression.Type"/>.</param>
    /// <returns>The new expression.</returns>
    /// <exception cref="InvalidOperationException">If the type of <paramref name="expression"/> is not wrappable.</exception>
    public static Expression CoerceToValueTask(Expression expression, Type? expressionType = null)
    {
        expressionType ??= expression.Type;

        Expression invokerExpr;
        if (expressionType.IsConstructedGenericType && expressionType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            invokerExpr = Expression.Call(ToResultValueTaskInfo.MakeGenericMethod(expressionType.GetGenericArguments()[0]), expression);
        }
        else if (expressionType.IsConstructedGenericType && expressionType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            invokerExpr = Expression.Call(ToResultTaskInfo.MakeGenericMethod(expressionType.GetGenericArguments()[0]), expression);
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

    private static readonly MethodInfo ToResultValueTaskInfo
        = typeof(CommandTreeBuilder).GetMethod(nameof(ToResultValueTask), BindingFlags.Static | BindingFlags.NonPublic)
          ?? throw new InvalidOperationException($"Did not find {nameof(ToResultValueTask)}");

    private static readonly MethodInfo ToResultTaskInfo
        = typeof(CommandTreeBuilder).GetMethod(nameof(ToResultTask), BindingFlags.Static | BindingFlags.NonPublic)
          ?? throw new InvalidOperationException($"Did not find {nameof(ToResultTask)}");
}
