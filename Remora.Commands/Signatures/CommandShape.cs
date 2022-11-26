//
//  CommandShape.cs
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
using JetBrains.Annotations;
using Remora.Commands.Attributes;
using Remora.Commands.Builders;
using Remora.Commands.Extensions;

namespace Remora.Commands.Signatures;

/// <summary>
/// Represents the general "shape" of a command. This type is used to determine whether a sequence of tokens could
/// fit the associated command, provided all other things hold true.
/// </summary>
[PublicAPI]
public class CommandShape
{
    /// <summary>
    /// Gets the parameters for this command shape.
    /// </summary>
    public IReadOnlyList<IParameterShape> Parameters { get; }

    /// <summary>
    /// Gets a user-configured description of the command.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandShape"/> class.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <param name="description">The description of the command.</param>
    public CommandShape(IReadOnlyList<IParameterShape> parameters, string? description = null)
    {
        this.Parameters = parameters;
        this.Description = description ?? Constants.DefaultDescription;
    }

    /// <summary>
    /// Creates a new <see cref="CommandShape"/> from the given builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The command shape.</returns>
    public static CommandShape FromBuilder(CommandBuilder builder)
    {
        var constructedParameters = new List<IParameterShape>();
        constructedParameters.AddRange(builder.Parameters.Select(p => p.Build()));

        return new CommandShape(constructedParameters, builder.Description ?? Constants.DefaultDescription);
    }

    /// <summary>
    /// Creates a new <see cref="CommandShape"/> from the given method.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <returns>The command shape.</returns>
    public static CommandShape FromMethod(MethodInfo method)
    {
        var positionalParameters = new List<IParameterShape>();
        var namedParameters = new List<IParameterShape>();

        foreach (var parameter in method.GetParameters())
        {
            var optionAttribute = parameter.GetCustomAttribute<OptionAttribute>();
            var rangeAttribute = parameter.GetCustomAttribute<RangeAttribute>();

            if (optionAttribute is null)
            {
                var newPositionalParameter = CreatePositionalParameterShape
                (
                    rangeAttribute,
                    parameter
                );

                positionalParameters.Add(newPositionalParameter);
            }
            else
            {
                var newNamedParameter = CreateNamedParameterShape
                (
                    optionAttribute,
                    rangeAttribute,
                    parameter
                );

                namedParameters.Add(newNamedParameter);
            }
        }

        var description = method.GetDescriptionOrDefault();
        return new CommandShape(namedParameters.Concat(positionalParameters).ToList(), description);
    }

    private static IParameterShape CreateNamedParameterShape
    (
        OptionAttribute optionAttribute,
        RangeAttribute? rangeAttribute,
        ParameterInfo parameter
    )
    {
        var isCollection = parameter.ParameterType.IsSupportedCollection();

        IParameterShape newNamedParameter;
        if (optionAttribute is SwitchAttribute)
        {
            newNamedParameter = CreateNamedSwitchParameterShape(optionAttribute, parameter);
        }
        else if (!isCollection)
        {
            var greedyAttribute = parameter.GetCustomAttribute<GreedyAttribute>();

            newNamedParameter = greedyAttribute is null
                ? CreateNamedSingleValueParameterShape(optionAttribute, parameter)
                : CreateGreedyNamedSingleValueParameterShape(optionAttribute, parameter);
        }
        else
        {
            newNamedParameter = CreateNamedCollectionParameterShape(optionAttribute, rangeAttribute, parameter);
        }

        return newNamedParameter;
    }

    private static IParameterShape CreateNamedCollectionParameterShape
    (
        OptionAttribute optionAttribute,
        RangeAttribute? rangeAttribute,
        ParameterInfo parameter
    )
    {
        var description = parameter.GetDescriptionOrDefault();

        IParameterShape newNamedParameter;
        if (optionAttribute.ShortName is null)
        {
            newNamedParameter = new NamedCollectionParameterShape
            (
                parameter,
                optionAttribute.LongName ?? throw new InvalidOperationException(),
                rangeAttribute?.GetMin(),
                rangeAttribute?.GetMax(),
                description
            );
        }
        else if (optionAttribute.LongName is null)
        {
            newNamedParameter = new NamedCollectionParameterShape
            (
                parameter,
                optionAttribute.ShortName ?? throw new InvalidOperationException(),
                rangeAttribute?.GetMin(),
                rangeAttribute?.GetMax(),
                description
            );
        }
        else
        {
            newNamedParameter = new NamedCollectionParameterShape
            (
                parameter,
                optionAttribute.ShortName ?? throw new InvalidOperationException(),
                optionAttribute.LongName ?? throw new InvalidOperationException(),
                rangeAttribute?.GetMin(),
                rangeAttribute?.GetMax(),
                description
            );
        }

        return newNamedParameter;
    }

