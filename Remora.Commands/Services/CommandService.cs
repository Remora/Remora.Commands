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
        private readonly CommandTree _tree;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandService"/> class.
        /// </summary>
        /// <param name="tree">The command tree.</param>
        internal CommandService(CommandTree tree)
        {
            _tree = tree;
        }

        /// <summary>
        /// Attempts to find and execute a command that matches the given command string.
        /// </summary>
        /// <param name="commandString">The command string.</param>
        /// <param name="services">The services available to the invocation.</param>
        /// <param name="additionalParameters">
        /// Any additional parameters that should be available during instantiation of the command group.
        /// </param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>An execution result which may or may not have succeeded.</returns>
        public async Task<CommandExecutionResult> TryExecuteAsync
        (
            string commandString,
            IServiceProvider services,
            object[]? additionalParameters = null,
            CancellationToken ct = default
        )
        {
            additionalParameters ??= Array.Empty<object>();

            var searchResults = _tree.Search(commandString).ToList();
            if (searchResults.Count == 0)
            {
                return CommandExecutionResult.NotFound();
            }

            foreach (var boundCommandShape in searchResults)
            {
                var executeResult = await TryExecuteAsync(boundCommandShape, services, additionalParameters, ct);
                if (executeResult.IsSuccess)
                {
                    return executeResult;
                }

                switch (executeResult.Status)
                {
                    case ExecutionStatus.Failed:
                    case ExecutionStatus.Faulted:
                    {
                        return executeResult;
                    }
                    case ExecutionStatus.Ambiguous:
                    case ExecutionStatus.NotFound:
                    {
                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return CommandExecutionResult.NotFound();
        }

        private async Task<CommandExecutionResult> TryExecuteAsync
        (
            BoundCommandNode boundCommandNode,
            IServiceProvider services,
            object[]? additionalParameters,
            CancellationToken ct = default
        )
        {
            // Check method-level conditions, if any
            var method = boundCommandNode.Node.CommandMethod;
            var methodConditionsResult = await CheckConditionsAsync(services, method, ct);
            if (!methodConditionsResult.IsSuccess)
            {
                return CommandExecutionResult.Failed(methodConditionsResult);
            }

            var materializeResult = await MaterializeParametersAsync(boundCommandNode, services, ct);
            if (!materializeResult.IsSuccess)
            {
                return CommandExecutionResult.FromError(materializeResult);
            }

            var materializedParameters = materializeResult.Entity;

            // Check parameter-level conditions, if any
            var methodParameters = method.GetParameters();
            foreach (var (parameter, value) in methodParameters.Zip(materializedParameters, (info, o) => (info, o)))
            {
                var parameterConditionResult = await CheckConditionsAsync(services, parameter, value, ct);
                if (!parameterConditionResult.IsSuccess)
                {
                    return CommandExecutionResult.Failed(parameterConditionResult);
                }
            }

            var groupType = boundCommandNode.Node.GroupType;
            var groupInstance = (CommandGroup)ActivatorUtilities.CreateInstance
            (
                services,
                groupType,
                additionalParameters
            );

            groupInstance.SetCancellationToken(ct);

            try
            {
                var returnValue = (Task<IResult>)method.Invoke(groupInstance, materializedParameters);
                var result = await returnValue;
                if (!result.IsSuccess)
                {
                    return CommandExecutionResult.Failed(result);
                }

                return CommandExecutionResult.FromSuccess(result);
            }
            catch (Exception ex)
            {
                return CommandExecutionResult.Faulted(ex);
            }
            finally
            {
                if (groupInstance is IDisposable d)
                {
                    d.Dispose();
                }

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
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A condition result which may or may not have succeeded.</returns>
        private async Task<DetermineConditionResult> CheckConditionsAsync
        (
            IServiceProvider services,
            MethodInfo method,
            CancellationToken ct
        )
        {
            var conditionAttributes = method.GetCustomAttributes(typeof(ConditionAttribute), false);
            if (!conditionAttributes.Any())
            {
                return DetermineConditionResult.FromSuccess();
            }

            foreach (var conditionAttribute in conditionAttributes)
            {
                var conditionType = typeof(ICondition<>).MakeGenericType(conditionAttribute.GetType());
                var conditionMethod = conditionType.GetMethod(nameof(ICondition<ConditionAttribute>.CheckAsync));
                if (conditionMethod is null)
                {
                    throw new InvalidOperationException();
                }

                var conditions = services.GetServices(conditionType);
                foreach (var condition in conditions)
                {
                    var result = await (ValueTask<DetermineConditionResult>)conditionMethod.Invoke
                    (
                        condition,
                        new[] { conditionAttribute, ct }
                    );

                    if (!result.IsSuccess)
                    {
                        return result;
                    }
                }
            }

            return DetermineConditionResult.FromSuccess();
        }

        /// <summary>
        /// Checks the user-provided conditions of the given parameter. If a condition does not pass, the command will
        /// not execute.
        /// </summary>
        /// <param name="services">The available services.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="value">The materialized value of the parameter.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A condition result which may or may not have succeeded.</returns>
        private async Task<DetermineConditionResult> CheckConditionsAsync
        (
            IServiceProvider services,
            ParameterInfo parameter,
            object value,
            CancellationToken ct
        )
        {
            var conditionAttributes = parameter.GetCustomAttributes(typeof(ConditionAttribute), false);
            if (!conditionAttributes.Any())
            {
                return DetermineConditionResult.FromSuccess();
            }

            foreach (var conditionAttribute in conditionAttributes)
            {
                var conditionType = typeof(ICondition<,>).MakeGenericType
                (
                    conditionAttribute.GetType(),
                    value.GetType()
                );

                var conditionMethod = conditionType.GetMethod(nameof(ICondition<ConditionAttribute>.CheckAsync));
                if (conditionMethod is null)
                {
                    throw new InvalidOperationException();
                }

                var conditions = services.GetServices(conditionType);
                foreach (var condition in conditions)
                {
                    var result = await (ValueTask<DetermineConditionResult>)conditionMethod.Invoke
                    (
                        condition,
                        new[] { conditionAttribute, value, ct }
                    );

                    if (!result.IsSuccess)
                    {
                        return result;
                    }
                }
            }

            return DetermineConditionResult.FromSuccess();
        }

        /// <summary>
        /// Converts a set of parameter-bound strings into their corresponding CLR types.
        /// </summary>
        /// <param name="boundCommandNode">The bound command node.</param>
        /// <param name="services">The available services.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A sorted array of materialized parameters.</returns>
        private async Task<RetrieveEntityResult<object[]>> MaterializeParametersAsync
        (
            BoundCommandNode boundCommandNode,
            IServiceProvider services,
            CancellationToken ct
        )
        {
            var materializedParameters = new List<object>();

            var boundParameters = boundCommandNode.BoundParameters.ToDictionary(bp => bp.ParameterShape.Parameter);
            foreach (var parameter in boundCommandNode.Node.CommandMethod.GetParameters())
            {
                if (!boundParameters.TryGetValue(parameter, out var boundParameter))
                {
                    if (!parameter.IsOptional)
                    {
                        return RetrieveEntityResult<object[]>.FromError
                        (
                            "An unbound required parameter was encountered."
                        );
                    }

                    materializedParameters.Add(parameter.DefaultValue);
                    continue;
                }

                var typeToParse = parameter.ParameterType;
                var isCollection = typeToParse.IsSupportedEnumerable();
                if (isCollection)
                {
                    typeToParse = typeToParse.GetCollectionElementType();
                }

                var parserType = typeof(ITypeParser<>).MakeGenericType(typeToParse);
                if (!(services.GetService(parserType) is ITypeParser parser))
                {
                    // We can't parse this type
                    return RetrieveEntityResult<object[]>.FromError
                    (
                        $"No parser has been registered for \"{parserType.Name}\"."
                    );
                }

                if (isCollection)
                {
                    var collectionType = typeof(List<>).MakeGenericType(typeToParse);
                    IList collection = (IList)Activator.CreateInstance(collectionType);
                    foreach (var value in boundParameter.Tokens)
                    {
                        var parseResult = await parser.TryParseAsync(value, ct);
                        if (!parseResult.IsSuccess)
                        {
                            // Parsing failed
                            return RetrieveEntityResult<object[]>.FromError
                            (
                                $"Failed to parse an instance of \"{typeToParse.Name}\" from the value \"{value}\""
                            );
                        }

                        collection.Add(parseResult.Entity);
                    }

                    materializedParameters.Add(collection);
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

                    // Special case: switches
                    if (boundParameter.ParameterShape is SwitchParameterShape)
                    {
                        // No need for parsing; if a switch is present it means it's the inverse of the parameter's
                        // default value
                        var defaultValue = (bool)parameter.DefaultValue;
                        materializedParameters.Add(!defaultValue);
                        continue;
                    }

                    var value = boundParameter.Tokens[0];
                    var parseResult = await parser.TryParseAsync(value, ct);
                    if (!parseResult.IsSuccess)
                    {
                        return RetrieveEntityResult<object[]>.FromError
                        (
                            $"Failed to parse an instance of \"{typeToParse.Name}\" from the value \"{value}\""
                        );
                    }

                    materializedParameters.Add(parseResult.Entity);
                }
            }

            return materializedParameters.ToArray();
        }
    }
}
