//
//  MissingParserError.cs
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
using JetBrains.Annotations;
using Remora.Results;

namespace Remora.Commands.Results
{
    /// <summary>
    /// Represents the lack of a parser for a given type.
    /// </summary>
    [PublicAPI]
    public record MissingParserError : ResultError
    {
        /// <summary>
        /// Gets the type for which no parser has been registered.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingParserError"/> class.
        /// </summary>
        /// <param name="type">The type for which no parser has been registered.</param>
        public MissingParserError(Type type)
            : base($"No parser has been registered for \"{type.Name}\".")
        {
            this.Type = type;
        }
    }
}
