//
//  SwitchParameterShape.cs
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
using Remora.Commands.Conditions;
using Remora.Commands.Extensions;
using Remora.Commands.Tokenization;
using Remora.Commands.Trees;

namespace Remora.Commands.Signatures;

/// <summary>
/// Represents a switch parameter that maps to a boolean value.
/// </summary>
[PublicAPI]
public class SwitchParameterShape : IParameterShape
{
    /// <summary>
    /// Gets the short name of the parameter, if any. At least one of <see cref="ShortName"/> and
    /// <see cref="LongName"/> must be set.
    /// </summary>
    public char? ShortName { get; }

    /// <summary>
    /// Gets the long name of the parameter, if any. At least one of <see cref="ShortName"/> and
    /// <see cref="LongName"/> must be set.
    /// </summary>
    public string? LongName { get; }

    /// <inheritdoc/>
    public virtual object? DefaultValue { get; }

    /// <inheritdoc/>
    public string HintName
    {
        get
        {
            if (this.LongName is not null)
            {
                return this.LongName;
            }

            if (this.ShortName is not null)
            {
                return this.ShortName.ToString() ?? throw new InvalidOperationException();
            }

            return this._parameterName ?? throw new InvalidOperationException();
        }
    }

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
    /// Initializes a new instance of the <see cref="SwitchParameterShape"/> class.
    /// </summary>
    /// <param name="parameter">The underlying parameter.</param>
    /// <param name="shortName">The short name.</param>
    /// <param name="longName">The long name.</param>
    /// <param name="description">The description of the parameter.</param>
    public SwitchParameterShape
    (
        ParameterInfo parameter,
        char shortName,
        string longName,
        string? description = null
    )
    : this(parameter)
    {
        this.ShortName = shortName;
        this.LongName = longName;
        this.Description = description ?? Constants.DefaultDescription;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SwitchParameterShape"/> class.
    /// </summary>
    /// <param name="parameter">The underlying parameter.</param>
    /// <param name="shortName">The short name.</param>
    /// <param name="description">The description of the parameter.</param>
    public SwitchParameterShape(ParameterInfo parameter, char shortName, string? description = null)
    : this(parameter)
    {
        this.ShortName = shortName;
        this.Description = description ?? Constants.DefaultDescription;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SwitchParameterShape"/> class.
    /// </summary>
    /// <param name="parameter">The underlying parameter.</param>
    /// <param name="longName">The long name.</param>
    /// <param name="description">The description of the parameter.</param>
    public SwitchParameterShape
    (
        ParameterInfo parameter,
        string longName,
        string? description = null
    )
    : this(parameter)
    {
        this.LongName = longName;
        this.Description = description ?? Constants.DefaultDescription;
    }

    private SwitchParameterShape(ParameterInfo parameter)
    {
        this.DefaultValue = parameter.DefaultValue;
        this.ParameterType = parameter.ParameterType;
        this.Attributes = parameter.GetCustomAttributes().Where(a => !typeof(ConditionAttribute).IsAssignableFrom(a.GetType())).ToArray();
        this.Conditions = parameter.GetCustomAttributes().Where(a => typeof(ConditionAttribute).IsAssignableFrom(a.GetType())).Cast<ConditionAttribute>().ToArray();
        this.IsNullable = parameter.AllowsNull();
        this.Description = Constants.DefaultDescription;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SwitchParameterShape"/> class.
    /// </summary>
    /// <param name="shortName">The short name.</param>
    /// <param name="longName">The long name.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="parameterType">The type of the parameter.</param>
    /// <param name="isOptional">Whether the parameter is optional.</param>
    /// <param name="defaultValue">The default value of the parameter, if any.</param>
    /// <param name="attributes">The attributes of the parameter.</param>
    /// <param name="conditions">The conditions of the parameter.</param>
    /// <param name="description">The description of the paremeter.</param>
    public SwitchParameterShape
    (
        char? shortName,
        string? longName,
        string parameterName,
        Type parameterType,
        bool isOptional,
        object? defaultValue,
        IReadOnlyList<Attribute> attributes,
        IReadOnlyList<ConditionAttribute> conditions,
        string description
    )
    {
        this.ShortName = shortName;
        this.LongName = longName;
        this._parameterName = parameterName;
        this.ParameterType = parameterType;
        this._isOptional = isOptional;
        this.DefaultValue = defaultValue;
        this.Attributes = attributes;
        this.Conditions = conditions;
        this.Description = description;
    }

    /// <inheritdoc/>
    public virtual bool Matches
    (
        TokenizingEnumerator tokenizer,
        out ulong consumedTokens,
        TreeSearchOptions? searchOptions = null
    )
    {
        searchOptions ??= new TreeSearchOptions();
        consumedTokens = 0;

        if (!tokenizer.MoveNext())
        {
            return false;
        }

        switch (tokenizer.Current.Type)
        {
            case TokenType.LongName:
            {
                if (this.LongName is null)
                {
                    return false;
                }

                if (!tokenizer.Current.Value.Equals(this.LongName, searchOptions.KeyComparison))
                {
                    return false;
                }

                break;
            }
            case TokenType.ShortName:
            {
                if (this.ShortName is null)
                {
                    return false;
                }

                if (tokenizer.Current.Value.Length != 1)
                {
                    return false;
                }

                if (tokenizer.Current.Value[0] != this.ShortName.Value)
                {
                    return false;
                }

                break;
            }
            case TokenType.Value:
            {
                return false;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        consumedTokens = 1;
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

        var (name, value) = namedValue;

        var nameMatches = name.Equals(this.LongName, searchOptions.KeyComparison) ||
                          (this.ShortName is not null && name.Length == 1 && name[0] == this.ShortName);

        if (!nameMatches)
        {
            return false;
        }

        if (value.Count == 0)
        {
            return true;
        }

        isFatal = true;
        return false;
    }

    /// <inheritdoc/>
    public virtual bool IsOmissible(TreeSearchOptions? searchOptions = null) => this._isOptional;
}
