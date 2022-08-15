//
//  AbstractTypeParser.cs
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
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Remora.Results;

#pragma warning disable CS1591, SA1402

namespace Remora.Commands.Parsers;

/// <summary>
/// Represents an abstract type parser.
/// </summary>
/// <typeparam name="TType">The type to parse.</typeparam>
[PublicAPI]
public abstract class AbstractTypeParser<TType> : ITypeParser<TType>
{
    /// <inheritdoc/>
    bool ITypeParser.CanParse(Type type)
    {
        return type.IsAssignableFrom(typeof(TType));
    }

    /// <inheritdoc />
    public virtual ValueTask<Result<TType>> TryParseAsync
    (
        string token,
        CancellationToken ct = default
    )
        => new(new InvalidOperationError("This parser doesn't support single-token parsing."));

    /// <inheritdoc/>
    public virtual ValueTask<Result<TType>> TryParseAsync
    (
        IReadOnlyList<string> tokens,
        CancellationToken ct = default
    )
        => new(new InvalidOperationError("This parser doesn't support multi-token parsing."));

    /// <inheritdoc/>
    async ValueTask<Result<object?>> ITypeParser.TryParseAsync
    (
        string token,
        Type type,
        CancellationToken ct
    )
    {
        return (await TryParseAsync(token, ct)).Map(x => (object?)x);
    }

    /// <inheritdoc/>
    async ValueTask<Result<object?>> ITypeParser.TryParseAsync
    (
        IReadOnlyList<string> tokens,
        Type type,
        CancellationToken ct
    )
    {
        return (await TryParseAsync(tokens, ct)).Map(x => (object?)x);
    }
}

/// <summary>
/// Represents an abstract type parser.
/// </summary>
[PublicAPI]
public abstract class AbstractTypeParser : ITypeParser
{
    /// <inheritdoc/>
    public abstract bool CanParse(Type type);

    /// <inheritdoc/>
    public virtual ValueTask<Result<object?>> TryParseAsync
    (
        string token,
        Type type,
        CancellationToken ct = default
    )
        => new(new InvalidOperationError("This parser doesn't support single-token parsing."));

    /// <inheritdoc/>
    public virtual ValueTask<Result<object?>> TryParseAsync
    (
        IReadOnlyList<string> tokens,
        Type type,
        CancellationToken ct = default
    )
        => new(new InvalidOperationError("This parser doesn't support multi-token parsing."));
}
