//
//  AmbiguousCommandInvocationError.cs
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
using Remora.Commands.Services;
using Remora.Results;

namespace Remora.Commands.Results;

/// <summary>
/// Raised when two or more commands pass all preconditions and are otherwise acceptable as execution candidates.
/// </summary>
/// <param name="CommandCandidates">The potential commands that could have been executed.</param>
[PublicAPI]
public record AmbiguousCommandInvocationError(IReadOnlyList<PreparedCommand>? CommandCandidates)
    : ResultError("Two or more commands could have been executed by that.");
