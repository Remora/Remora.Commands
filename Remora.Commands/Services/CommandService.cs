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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Remora.Commands.Conditions;
using Remora.Commands.Extensions;
using Remora.Commands.Groups;
using Remora.Commands.Parsers;
using Remora.Commands.Results;
using Remora.Commands.Signatures;
using Remora.Commands.Trees;
using Remora.Results;

namespace Remora.Commands.Services
{
    /// <summary>
    /// Handles search and dispatch of commands.
    /// </summary>
    [PublicAPI]
    public class CommandService
    {
        private readonly TypeRepository<ICondition> _conditionRepository;
        private readonly TypeRepository<ITypeParser> _parserRepository;

        /// <summary>
        /// Gets the tree of registered commands.
        /// </summary>
        public CommandTree Tree { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandService"/> class.
        /// </summary>
        /// <param name="tree">The command tree.</param>
        /// <param name="conditionRepository">The condition type repository.</param>
        /// <param name="parserRepository">The parser type repository.</param>
        internal CommandService
        (
            CommandTree tree,
            IOptions<TypeRepository<ICondition>> conditionRepository,
            IOptions<TypeRepository<ITypeParser>> parserRepository
        )
        {
            this.Tree = tree;
            _conditionRepository = conditionRepository.Value;
            _parserRepository = parserRepository.Value;
        }

        /// <summary>
        /// Attempts to find and execute a command that matches the given command string.
        /// </summary>
        /// <param name="commandString">The command string.</param>
        /// <param name="services">The services available to the invocation.</param>
        /// <param name="additionalParameters">
        /// Any additional parameters that should be available during instantiation of the command group.
        /// </param>
        /// <param name="searchOptions">A set of search options.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>An execution result which may or may not have succeeded.</returns>
        public async Task<Result<IResult>> TryExecuteAsync
        (
            string commandString,
            IServiceProvider services,
            object[]? additionalParameters = null,
            TreeSearchOptions? searchOptions = null,
            CancellationToken ct = default
        )
        {
            additionalParameters ??= Array.Empty<object>();

            var searchResults = this.Tree.Search(commandString, searchOptions).ToList();
            if (searchResults.Count == 0)
            {
                return new CommandNotFoundError(commandString);
            }

            return await TryExecuteAsync(searchResults, services, additionalParameters, ct);
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
        /// <param name="additionalParameters">
        /// Any additional parameters that should be available during instantiation of the command group.
        /// </param>
        /// <param name="searchOptions">A set of search options.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>An execution result which may or may not have succeeded.</returns>
        public async Task<Result<IResult>> TryExecuteAsync
        (
            string commandNameString,
            IReadOnlyDictionary<string, IReadOnlyList<string>> namedParameters,
            IServiceProvider services,
            object[]? additionalParameters = null,
            TreeSearchOptions? searchOptions = null,
            CancellationToken ct = default
        )
        {
            additionalParameters ??= Array.Empty<object>();

            var searchResults = this.Tree.Search(commandNameString, namedParameters, searchOptions).ToList();
            if (searchResults.Count == 0)
            {
                return new CommandNotFoundError(commandNameString);
            }

            return await TryExecuteAsync(searchResults, services, additionalParameters, ct);
        }

        private async Task<Result<IResult>> TryExecuteAsync
        (
            IReadOnlyList<BoundCommandNode> commandCandidates,
            IServiceProvider services,
            object[] additionalParameters,
            CancellationToken ct
        )
        {
            foreach (var commandCandidate in commandCandidates)
            {
                var executeResult = await TryExecuteAsync(commandCandidate, services, additionalParameters, ct);
                if (executeResult.IsSuccess)
                {
                    return executeResult;
                }
            }

            return new NoCompatibleCommandFoundError(commandCandidates);
        }

        private async Task<Result<IResult>> TryExecuteAsync
        (
            BoundCommandNode boundCommandNode,
            IServiceProvider services,
            object[] additionalParameters,
            CancellationToken ct = default
        )
        {
            // Check method-level conditions, if any
            var method = boundCommandNode.Node.CommandMethod;
            var methodConditionsResult = await CheckConditionsAsync(services, method, additionalParameters, ct);
            if (!methodConditionsResult.IsSuccess)
            {
                return Result<IResult>.FromError
                (
                    new GenericError("One or more method conditions failed."),
                    methodConditionsResult
                );
            }

            var materializeResult = await MaterializeParametersAsync
            (
                boundCommandNode,
                services,
                additionalParameters,
                ct
            );

            if (!materializeResult.IsSuccess)
            {
                return Result.FromError(new GenericError("Failed to materialize all parameters."), materializeResult);
            }

            var materializedParameters = materializeResult.Entity;

            // Check parameter-level conditions, if any
            var methodParameters = method.GetParameters();
            foreach (var (parameter, value) in methodParameters.Zip(materializedParameters, (info, o) => (info, o)))
            {
                var parameterConditionResult = await CheckConditionsAsync
                (
                    services,
                    parameter,
                    value,
                    additionalParameters,
                    ct
                );

                if (!parameterConditionResult.IsSuccess)
                {
                    return Result<IResult>.FromError
                    (
                        new GenericError("One or more parameter conditions failed."),
                        methodConditionsResult
                    );
                }
            }

            var groupType = boundCommandNode.Node.GroupType;
            var groupInstance = CreateInstance<CommandGroup>(services, groupType, additionalParameters);

            groupInstance.SetCancellationToken(ct);

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

                    var invocationResult = method.Invoke(groupInstance, materializedParameters);
                    var unwrapTask = (Task<IResult>)(unwrapMethod.Invoke
                    (
                        null, new[] { invocationResult }
                    ) ?? throw new InvalidOperationException());

                    result = await unwrapTask;
                }
                else
                {
                    var invocationResult = (Task)(method.Invoke(groupInstance, materializedParameters)
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
            finally
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (groupInstance is IDisposable d)
                {
                    d.Dispose();
                }

                // ReSharper disable once SuspiciousTypeConversion.Global
                if (groupInstance is IAsyncDisposable a)
                {
                    await a.DisposeAsync();
                }
            }
        }

        /// <summary>
        /// Checks the user-provided conditions of the given method. If a condition does not pass, the command will
        /// not execute.
        /// </summary>
        /// <param name="services">The available services.</param>
        /// <param name="method">The method.</param>
        /// <param name="additionalParameters">
        /// Any additional parameters that should be available during instantiation of the command group.
        /// </param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A condition result which may or may not have succeeded.</returns>
        private async Task<Result> CheckConditionsAsync
        (
            IServiceProvider services,
            MethodInfo method,
            object[] additionalParameters,
            CancellationToken ct
        )
        {
            var conditionAttributes = method.GetCustomAttributes(typeof(ConditionAttribute), false);
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

                var conditionTypes = _conditionRepository.GetTypes(conditionType).ToList();
                if (conditionTypes.Count == 0)
                {
                    throw new InvalidOperationException
                    (
                        "Condition attributes were applied, but no matching condition was registered."
                    );
                }

                var conditions = conditionTypes.Select
                (
                    c => CreateInstance<ICondition>(services, c, additionalParameters)
                );

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
                        return result;
                    }
                }
            }

