//
//  CommandHelpService.cs
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
using System.Reflection;
using JetBrains.Annotations;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.CommandInformation;
using Remora.Commands.Conditions;
using Remora.Commands.Extensions;
using Remora.Commands.Groups;
using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Remora.Commands.Services
{
    /// <inheritdoc />
    [PublicAPI]
    public sealed class CommandHelpService : ICommandHelpService
    {
        private readonly CommandService _commandService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHelpService"/> class.
        /// </summary>
        /// <param name="commandSerice">The <see cref="CommandService"/>.</param>
        public CommandHelpService(CommandService commandSerice)
        {
            _commandService = commandSerice;
        }

        /// <inheritdoc />
        public Result<OneOf<IGroupInfo, ICommandInfo, ICommandInfo[]>> FindInfo(string commandString, Tokenization.TokenizerOptions? tokenizerOptions = null, Trees.TreeSearchOptions? treeSearchOptions = null)
        {
            tokenizerOptions ??= new Tokenization.TokenizerOptions();
            treeSearchOptions ??= new Trees.TreeSearchOptions();

            if (string.IsNullOrWhiteSpace(commandString))
            {
                return new ArgumentNullError(nameof(commandString), "Command String cannot be null, empty, or comprised entirely of whitespace.");
            }

            var parts = commandString.Split(tokenizerOptions.Delimiter);

            // Walk the tree
            IParentNode parentNode = _commandService.Tree.Root;
            List<IChildNode> childNodes = new();
            foreach (var part in parts)
            {
                // Search for a child node matching the name of the next item in the sequence.
                var foundNodes = parentNode.Children.Where(child => child.Key.Equals(part, treeSearchOptions.KeyComparison)).ToList();
                var firstNode = foundNodes.FirstOrDefault();

                // We searched for a child node with that key, but nothing was found.
                if (firstNode is null)
                {
                    return new NotFoundError($"Cound not find any nodes matching the specified search options.");
                }

                // A node was found and it is a parent node (probably a group).
                // TODO: Handle edge case where a node and a command have the same key and we're at the end of the search.
                if (firstNode is IParentNode parent)
                {
                    parentNode = parent;
                    continue;
                }

                // A node was found and it is a child node (probably a command).
                if (firstNode is IChildNode child)
                {
                    // Return any possible siblings too.
                    childNodes = foundNodes;
                }
            }

            if (!childNodes.Any() && parentNode is GroupNode group)
            {
                var getResult = GetGroupInfo(group);
                if (getResult.IsDefined(out var groupInfo))
                {
                    return OneOf<IGroupInfo, ICommandInfo, ICommandInfo[]>.FromT0(groupInfo);
                }

                return Result<OneOf<IGroupInfo, ICommandInfo, ICommandInfo[]>>.FromError(getResult);
            }

            if (childNodes.Any())
            {
                if (childNodes.Count() == 1)
                {
                    if (childNodes[0] is CommandNode command)
                    {
                        var getResult = GetCommandInfo(command);
                        if (getResult.IsDefined(out var commandInfo))
                        {
                            return OneOf<IGroupInfo, ICommandInfo, ICommandInfo[]>.FromT1(commandInfo);
                        }
                        return Result<OneOf<IGroupInfo, ICommandInfo, ICommandInfo[]>>.FromError(getResult);
                    }

                    return new NotSupportedError("Cannot get information for a child node that is not a command node.");
                }

                // Multiple nodes found
                var commandInfos = new List<ICommandInfo>();
                var resultErrors = new List<IResultError>();
                foreach (var child in childNodes)
                {
                    if (child is CommandNode commandNode)
                    {
                        var getResult = GetCommandInfo(commandNode);
                        if (getResult.IsDefined(out var commandInfo))
                        {
                            commandInfos.Add(commandInfo);
                        }

                        resultErrors.Add(getResult.Error!);
                    }
                    resultErrors.Add(new NotSupportedError("Cannot get information for a child node that is not a command node."));
                }

                if (commandInfos.Any())
                {
                    return OneOf<IGroupInfo, ICommandInfo, ICommandInfo[]>.FromT2(commandInfos.ToArray());
                }

                return new AggregateError((IReadOnlyCollection<IResult>)resultErrors.AsReadOnly());
            }

            return new InvalidOperationError("The search operation failed.");
        }

        /// <inheritdoc />
        public Result<IGroupInfo> GetGroupInfo(Type commandGroupType, bool buildChildGroups = false)
        {
            // TODO: This only finds root-level groups currently. This should also search child groups.
            var groups = _commandService.Tree.Root.Children.Where(x => x is GroupNode && x.Key == commandGroupType.Name).Cast<GroupNode>();

            if (!groups.Any())
            {
                return new NotFoundError($"Could not find any groups of type '{commandGroupType.Name}' registered with the CommandService.");
            }
            else if (groups.Count() == 1)
            {
                var group = groups.First();

                var infoResult = GetGroupInfo(group!, buildChildGroups);

                return infoResult;
            }
            else
            {
                // More than one with the same name.
                var fullPath = commandGroupType.FullName;

                var group = groups.FirstOrDefault(group => fullPath.EndsWith(WalkNamespace(group)));

                if (group is null)
                {
                    return new NotFoundError($"Could not find the requested group of type '{commandGroupType.Name}' registered with the CommandService.");
                }

                var infoResult = GetGroupInfo(group, buildChildGroups);

                return infoResult;
            }
        }

        /// <inheritdoc />
        public Result<IGroupInfo> GetGroupInfo<TCommandGroup>(bool buildChildGroups = false)
            where TCommandGroup : CommandGroup
            => GetGroupInfo(typeof(TCommandGroup), buildChildGroups);

        /// <inheritdoc />
        public Result<IRootInfo> GetAllCommands()
        {
            var root = _commandService.Tree.Root;

            var rootCommands = new List<ICommandInfo>();
            var rootGroups = new List<IGroupInfo>();

            foreach (var child in root.Children)
            {
                if (child is CommandNode command)
                {
                    var infoResult = GetCommandInfo(command);
                    if (infoResult.IsDefined(out var commandInfo))
                    {
                        rootCommands.Add(commandInfo);
                    }
                }
                else if (child is GroupNode group)
                {
                    var infoResult = GetGroupInfo(group, true);
                    if (infoResult.IsDefined(out var groupInfo))
                    {
                        rootGroups.Add(groupInfo);
                    }
                }
                else
                {
                    // Unsupported at this time. Just skip.
                    continue;
                }
            }

            return new RootInfo(rootGroups.AsReadOnly(), rootCommands.AsReadOnly());
        }

        private Result<OneOf<ICommandInfo, ICommandInfo[]>> GetCommandInfo(ReadOnlySpan<char> commandString, Tokenization.TokenizerOptions? tokenizerOptions = null, Trees.TreeSearchOptions? searchOptions = null)
        {
            var searchResults = _commandService.Tree.Search(commandString, tokenizerOptions, searchOptions);

            if (!searchResults.Any())
            {
                return new NotFoundError($"Cound not find any commands matching the specified search options.");
            }

            List<ICommandInfo> results = new();

            foreach (var result in searchResults)
            {
                var hiddenAttribute = result.Node.CommandMethod.GetCustomAttribute<HiddenFromHelpAttribute>();

                results.Add(new CommandInfo
                (
                    Name: result.Node.Key,
                    Description: result.Node.Description,
                    Aliases: result.Node.Aliases,
                    Hidden: hiddenAttribute is not null,
                    HiddenFromHelpComment: hiddenAttribute?.Comment ?? null,
                    GetConditionInfo(result.Node),
                    result.Node.CommandMethod.GetParameters()
                ));
            }

            return results.Count == 1
                ? OneOf<ICommandInfo, ICommandInfo[]>.FromT0(results[0])
                : OneOf<ICommandInfo, ICommandInfo[]>.FromT1(results.ToArray());
        }

        /// <summary>
        /// Gets a <see cref="ICommandInfo"/> for the provided <see cref="CommandNode"/>.
        /// </summary>
        /// <param name="node">The <see cref="CommandNode"/> to inspect.</param>
        /// <returns>An <see cref="ICommandInfo"/> built from the provided node.</returns>
        internal Result<ICommandInfo> GetCommandInfo(CommandNode node)
        {
            var name = node.Key;
            var description = node.Description;
            var aliases = node.Aliases;

            var conditionInfo = GetConditionInfo(node);
            var argumentInfo = node.CommandMethod.GetParameters();

            var commandInfo = new CommandInfo(name, description, aliases, false, null, conditionInfo, argumentInfo);

            var hiddenAttribute = node.CommandMethod.GetCustomAttribute<HiddenFromHelpAttribute>();
            if (hiddenAttribute is { } present)
            {
                return commandInfo with
                {
                    Hidden = true,
                    HiddenFromHelpComment = present.Comment
                };
            }

            return commandInfo;
        }

        /// <summary>
        /// Gets a <see cref="IGroupInfo"/> for the provided <see cref="GroupNode"/>.
        /// </summary>
        /// <param name="node">The <see cref="GroupNode"/> to inspect.</param>
        /// <param name="buildChildGroups">If true, child nodes will be created and populated. If false, the <see cref="IGroupInfo.ChildGroups"/> collection will be empty.</param>
        /// <returns>An <see cref="IGroupInfo"/> built from the provided node.</returns>
        internal Result<IGroupInfo> GetGroupInfo(GroupNode node, bool buildChildGroups = false)
        {
            var name = node.Key;
            var description = node.Description;
            var aliases = node.Aliases;

            var childCommands = new List<ICommandInfo>();
            var childGroups = new List<IGroupInfo>();

            foreach (var child in node.Children)
            {
                if (buildChildGroups && child is GroupNode group)
                {
                    var infoResult = GetGroupInfo(group, buildChildGroups);
                    if (infoResult.IsDefined(out var groupInfo))
                    {
                        childGroups.Add(groupInfo);
                    }
                }
                else if (child is CommandNode command)
                {
                    var infoResult = GetCommandInfo(command);
                    if (infoResult.IsDefined(out var commandInfo))
                    {
                        childCommands.Add(commandInfo);
                    }
                }
                else
                {
                    // Not supported at this time.
                    continue;
                }
            }

            var info = new GroupInfo(name, description, aliases, false, null, childCommands.AsReadOnly(), childGroups.AsReadOnly());

            var hiddenAttribute = node.GetType().GetCustomAttribute<HiddenFromHelpAttribute>();
            if (hiddenAttribute is { } present)
            {
                return info with
                {
                    Hidden = true,
                    HiddenFromHelpComment = present.Comment
                };
            }

            return info;
        }

        /// <summary>
        /// Gets a read-only list of <see cref="IConditionInfo"/>s for the provided <see cref="CommandNode"/>.
        /// </summary>
        /// <param name="node">The <see cref="CommandNode"/> to inspect.</param>
        /// <returns>A read-only list of <see cref="IConditionInfo"/>s built from the provided <see cref="CommandNode"/>.</returns>
        internal IReadOnlyList<IConditionInfo> GetConditionInfo(CommandNode node)
        {
            var conditionInfos = new List<IConditionInfo>();

            foreach (var attribute in node.CommandMethod.GetCustomAttributes<ConditionAttribute>())
            {
                var type = attribute.GetType();

                conditionInfos.Add(new ConditionInfo(type.Name, type.GetDescriptionOrDefault()));
            }

            return conditionInfos;
        }

        /// <summary>
        /// Gets a fully qualified, plus-delimited name of a group, starting with the name of the highest parent in the command tree and ending with the name of this node.
        /// </summary>
        /// <param name="groupNode">The node to walk with.</param>
        /// <returns>A fully qualified, plus-delimited name of a group.</returns>
        internal string WalkNamespace(GroupNode groupNode)
        {
            List<string> names = new();

            names.Add(groupNode.Key);

            var node = groupNode.Parent;
            while (node is GroupNode group)
            {
                names.Add(group.Key);
                node = group.Parent;
            }

            return string.Join('+', names.ToArray().Reverse());
        }
    }
}
