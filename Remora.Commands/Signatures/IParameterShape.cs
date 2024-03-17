//
//  IParameterShape.cs
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
using Remora.Commands.Tokenization;
using Remora.Commands.Trees;

namespace Remora.Commands.Signatures;

/// <summary>
/// Represents the "shape" of a single parameter. This type is used to determine whether a sequence of tokens could
/// fit the associated parameter.
/// </summary>
[PublicAPI]
public interface IParameterShape
{
    /// <summary>
    /// Gets the default value, if any.
    /// </summary>
    object? DefaultValue { get; }

    /// <summary>
    /// Gets a string that can be used to refer to the parameter in human-readable content. Typically, this is the
    /// configured name of the parameter, or a unique fallback value.
    /// </summary>
    string HintName { get; }

    /// <summary>
    /// Gets a user-configured description of the parameter.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets a list of attributes that apply to this parameter.
    /// </summary>
    IReadOnlyList<Attribute> Attributes { get; }

    /// <summary>
    /// Gets a list of conditions that apply to this parameter.
    /// </summary>
    IReadOnlyList<ConditionAttribute> Conditions { get; }

    /// <summary>
    /// Gets the type the parameter represents, or otherwise should be parsed as.
    /// </summary>
    Type ParameterType { get; }

    /// <summary>
    /// Gets a value indicating whether the parameter's type allows for null values.
    /// </summary>
    bool IsNullable { get; }

    /// <summary>
    /// Determines whether the given token sequence matches the parameter shape.
    /// </summary>
    /// <param name="tokenizer">The token sequence.</param>
    /// <param name="consumedTokens">The number of tokens that would be consumed by this parameter.</param>
    /// <param name="searchOptions">A set of search options.</param>
    /// <returns>true if the shape matches; otherwise, false.</returns>
    public bool Matches
    (
        TokenizingEnumerator tokenizer,
        out ulong consumedTokens,
        TreeSearchOptions? searchOptions = null
    );

    /// <summary>
    /// Determines whether the given named value matches the parameter shape.
    /// </summary>
    /// <param name="namedValue">The named value.</param>
    /// <param name="isFatal">Whether the mismatch was fatal, and the entire command should be rejected.</param>
    /// <param name="searchOptions">A set of search options.</param>
    /// <returns>true if the shape matches; otherwise, false.</returns>
    public bool Matches
    (
        KeyValuePair<string, IReadOnlyList<string>> namedValue,
        out bool isFatal,
        TreeSearchOptions? searchOptions = null
    );

    /// <summary>
    /// Determines whether the parameter is omissible; that is, it is either optional or has a well-established
    /// default value.
    /// </summary>
    /// <param name="searchOptions">A set of search options.</param>
    /// <returns>true if the parameter is omissible; otherwise, false.</returns>
    public bool IsOmissible(TreeSearchOptions? searchOptions = null);
}
