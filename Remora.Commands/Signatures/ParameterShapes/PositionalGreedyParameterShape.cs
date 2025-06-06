//
//  PositionalGreedyParameterShape.cs
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
using System.Reflection;
using JetBrains.Annotations;
using Remora.Commands.Conditions;
using Remora.Commands.Extensions;
using Remora.Commands.Tokenization;
using Remora.Commands.Trees;
using static Remora.Commands.Tokenization.TokenType;

namespace Remora.Commands.Signatures;

/// <summary>
/// Represents a single value without a name.
/// </summary>
[PublicAPI]
public class PositionalGreedyParameterShape : IParameterShape
{
    /// <inheritdoc/>
    public virtual object? DefaultValue { get; }

    /// <inheritdoc/>
    public string HintName => _parameterName ?? throw new InvalidOperationException();

    /// <inheritdoc/>
    public string Description { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Attribute> Attributes { get; }

    /// <inheritdoc/>
    public IReadOnlyList<ConditionAttribute> Conditions { get; }

    /// <inheritdoc/>
    public Type ParameterType { get; }

    /// <inheritdoc/>
    public bool IsNullable { get; }

    private readonly bool _isOptional;
    private readonly string? _parameterName;

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionalGreedyParameterShape"/> class.
    /// </summary>
    /// <param name="parameter">The underlying parameter.</param>
    /// <param name="description">The description of the parameter.</param>
    public PositionalGreedyParameterShape(ParameterInfo parameter, string? description = null)
    {
        parameter.GetAttributesAndConditions(out var attributes, out var conditions);

        _parameterName = parameter.Name;
        _isOptional = parameter.IsOptional;
        this.DefaultValue = parameter.DefaultValue;
        this.ParameterType = parameter.ParameterType;
        this.Attributes = attributes;
        this.Conditions = conditions;
        this.IsNullable = parameter.AllowsNull();
        this.Description = description ?? Constants.DefaultDescription;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionalGreedyParameterShape"/> class.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="parameterType">The type of the parameter.</param>
    /// <param name="isOptional">Whether the parameter is optional.</param>
    /// <param name="defaultValue">The default value of the parameter, if any.</param>
    /// <param name="attributes">The attributes of the parameter.</param>
    /// <param name="conditions">The conditions of the parameter.</param>
    /// <param name="description">The description of the paremeter.</param>
    public PositionalGreedyParameterShape
    (
        string parameterName,
        Type parameterType,
        bool isOptional,
        object? defaultValue,
        IReadOnlyList<Attribute> attributes,
        IReadOnlyList<ConditionAttribute> conditions,
        string description
    )
    {
        _isOptional = isOptional;
        _parameterName = parameterName;
        this.ParameterType = parameterType;
        this.DefaultValue = defaultValue;
        this.IsNullable = parameterType.IsNullable();
        this.Attributes = attributes;
        this.Conditions = conditions;
        this.Description = description;
    }

    /// <inheritdoc />
    public virtual bool Matches
    (
        TokenizingEnumerator tokenizer,
        out ulong consumedTokens,
        TreeSearchOptions? searchOptions = null
    )
    {
        consumedTokens = 0;

        // Eat at least one value token
        if (!tokenizer.MoveNext())
        {
            return false;
        }

        if (tokenizer.Current.Type is not Value)
        {
            return false;
        }

        ulong consumedValueTokens = 1;
        while (tokenizer.MoveNext() && tokenizer.Current.Type is Value)
        {
            consumedValueTokens++;
        }

        consumedTokens = consumedValueTokens;
        return true;
    }

    /// <inheritdoc/>
    public virtual bool Matches
    (
        KeyValuePair<string, IReadOnlyList<string>> namedValue,
        out bool isFatal,
        TreeSearchOptions? searchOptions = null
    )
    {
        searchOptions ??= new TreeSearchOptions();
        isFatal = false;

        // This one is a bit of a special case. Since all parameters are named in the case of pre-bound parameters,
        // we'll use the actual parameter name as a hint to match against.
        var (name, value) = namedValue;

        if (!name.Equals(_parameterName, searchOptions.KeyComparison))
        {
            return false;
        }

        if (value.Count >= 1)
        {
            return true;
        }

        isFatal = true;
        return false;
    }

    /// <inheritdoc/>
    public virtual bool IsOmissible(TreeSearchOptions? searchOptions = null) => _isOptional;
}