            return Result.FromSuccess();
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

        /// <summary>
        /// Checks the user-provided conditions of the given parameter. If a condition does not pass, the command will
        /// not execute.
        /// </summary>
        /// <param name="services">The available services.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="value">The materialized value of the parameter.</param>
        /// <param name="additionalParameters">
        /// Any additional parameters that should be available during instantiation of the command group.
        /// </param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A condition result which may or may not have succeeded.</returns>
        private async Task<Result> CheckConditionsAsync
        (
            IServiceProvider services,
            ParameterInfo parameter,
            object? value,
            object[] additionalParameters,
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

                var conditionTypes = _conditionRepository.GetTypes(conditionType).ToList();
                if (conditionTypes.Count == 0)
                {
                    throw new InvalidOperationException
                    (
                        "Condition attributes were applied, but no matching condition was registered."
                    );
                }

                var conditions = conditionTypes.Select
                (
                    c => CreateInstance<ICondition>(services, c, additionalParameters)
                );

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
                        return result;
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
        /// <param name="additionalParameters">
        /// Any additional parameters that should be available during instantiation of the command group.
        /// </param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A sorted array of materialized parameters.</returns>
        private async Task<Result<object?[]>> MaterializeParametersAsync
        (
            BoundCommandNode boundCommandNode,
            IServiceProvider services,
            object[] additionalParameters,
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

                var typeToParse = parameter.ParameterType;
                var isCollection = typeToParse.IsSupportedEnumerable();
                if (isCollection)
                {
                    typeToParse = typeToParse.GetCollectionElementType();
                }

                var parserType = typeof(ITypeParser<>).MakeGenericType(typeToParse);
                var registeredParser = _parserRepository.GetTypes(parserType).LastOrDefault();

                if (registeredParser is null)
                {
                    if (typeToParse.IsEnum)
                    {
                        // We'll use our fallback parser
                        registeredParser = typeof(EnumParser<>).MakeGenericType(typeToParse);
                    }
                    else
                    {
                        return new MissingParserError(typeToParse);
                    }
                }

                var parser = CreateInstance<ITypeParser>(services, registeredParser, additionalParameters);
                if (isCollection)
                {
                    var collectionType = typeof(List<>).MakeGenericType(typeToParse);
                    IList collection = (IList)Activator.CreateInstance(collectionType)!;
                    foreach (var value in boundParameter.Tokens)
                    {
                        var parseResult = await parser.TryParseAsync(value, ct);
                        if (!parseResult.IsSuccess)
                        {
                            return Result<object?[]>.FromError(new ParameterParsingError(boundParameter), parseResult);
                        }

                        collection.Add(parseResult.Entity);
                    }

                    materializedParameters.Add(collection);
                }
                else
                {
                    // Special case: switches
                    if (boundParameter.ParameterShape is SwitchParameterShape)
                    {
                        // No need for parsing; if a switch is present it means it's the inverse of the parameter's
                        // default value
                        var defaultValue = (bool)(parameter.DefaultValue ?? throw new InvalidOperationException());
                        materializedParameters.Add(!defaultValue);
                        continue;
                    }

                    // Special case: greedy parameters
                    string value;
                    if (boundParameter.ParameterShape is NamedGreedyParameterShape or PositionalGreedyParameterShape)
                    {
                        // Merge the bound tokens
                        value = string.Join(' ', boundParameter.Tokens);
                    }
                    else
                    {
                        if (boundParameter.Tokens.Count > 1)
                        {
                            throw new InvalidOperationException
                            (
                                "More than one token was provided for a single-value parameter."
                            );
                        }

                        value = boundParameter.Tokens[0];
                    }

                    var parseResult = await parser.TryParseAsync(value, ct);
                    if (!parseResult.IsSuccess)
                    {
                        return Result<object?[]>.FromError(new ParameterParsingError(boundParameter), parseResult);
                    }

                    materializedParameters.Add(parseResult.Entity);
                }
            }

            return materializedParameters.ToArray();
        }

        private TInstance CreateInstance<TInstance>
        (
            IServiceProvider services,
            Type typeToCreate,
            object[] additionalParameters
        )
        {
            try
            {
                return (TInstance)ActivatorUtilities.CreateInstance
                (
                    services,
                    typeToCreate,
                    additionalParameters
                );
            }
            catch (InvalidOperationException)
            {
                // Try without offering additional parameters; this method has a bad habit of throwing if it can't
                // match all provided extra parameters.
                return (TInstance)ActivatorUtilities.CreateInstance
                (
                    services,
                    typeToCreate
                );
            }
        }
    }
}
