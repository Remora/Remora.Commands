//
//  CommandTree.cs
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
using System.Linq;
using Remora.Commands.Signatures;
using Remora.Commands.Tokenization;
using Remora.Commands.Trees.Nodes;

namespace Remora.Commands.Trees
{
    /// <summary>
    /// Represents a tree view of the available commands.
    /// </summary>
    public class CommandTree
    {
        /// <summary>
        /// Gets the root of the command tree.
        /// </summary>
        public RootNode Root { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandTree"/> class.
        /// </summary>
        /// <param name="root">The root of the command tree.</param>
        public CommandTree(RootNode root)
        {
            this.Root = root;
        }

        /// <summary>
        /// Searches the command tree for a command that matches the shape of the given command string.
        /// </summary>
        /// <param name="commandString">The raw command string.</param>
        /// <returns>A search result which may or may not have succeeded.</returns>
        public IEnumerable<BoundCommandNode> Search(ReadOnlySpan<char> commandString)
        {
            var tokenizer = new TokenizingEnumerator(commandString);
            return Search(this.Root, tokenizer);
        }

        /// <summary>
        /// Searches the command tree for a command that matches the shape of the given command name string, and its
        /// named parameters. In this case, positional parameters are not supported; they are matched against their
        /// in-source names instead.
        /// </summary>
        /// <param name="commandNameString">The named command string.</param>
        /// <param name="namedParameters">The named parameters.</param>
        /// <returns>The matching command nodes.</returns>
        public IEnumerable<BoundCommandNode> Search
        (
            ReadOnlySpan<char> commandNameString,
            IReadOnlyDictionary<string, IReadOnlyList<string>> namedParameters
        )
        {
            var splitEnumerator = new SpanSplitEnumerator(commandNameString, " ");

            var matchingNodes = Search(this.Root, splitEnumerator);
            var boundNodes = matchingNodes
                .Select
                (
                    c =>
                    (
                        IsSuccess: c.TryBind(namedParameters, out var boundCommandNode),
                        BoundCommandNode: boundCommandNode
                    )
                )
                .Where(kvp => kvp.IsSuccess)
                .Select(kvp => kvp.BoundCommandNode!);

            return boundNodes;
        }

        /// <summary>
        /// Performs a depth-first search of the given node.
        /// </summary>
        /// <param name="parentNode">The node.</param>
        /// <param name="enumerator">The splitting enumerator.</param>
        /// <returns>The matching nodes.</returns>
        private IEnumerable<CommandNode> Search
        (
            IParentNode parentNode,
            SpanSplitEnumerator enumerator
        )
        {
            var commandNodes = new List<CommandNode>();

            foreach (var child in parentNode.Children)
            {
                if (!IsNodeMatch(child, enumerator))
                {
                    continue;
                }

                switch (child)
                {
                    case CommandNode commandNode:
                    {
                        commandNodes.Add(commandNode);
                        break;
                    }
                    case IParentNode groupNode:
                    {
                        // Consume the token
                        if (!enumerator.MoveNext())
                        {
                            // No more tokens, so we can't continue searching
                            return commandNodes;
                        }

                        var nestedResults = Search(groupNode, enumerator);
                        commandNodes.AddRange(nestedResults);

                        continue;
                    }
                    default:
                    {
                        throw new InvalidOperationException
                        (
                            "Unknown node type encountered; tree is invalid and the search cannot continue."
                        );
                    }
                }
            }

            return commandNodes;
        }

        /// <summary>
        /// Performs a depth-first search of the given node.
        /// </summary>
        /// <param name="parentNode">The node.</param>
        /// <param name="tokenizer">The tokenizer.</param>
        /// <returns>The matching nodes.</returns>
        private IEnumerable<BoundCommandNode> Search(IParentNode parentNode, TokenizingEnumerator tokenizer)
        {
            var boundCommandNodes = new List<BoundCommandNode>();
            foreach (var child in parentNode.Children)
            {
                if (!IsNodeMatch(child, tokenizer))
                {
                    continue;
                }

                switch (child)
                {
                    case CommandNode commandNode:
                    {
                        if (!commandNode.TryBind(tokenizer, out var boundCommandShape))
                        {
                             continue;
                        }

                        boundCommandNodes.Add(boundCommandShape);
                        break;
                    }
                    case IParentNode groupNode:
                    {
                        // Consume the token
                        if (!tokenizer.MoveNext())
                        {
                            // No more tokens, so we can't continue searching
                            return boundCommandNodes;
                        }

                        var nestedResults = Search(groupNode, tokenizer);
                        boundCommandNodes.AddRange(nestedResults);

                        continue;
                    }
                    default:
                    {
                        throw new InvalidOperationException
                        (
                            "Unknown node type encountered; tree is invalid and the search cannot continue."
                        );
                    }
                }
            }

            return boundCommandNodes;
        }

        /// <summary>
        /// Determines whether a node matches the current state of the enumerator.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="enumerator">The tokenizer.</param>
        /// <returns>true if the node matches; otherwise, false.</returns>
        private bool IsNodeMatch(IChildNode node, SpanSplitEnumerator enumerator)
        {
            if (!enumerator.MoveNext())
            {
                return false;
            }

            if (enumerator.Current.Equals(node.Key, StringComparison.Ordinal))
            {
                return true;
            }

            foreach (var alias in node.Aliases)
            {
                if (enumerator.Current.Equals(alias, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether a node matches the current state of the tokenizer.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="tokenizer">The tokenizer.</param>
        /// <returns>true if the node matches; otherwise, false.</returns>
        private bool IsNodeMatch(IChildNode node, TokenizingEnumerator tokenizer)
        {
            if (!tokenizer.MoveNext())
            {
                return false;
            }

            if (tokenizer.Current.Type != TokenType.Value)
            {
                return false;
            }

            if (tokenizer.Current.Value.Equals(node.Key, StringComparison.Ordinal))
            {
                return true;
            }

            foreach (var alias in node.Aliases)
            {
                if (tokenizer.Current.Value.Equals(alias, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
