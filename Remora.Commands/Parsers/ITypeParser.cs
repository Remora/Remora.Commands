//
//  ITypeParser.cs
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
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Remora.Results;

#pragma warning disable CS1591, SA1402

namespace Remora.Commands.Parsers;

/// <summary>
/// Represents the internal API of a specialized type parser.
/// </summary>
/// <typeparam name="TType">The type to parse.</typeparam>
[PublicAPI]
public interface ITypeParser<TType> : ITypeParser
{
    /// <summary>
    /// Attempts to parse the given string into an instance of <typeparamref name="TType"/>.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns>A retrieval result which may or may not have succeeded.</returns>
    ValueTask<Result<TType>> TryParseAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Attempts to parse the given set of tokens into an instance of <typeparamref name="TType"/>.
    /// </summary>
    /// <param name="tokens">The tokens.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns>A retrieval result which may or may not have succeeded.</returns>
    ValueTask<Result<TType>> TryParseAsync(IReadOnlyList<string> tokens, CancellationToken ct = default);
}

/// <summary>
/// Represents the internal API of a general type parser.
/// </summary>
[PublicAPI]
public interface ITypeParser
{
    /// <summary>
    /// Determines whether the parser can parse the given type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>true if the parser can parse the type; otherwise, false.</returns>
    bool CanParse(Type type);

    /// <summary>
    /// Attempts to parse the given string into a CLR object.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="type">The type to parse.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns>A retrieval result which may or may not have succeeded.</returns>
    ValueTask<Result<object?>> TryParseAsync(string token, Type type, CancellationToken ct);

    /// <summary>
    /// Attempts to parse the given string into a CLR object.
    /// </summary>
    /// <param name="tokens">The token.</param>
    /// <param name="type">The type to parse.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns>A retrieval result which may or may not have succeeded.</returns>
    ValueTask<Result<object?>> TryParseAsync(IReadOnlyList<string> tokens, Type type, CancellationToken ct);
}
