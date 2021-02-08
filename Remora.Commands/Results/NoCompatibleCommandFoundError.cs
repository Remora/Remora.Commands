//
//  NoCompatibleCommandFoundError.cs
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

using System.Collections.Generic;
using JetBrains.Annotations;
using Remora.Commands.Signatures;
using Remora.Results;

namespace Remora.Commands.Results
{
    /// <summary>
    /// Represents a failure to find a compatible command that could be executed from a list of candidates.
    /// </summary>
    [PublicAPI]
    public record NoCompatibleCommandFoundError : ResultError
    {
        /// <summary>
        /// Gets the command candidates that were considered.
        /// </summary>
        public IReadOnlyList<BoundCommandNode> Candidates { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoCompatibleCommandFoundError"/> class.
        /// </summary>
        /// <param name="candidates">The command candidates that were considered.</param>
        public NoCompatibleCommandFoundError(IReadOnlyList<BoundCommandNode> candidates)
            : base("No compatible, executable command could be found.")
        {
            this.Candidates = candidates;
        }
    }
}
