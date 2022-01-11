//
//  CommandTreeAccessor.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Remora.Commands.Trees;

namespace Remora.Commands.Services;

/// <summary>
/// Serves as an accessor service for multiple command trees.
/// </summary>
[PublicAPI]
public class CommandTreeAccessor
{
    /// <summary>
    /// Gets the name of the default tree.
    /// </summary>
    public static string DefaultTreeName => "__default";

    /// <summary>
    /// Gets the name of the tree that contains all configured modules.
    /// </summary>
    /// <remarks>
    /// This is mainly useful for help services.
    /// </remarks>
    public static string AllTreeName => "__all";

    private readonly CommandTreeAccessorOptions _accessorOptions;
    private readonly IOptionsSnapshot<CommandTreeBuilder> _treeBuilderSnapshot;
    private readonly Dictionary<string, CommandTree> _trees = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandTreeAccessor"/> class.
    /// </summary>
    /// <param name="accessorOptions">The accessor options.</param>
    /// <param name="treeBuilderSnapshot">The command tree builder snapshot.</param>
    public CommandTreeAccessor
    (
        IOptions<CommandTreeAccessorOptions> accessorOptions,
        IOptionsSnapshot<CommandTreeBuilder> treeBuilderSnapshot
    )
    {
        _accessorOptions = accessorOptions.Value;
        _treeBuilderSnapshot = treeBuilderSnapshot;

        if (!_accessorOptions.PreloadTrees)
        {
            return;
        }

        foreach (var treeName in _accessorOptions.TreeNames)
        {
            _trees.Add(treeName, _treeBuilderSnapshot.Get(treeName).Build());
        }
    }

    /// <summary>
    /// Attempts to retrieve a command tree by its configured name.
    /// </summary>
    /// <param name="treeName">The name of the tree. A null value signifies the default, unnamed tree.</param>
    /// <param name="tree">The tree, if any.</param>
    /// <returns>true if a tree with that name was found; otherwise, false.</returns>
    public bool TryGetNamedTree(string? treeName, [NotNullWhen(true)] out CommandTree? tree)
    {
        treeName ??= DefaultTreeName;
        tree = null;

        if (!_accessorOptions.TreeNames.Contains(treeName))
        {
            return false;
        }

        if (_trees.TryGetValue(treeName, out tree))
        {
            return true;
        }

        tree = _treeBuilderSnapshot.Get(treeName).Build();
        _trees.Add(treeName, tree);
        return true;
    }

    /// <summary>
    /// Gets all available trees.
    /// </summary>
    /// <returns>The trees, mapped to their names.</returns>
    public IReadOnlyDictionary<string, CommandTree> GetTrees()
    {
        if (_accessorOptions.PreloadTrees)
        {
            // all trees are already loaded
            return _trees;
        }

        foreach (var treeName in _accessorOptions.TreeNames)
        {
            if (_trees.ContainsKey(treeName))
            {
                continue;
            }

            _trees.Add(treeName, _treeBuilderSnapshot.Get(treeName).Build());
        }

        return _trees;
    }
}
