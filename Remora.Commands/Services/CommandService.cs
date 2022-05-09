//
//  CommandService.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
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
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Conditions;
using Remora.Commands.Extensions;
using Remora.Commands.Groups;
using Remora.Commands.Results;
using Remora.Commands.Signatures;
using Remora.Commands.Tokenization;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Remora.Commands.Services;

/// <summary>
/// Handles search and dispatch of commands.
/// </summary>
[PublicAPI]
public class CommandService
{
    private readonly TypeParserService _typeParserService;

    /// <summary>
    /// Gets the service through which trees of registered commands can be accessed.
    /// </summary>
    public CommandTreeAccessor TreeAccessor { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandService"/> class.
    /// </summary>
    /// <param name="treeAccessor">The command tree accessor.</param>
    /// <param name="typeParserService">The parser service.</param>
    public CommandService
    (
        CommandTreeAccessor treeAccessor,
        TypeParserService typeParserService
    )
    {
        this.TreeAccessor = treeAccessor;
        _typeParserService = typeParserService;
    }

    /// <summary>
    /// Attempts to find and execute a command that matches the given command string.
    /// </summary>
    /// <param name="commandString">The command string.</param>
    /// <param name="services">The services available to the invocation.</param>
    /// <param name="tokenizerOptions">The tokenizer options.</param>
    /// <param name="searchOptions">A set of search options.</param>
    /// <param name="treeName">The name of the tree to search for the command in.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns>An execution result which may or may not have succeeded.</returns>
    public async Task<Result<IResult>> TryExecuteAsync
    (
        string commandString,
        IServiceProvider services,
        TokenizerOptions? tokenizerOptions = null,
        TreeSearchOptions? searchOptions = null,
        string? treeName = null,
        CancellationToken ct = default
    )
    {
        var prepareCommand = await TryPrepareCommandAsync
        (
            commandString,
            services,
            tokenizerOptions,
            searchOptions,
            treeName,
            ct
        );

        if (!prepareCommand.IsSuccess)
        {
            return Result<IResult>.FromError(prepareCommand);
        }

        var preparedCommand = prepareCommand.Entity;
        return await TryExecuteAsync(preparedCommand, services, ct);
    }

    /// <summary>
    /// Attempts to find and execute a command that matches the given command string and associated named
    /// parameters.
    /// </summary>
    /// <param name="commandNameString">
    /// The command name string. This string should not contain any parameters or their values.
    /// </param>
    /// <param name="namedParameters">The named parameters.</param>
    /// <param name="services">The services available to the invocation.</param>
    /// <param name="tokenizerOptions">The tokenizer options.</param>
    /// <param name="searchOptions">A set of search options.</param>
    /// <param name="treeName">The name of the tree to search for the command in.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns>An execution result which may or may not have succeeded.</returns>
    public async Task<Result<IResult>> TryExecuteAsync
    (
        string commandNameString,
        IReadOnlyDictionary<string, IReadOnlyList<string>> namedParameters,
        IServiceProvider services,
        TokenizerOptions? tokenizerOptions = null,
        TreeSearchOptions? searchOptions = null,
        string? treeName = null,
        CancellationToken ct = default
    )
    {
        var prepareCommand = await TryPrepareCommandAsync
        (
            commandNameString,
            namedParameters,
            services,
            tokenizerOptions,
            searchOptions,
            treeName,
            ct
        );

        if (!prepareCommand.IsSuccess)
        {
            return Result<IResult>.FromError(prepareCommand);
        }

        // At this point, a single candidate remains, so we execute it
        var preparedCommand = prepareCommand.Entity;
        return await TryExecuteAsync(preparedCommand, services, ct);
    }

    /// <summary>
    /// Attempts to find and execute a command that matches the given command string and associated named
    /// parameters.
    /// </summary>
    /// <param name="commandPath">
    /// The command path, that is, the sequential components of the full command name.
    /// </param>
    /// <param name="namedParameters">The named parameters.</param>
    /// <param name="services">The services available to the invocation.</param>
    /// <param name="tokenizerOptions">The tokenizer options.</param>
    /// <param name="searchOptions">A set of search options.</param>
    /// <param name="treeName">The name of the tree to search for the command in.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns>An execution result which may or may not have succeeded.</returns>
    public async Task<Result<IResult>> TryExecuteAsync
    (
        IReadOnlyList<string> commandPath,
        IReadOnlyDictionary<string, IReadOnlyList<string>> namedParameters,
        IServiceProvider services,
        TokenizerOptions? tokenizerOptions = null,
        TreeSearchOptions? searchOptions = null,
        string? treeName = null,
        CancellationToken ct = default
    )
    {
        var prepareCommand = await TryPrepareCommandAsync
        (
            commandPath,
            namedParameters,
            services,
            tokenizerOptions,
            searchOptions,
            treeName,
            ct
        );

        if (!prepareCommand.IsSuccess)
        {
            return Result<IResult>.FromError(prepareCommand);
        }

        // At this point, a single candidate remains, so we execute it
        var preparedCommand = prepareCommand.Entity;
        return await TryExecuteAsync(preparedCommand, services, ct);
    }

    /// <summary>
    /// Attempts to find and prepare a command for execution, but does not actually execute it.
    /// </summary>
    /// <param name="commandString">The command string.</param>
    /// <param name="services">The services available to the invocation.</param>
    /// <param name="tokenizerOptions">The tokenizer options.</param>
    /// <param name="searchOptions">A set of search options.</param>
    /// <param name="treeName">The name of the tree to search for the command in.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns>
    /// A result which may or may not have succeeded, containing the node and its materialized parameters.
    /// </returns>
    public async Task<Result<PreparedCommand>> TryPrepareCommandAsync
    (
        string commandString,
        IServiceProvider services,
        TokenizerOptions? tokenizerOptions = null,
        TreeSearchOptions? searchOptions = null,
        string? treeName = null,
        CancellationToken ct = default
    )
    {
        if (!this.TreeAccessor.TryGetNamedTree(treeName, out var tree))
        {
            return new TreeNotFoundError(treeName);
        }

        var searchResults = tree.Search(commandString, tokenizerOptions, searchOptions).ToList();
        if (searchResults.Count == 0)
        {
            return new CommandNotFoundError(commandString);
        }

        return await TryPrepareCommandAsync(searchResults, services, ct);
    }

    /// <summary>
    /// Attempts to find and prepare a command for execution, but does not actually execute it.
    /// </summary>
    /// <param name="commandNameString">
    /// The command name string. This string should not contain any parameters or their values.
    /// </param>
    /// <param name="namedParameters">The named parameters.</param>
    /// <param name="services">The services available to the invocation.</param>
    /// <param name="tokenizerOptions">The tokenizer options.</param>
    /// <param name="searchOptions">A set of search options.</param>
    /// <param name="treeName">The name of the tree to search for the command in.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns>
    /// A result which may or may not have succeeded, containing the node and its materialized parameters.
    /// </returns>
    public Task<Result<PreparedCommand>> TryPrepareCommandAsync
    (
        string commandNameString,
        IReadOnlyDictionary<string, IReadOnlyList<string>> namedParameters,
        IServiceProvider services,
        TokenizerOptions? tokenizerOptions = null,
        TreeSearchOptions? searchOptions = null,
        string? treeName = null,
        CancellationToken ct = default
    )
    {
        return TryPrepareCommandAsync
        (
            commandNameString.Split(' ', StringSplitOptions.RemoveEmptyEntries),
            namedParameters,
            services,
            tokenizerOptions,
            searchOptions,
            treeName,
            ct
        );
    }

    /// <summary>
    /// Attempts to find and prepare a command for execution, but does not actually execute it.
    /// </summary>
    /// <param name="commandPath">
    /// The command path, that is, the sequential components of the full command name.
    /// </param>
    /// <param name="namedParameters">The named parameters.</param>
    /// <param name="services">The services available to the invocation.</param>
    /// <param name="tokenizerOptions">The tokenizer options.</param>
    /// <param name="searchOptions">A set of search options.</param>
    /// <param name="treeName">The name of the tree to search for the command in.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns>
    /// A result which may or may not have succeeded, containing the node and its materialized parameters.
    /// </returns>
    public async Task<Result<PreparedCommand>> TryPrepareCommandAsync
    (
        IReadOnlyList<string> commandPath,
        IReadOnlyDictionary<string, IReadOnlyList<string>> namedParameters,
        IServiceProvider services,
        TokenizerOptions? tokenizerOptions = null,
        TreeSearchOptions? searchOptions = null,
        string? treeName = null,
        CancellationToken ct = default
    )
    {
        if (!this.TreeAccessor.TryGetNamedTree(treeName, out var tree))
        {
            return new TreeNotFoundError(treeName);
        }

        var searchResults = tree.Search
        (
            commandPath,
            namedParameters,
            searchOptions
        ).ToList();

        if (searchResults.Count == 0)
        {
            return new CommandNotFoundError(string.Join(' ', commandPath));
        }

        return await TryPrepareCommandAsync(searchResults, services, ct);
    }

    /// <summary>
    /// Attempts to execute the given prepared command.
    /// </summary>
    /// <param name="preparedCommand">The prepared command.</param>
    /// <param name="services">The services available to the invocation.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns>An execution result which may or may not have succeeded.</returns>
    public async Task<Result<IResult>> TryExecuteAsync
    (
        PreparedCommand preparedCommand,
        IServiceProvider services,
        CancellationToken ct = default
    )
    {
        var (boundCommandNode, parameters) = preparedCommand;

        var groupType = boundCommandNode.Node.GroupType;
        var groupInstance = (CommandGroup)services.GetRequiredService(groupType);

        groupInstance.SetCancellationToken(ct);

        var method = boundCommandNode.Node.CommandMethod;

        try
        {
            IResult result;
            if (method.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                var genericUnwrapMethod = GetType()
                                              .GetMethod(nameof(UnwrapCommandValueTask), BindingFlags.NonPublic | BindingFlags.Static)
                                          ?? throw new InvalidOperationException();

                var unwrapMethod = genericUnwrapMethod
                    .MakeGenericMethod(method.ReturnType.GetGenericArguments().Single());

                var invocationResult = method.Invoke(groupInstance, parameters);
                var unwrapTask = (Task<IResult>)(unwrapMethod.Invoke
                (
                    null, new[] { invocationResult }
                ) ?? throw new InvalidOperationException());

                result = await unwrapTask;
            }
            else
            {
                var invocationResult = (Task)(method.Invoke(groupInstance, parameters)
                                              ?? throw new InvalidOperationException());
                await invocationResult;

                result = (IResult)(invocationResult.GetType().GetProperty(nameof(Task<object>.Result))
                    ?.GetValue(invocationResult) ?? throw new InvalidOperationException());
            }

            return Result<IResult>.FromSuccess(result);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    private async Task<Result<PreparedCommand>> TryPrepareCommandAsync
    (
        IReadOnlyList<BoundCommandNode> commandCandidates,
        IServiceProvider services,
        CancellationToken ct
    )
    {
        // First, walk through the candidates and do the following:
        // 1) Check group conditions
        // 2) Check method conditions
        // 3) Materialize parameters, parsing strings to concrete instances
        // 4) Check parameter conditions
        var preparedCommands = await Task.WhenAll
        (
            commandCandidates.Select(c => TryPrepareCommandAsync(c, services, ct))
        );

        var successfullyPreparedCommands = preparedCommands.Where(r => r.IsSuccess).Select(r => r.Entity).ToList();

        // Then, check if we have to bail out at this point
        if (successfullyPreparedCommands.Count > 1)
        {
            // Pick the most specific command, if one exists
            var mostSpecificCommands = successfullyPreparedCommands
                    .GroupBy(c => c.Command.Node.CalculateDepth())
                    .OrderBy(g => g.Key)
                    .First();

            if (mostSpecificCommands.Count() == 1)
            {
                return mostSpecificCommands.Single();
            }

            var ambiguousCommands = preparedCommands
                    .Where(r => r.IsSuccess)
                    .Select(r => r.Entity)
                    .ToArray();

            return new AmbiguousCommandInvocationError(ambiguousCommands);
        }

        if (preparedCommands.Any(r => r.IsSuccess))
        {
            return preparedCommands.Single(r => r.IsSuccess).Entity;
        }

        var errors = preparedCommands.Where(r => !r.IsSuccess).ToList();
        return errors.Count switch
        {
            1 => Result<PreparedCommand>.FromError(errors[0]),
            _ => new AggregateError(errors.Cast<IResult>().ToArray())
        };
    }

    private async Task<Result<PreparedCommand>> TryPrepareCommandAsync
    (
        BoundCommandNode boundCommandNode,
        IServiceProvider services,
        CancellationToken ct = default
    )
    {
        // Check group-level conditions, if any
        var groupTypeWithConditions = boundCommandNode.Node.GroupType;
        var groupNode = boundCommandNode.Node.Parent as GroupNode;

        while (true)
        {
            var groupConditionsResult = await CheckConditionsAsync
            (
                services,
                groupNode,
                groupTypeWithConditions,
                ct
            );

            if (!groupConditionsResult.IsSuccess)
            {
                return Result<PreparedCommand>.FromError(groupConditionsResult);
            }

            if (!typeof(CommandGroup).IsAssignableFrom(groupTypeWithConditions.DeclaringType))
            {
                break;
            }

            groupTypeWithConditions = groupTypeWithConditions.DeclaringType;
            if (groupNode is not null && !groupNode.GroupTypes.Contains(groupTypeWithConditions))
            {
                groupNode = groupNode.Parent as GroupNode;
            }
        }

        // Check method-level conditions, if any
        var method = boundCommandNode.Node.CommandMethod;
        var methodConditionsResult = await CheckConditionsAsync
        (
            services,
            boundCommandNode.Node,
            method,
            ct
        );

        if (!methodConditionsResult.IsSuccess)
        {
            return Result<PreparedCommand>.FromError(methodConditionsResult);
        }

        var materializeResult = await MaterializeParametersAsync
        (
            boundCommandNode,
            services,
            ct
        );

        if (!materializeResult.IsSuccess)
        {
            return Result<PreparedCommand>.FromError(materializeResult);
        }

        var materializedParameters = materializeResult.Entity;

        // Check parameter-level conditions, if any
        var methodParameters = method.GetParameters();
        foreach (var (parameter, value) in methodParameters.Zip(materializedParameters, (info, o) => (info, o)))
        {
            var parameterConditionResult = await CheckConditionsAsync
            (
                services,
                boundCommandNode.Node,
                parameter,
                value,
                ct
            );

            if (!parameterConditionResult.IsSuccess)
            {
                return Result<PreparedCommand>.FromError(parameterConditionResult);
            }
        }

        return new PreparedCommand(boundCommandNode, materializedParameters);
    }

    /// <summary>
    /// Checks the user-provided conditions applied to the given attribute provider. If a condition does not pass,
    /// the command will not execute.
    /// </summary>
    /// <param name="services">The available services.</param>
    /// <param name="node">The node whose conditions are being checked.</param>
    /// <param name="attributeProvider">The group.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns>A condition result which may or may not have succeeded.</returns>
    private async Task<Result> CheckConditionsAsync
    (
        IServiceProvider services,
        IChildNode? node,
        ICustomAttributeProvider attributeProvider,
        CancellationToken ct
    )
    {
        var conditionAttributes = attributeProvider.GetCustomAttributes(typeof(ConditionAttribute), false);
        if (!conditionAttributes.Any())
        {
            return Result.FromSuccess();
        }

        foreach (var conditionAttribute in conditionAttributes)
        {
            var conditionType = typeof(ICondition<>).MakeGenericType(conditionAttribute.GetType());

            var conditionMethod = conditionType.GetMethod(nameof(ICondition<ConditionAttribute>.CheckAsync));
            if (conditionMethod is null)
            {
                throw new InvalidOperationException();
            }

            var conditions = services
                .GetServices(conditionType)
                .Where(c => c is not null)
                .Cast<ICondition>()
                .ToList();

            if (conditions.Count == 0)
            {
                throw new InvalidOperationException
                (
                    "Condition attributes were applied, but no matching condition was registered."
                );
            }

            foreach (var condition in conditions)
            {
                var invocationResult = conditionMethod.Invoke
                                       (
                                           condition,
                                           new[] { conditionAttribute, ct }
                                       )
                                       ?? throw new InvalidOperationException();

                var result = await (ValueTask<Result>)invocationResult;

                if (!result.IsSuccess)
                {
                    return Result.FromError
                    (
                        new ConditionNotSatisfiedError
                        (
                            $"The condition \"{condition.GetType().Name}\" was not satisfied.",
                            node
                        ),
                        result
                    );
                }
            }
        }

        return Result.FromSuccess();
    }

    /// <summary>
    /// Checks the user-provided conditions of the given parameter. If a condition does not pass, the command will
    /// not execute.
    /// </summary>
    /// <param name="services">The available services.</param>
    /// <param name="node">The node whose conditions are being checked.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="value">The materialized value of the parameter.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns>A condition result which may or may not have succeeded.</returns>
    private async Task<Result> CheckConditionsAsync
    (
        IServiceProvider services,
        IChildNode node,
        ParameterInfo parameter,
        object? value,
        CancellationToken ct
    )
    {
        var conditionAttributes = parameter.GetCustomAttributes(typeof(ConditionAttribute), false);
        if (!conditionAttributes.Any())
        {
            return Result.FromSuccess();
        }

        foreach (var conditionAttribute in conditionAttributes)
        {
            var conditionType = typeof(ICondition<,>).MakeGenericType
            (
                conditionAttribute.GetType(),
                parameter.ParameterType
            );

            var conditionMethod = conditionType.GetMethod(nameof(ICondition<ConditionAttribute>.CheckAsync));
            if (conditionMethod is null)
            {
                throw new InvalidOperationException();
            }

            var conditions = services
                .GetServices(conditionType)
                .Where(c => c is not null)
                .Cast<ICondition>()
                .ToList();

            if (conditions.Count == 0)
            {
                throw new InvalidOperationException
                (
                    "Condition attributes were applied, but no matching condition was registered."
                );
            }

            foreach (var condition in conditions)
            {
                var invocationResult = conditionMethod.Invoke
                                       (
                                           condition,
                                           new[] { conditionAttribute, value, ct }
                                       )
                                       ?? throw new InvalidOperationException();

                var result = await (ValueTask<Result>)invocationResult;

                if (!result.IsSuccess)
                {
                    return Result.FromError
                    (
                        new ConditionNotSatisfiedError
                        (
                            $"The condition \"{condition.GetType().Name}\" was not satisfied.",
                            node
                        ),
                        result
                    );
                }
            }
        }

        return Result.FromSuccess();
    }

    /// <summary>
    /// Converts a set of parameter-bound strings into their corresponding CLR types.
    /// </summary>
    /// <param name="boundCommandNode">The bound command node.</param>
    /// <param name="services">The available services.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns>A sorted array of materialized parameters.</returns>
    private async Task<Result<object?[]>> MaterializeParametersAsync
    (
        BoundCommandNode boundCommandNode,
        IServiceProvider services,
        CancellationToken ct
    )
    {
        var materializedParameters = new List<object?>();

        var boundParameters = boundCommandNode.BoundParameters.ToDictionary(bp => bp.ParameterShape.Parameter);
        var parameterShapes = boundCommandNode.Node.Shape.Parameters.ToDictionary(p => p.Parameter);

        foreach (var parameter in boundCommandNode.Node.CommandMethod.GetParameters())
        {
            if (!boundParameters.TryGetValue(parameter, out var boundParameter))
            {
                var shape = parameterShapes[parameter];
                if (!shape.IsOmissible())
                {
                    return new RequiredParameterValueMissingError(shape);
                }

                materializedParameters.Add(shape.DefaultValue);
                continue;
            }

            // Special case: nullability
            if (boundParameter.ParameterShape.Parameter.AllowsNull())
            {
                // Support null literals
                if (boundParameter.Tokens.Count == 1 && boundParameter.Tokens[0].Trim() is "null")
                {
                    materializedParameters.Add(null);
                    continue;
                }
            }

            // Special case: switches
            if (boundParameter.ParameterShape is SwitchParameterShape)
            {
                // No need for parsing; if a switch is present it means it's the inverse of the parameter's
                // default value
                var defaultValue = (bool)(parameter.DefaultValue ?? throw new InvalidOperationException());
                materializedParameters.Add(!defaultValue);
                continue;
            }

            var isCollection = boundParameter.ParameterShape is
                NamedCollectionParameterShape or PositionalCollectionParameterShape;

            var isGreedy = boundParameter.ParameterShape is
                NamedGreedyParameterShape or PositionalGreedyParameterShape;

            Result<object?> tryParse;
            if (!isCollection || isGreedy)
            {
                // Special case: greedy parameters
                var value = boundParameter.Tokens[0];
                if (isGreedy)
                {
                    // Merge the bound tokens
                    value = string.Join(' ', boundParameter.Tokens);
                }

                tryParse = await _typeParserService.TryParseAsync
                (
                    services,
                    value,
                    parameter.ParameterType,
                    ct
                );

                if (!tryParse.IsSuccess)
                {
                    return Result<object?[]>.FromError(new ParameterParsingError(boundParameter), tryParse);
                }
            }
            else
            {
                tryParse = await _typeParserService.TryParseAsync
                (
                    services,
                    boundParameter.Tokens,
                    parameter.ParameterType,
                    ct
                );

                if (!tryParse.IsSuccess)
                {
                    return Result<object?[]>.FromError(new ParameterParsingError(boundParameter), tryParse);
                }
            }

            materializedParameters.Add(tryParse.Entity);
        }

        return materializedParameters.ToArray();
    }

    /// <summary>
    /// Helper method for unwrapping instances of <see cref="ValueTask{TResult}"/>.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private static async Task<IResult> UnwrapCommandValueTask<TEntity>(ValueTask<TEntity> task)
        where TEntity : IResult
    {
        return await task;
    }

    private TInstance CreateInstance<TInstance>
    (
        IServiceProvider services,
        Type typeToCreate
    )
    {
        return (TInstance)ActivatorUtilities.CreateInstance
        (
            services,
            typeToCreate
        );
    }
}