    private static IParameterShape CreateNamedSwitchParameterShape
    (
        OptionAttribute optionAttribute,
        ParameterInfo parameter
    )
    {
        if (!parameter.IsOptional)
        {
            throw new InvalidOperationException
            (
                $"{parameter.Member.Name}::{parameter.Name} incorrectly declared: " +
                "switches must have a default value."
            );
        }

        if (parameter.ParameterType != typeof(bool))
        {
            throw new InvalidOperationException
            (
                $"{parameter.Member.Name}::{parameter.Name} incorrectly declared: " +
                "switches must be booleans."
            );
        }

        var description = parameter.GetDescriptionOrDefault();

        IParameterShape newNamedParameter;
        if (optionAttribute.ShortName is null)
        {
            newNamedParameter = new SwitchParameterShape
            (
                parameter,
                optionAttribute.LongName ?? throw new InvalidOperationException(),
                description
            );
        }
        else if (optionAttribute.LongName is null)
        {
            newNamedParameter = new SwitchParameterShape
            (
                parameter,
                optionAttribute.ShortName ?? throw new InvalidOperationException(),
                description
            );
        }
        else
        {
            newNamedParameter = new SwitchParameterShape
            (
                parameter,
                optionAttribute.ShortName ?? throw new InvalidOperationException(),
                optionAttribute.LongName ?? throw new InvalidOperationException(),
                description
            );
        }

        return newNamedParameter;
    }

    private static IParameterShape CreateNamedSingleValueParameterShape
    (
        OptionAttribute optionAttribute,
        ParameterInfo parameter
    )
    {
        var description = parameter.GetDescriptionOrDefault();

        IParameterShape newNamedParameter;
        if (optionAttribute.ShortName is null)
        {
            newNamedParameter = new NamedParameterShape
            (
                parameter,
                optionAttribute.LongName ?? throw new InvalidOperationException(),
                description
            );
        }
        else if (optionAttribute.LongName is null)
        {
            newNamedParameter = new NamedParameterShape
            (
                parameter,
                optionAttribute.ShortName ?? throw new InvalidOperationException(),
                description
            );
        }
        else
        {
            newNamedParameter = new NamedParameterShape
            (
                parameter,
                optionAttribute.ShortName ?? throw new InvalidOperationException(),
                optionAttribute.LongName ?? throw new InvalidOperationException(),
                description
            );
        }

        return newNamedParameter;
    }

    private static IParameterShape CreateGreedyNamedSingleValueParameterShape
    (
        OptionAttribute optionAttribute,
        ParameterInfo parameter
    )
    {
        var description = parameter.GetDescriptionOrDefault();

        IParameterShape newNamedParameter;
        if (optionAttribute.ShortName is null)
        {
            newNamedParameter = new NamedGreedyParameterShape
            (
                parameter,
                optionAttribute.LongName ?? throw new InvalidOperationException(),
                description
            );
        }
        else if (optionAttribute.LongName is null)
        {
            newNamedParameter = new NamedGreedyParameterShape
            (
                parameter,
                optionAttribute.ShortName ?? throw new InvalidOperationException(),
                description
            );
        }
        else
        {
            newNamedParameter = new NamedGreedyParameterShape
            (
                parameter,
                optionAttribute.ShortName ?? throw new InvalidOperationException(),
                optionAttribute.LongName ?? throw new InvalidOperationException(),
                description
            );
        }

        return newNamedParameter;
    }

    private static IParameterShape CreatePositionalParameterShape
    (
        RangeAttribute? rangeAttribute,
        ParameterInfo parameter
    )
    {
        var description = parameter.GetDescriptionOrDefault();
        var isCollection = parameter.ParameterType.IsSupportedCollection();

        IParameterShape newPositionalParameter;
        if (!isCollection)
        {
            var greedyAttribute = parameter.GetCustomAttribute<GreedyAttribute>();

            newPositionalParameter = greedyAttribute is null
                ? new PositionalParameterShape(parameter, description)
                : new PositionalGreedyParameterShape(parameter, description);
        }
        else
        {
            newPositionalParameter = new PositionalCollectionParameterShape
            (
                parameter,
                rangeAttribute?.GetMin(),
                rangeAttribute?.GetMax(),
                description
            );
        }

        return newPositionalParameter;
    }
}
