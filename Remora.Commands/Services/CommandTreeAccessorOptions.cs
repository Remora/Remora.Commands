//
//  CommandTreeAccessorOptions.cs
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

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Remora.Commands.Services;

/// <summary>
/// Represents configurable options for the <see cref="CommandTreeAccessor"/>.
/// </summary>
[PublicAPI]
public class CommandTreeAccessorOptions
{
    private readonly List<string> _treeNames = new() { Constants.DefaultTreeName, Constants.AllTreeName };

    /// <summary>
    /// Gets the names of the registered trees.
    /// </summary>
    public IReadOnlyList<string> TreeNames => _treeNames;

    /// <summary>
    /// Gets or sets a value indicating whether all command trees should be preloaded at startup. If false, each tree
    /// will be loaded as it is requested and then cached.
    /// </summary>
    public bool PreloadTrees { get; set; }

    /// <summary>
    /// Adds a registered tree name.
    /// </summary>
    /// <param name="treeName">The tree name.</param>
    internal void AddTreeName(string? treeName)
    {
        treeName ??= Constants.DefaultTreeName;
        if (_treeNames.Contains(treeName))
        {
            return;
        }

        _treeNames.Add(treeName);
    }
}
