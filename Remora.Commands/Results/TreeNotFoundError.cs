//
//  TreeNotFoundError.cs
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
using Remora.Commands.Services;
using Remora.Results;

namespace Remora.Commands.Results;

/// <summary>
/// Represents a failure to find a tree.
/// </summary>
/// <param name="TreeName">The name of the tree. Defaults to the default tree name if null.</param>
[PublicAPI]
public record TreeNotFoundError(string? TreeName)
    : NotFoundError($"No tree named \"{TreeName ?? Constants.DefaultTreeName}\" found.");
