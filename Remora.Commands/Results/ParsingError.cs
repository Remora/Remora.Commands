//
//  ParsingError.cs
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

using JetBrains.Annotations;
using Remora.Results;

namespace Remora.Commands.Results;

/// <summary>
/// Represents a failure to parse the given type.
/// </summary>
/// <typeparam name="TType">The type that's being parsed.</typeparam>
[PublicAPI]
public record ParsingError<TType> : ResultError
{
    /// <summary>
    /// Gets the reason why the parsing failed.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParsingError{TType}"/> class.
    /// </summary>
    /// <param name="value">The value that failed to parse.</param>
    /// <param name="reason">The reason why the parsing failed.</param>
    public ParsingError(string? value, string? reason = null)
        : base($"Failed to parse \"{value ?? "null"}\" as an instance of the type \"{typeof(TType)}\"")
    {
        this.Reason = reason ?? "No reason given.";
    }
}
