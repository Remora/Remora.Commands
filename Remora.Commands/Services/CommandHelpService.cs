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
        public Result<OneOf<IGroupInfo, ICommandInfo, ICommandInfo[], ICommandBranch>> FindInfo(string commandString, Tokenization.TokenizerOptions? tokenizerOptions = null, Trees.TreeSearchOptions? treeSearchOptions = null)
        {
            static OneOf<IGroupInfo, ICommandInfo, ICommandInfo[], ICommandBranch> FromGroupInfo(IGroupInfo groupInfo)
                => OneOf<IGroupInfo, ICommandInfo, ICommandInfo[], ICommandBranch>.FromT0(groupInfo);
            static OneOf<IGroupInfo, ICommandInfo, ICommandInfo[], ICommandBranch> FromCommandInfo(ICommandInfo commandInfo)
                => OneOf<IGroupInfo, ICommandInfo, ICommandInfo[], ICommandBranch>.FromT1(commandInfo);
            static OneOf<IGroupInfo, ICommandInfo, ICommandInfo[], ICommandBranch> FromCommandInfos(ICommandInfo[] commandInfos)
                => OneOf<IGroupInfo, ICommandInfo, ICommandInfo[], ICommandBranch>.FromT2(commandInfos);
            static OneOf<IGroupInfo, ICommandInfo, ICommandInfo[], ICommandBranch> FromCommandBranch(ICommandBranch commandBranch)
                => OneOf<IGroupInfo, ICommandInfo, ICommandInfo[], ICommandBranch>.FromT3(commandBranch);
            static Result<OneOf<IGroupInfo, ICommandInfo, ICommandInfo[], ICommandBranch>> FromError<TEntity>(Result<TEntity> result)
                => Result<OneOf<IGroupInfo, ICommandInfo, ICommandInfo[], ICommandBranch>>.FromError(result);

            tokenizerOptions ??= new Tokenization.TokenizerOptions();
            treeSearchOptions ??= new Trees.TreeSearchOptions();

            if (string.IsNullOrWhiteSpace(commandString))
            {
                return new ArgumentNullError(nameof(commandString), "Command String cannot be null, empty, or comprised entirely of whitespace.");
            }

            var splitOptions = tokenizerOptions.IgnoreEmptyValues
                ? StringSplitOptions.RemoveEmptyEntries
                : StringSplitOptions.None;

            var parts = commandString.Split(tokenizerOptions.Delimiter, splitOptions);

            // Walk the tree
            IParentNode parentNode = _commandService.Tree.Root;
            List<IChildNode> childNodes = new();
            var enumerable = parts.GetEnumerator();
            while (true)
            {
                var part = enumerable.Current as string;
                var foundNodes = parentNode.Children.Where(child => child.Key.Equals(part, treeSearchOptions.KeyComparison)).ToList();

                if (!foundNodes.Any())
                {
                    return new NotFoundError($"Cound not find any nodes matching the specified search options.");
                }

                // If we find exactly one result.
                // This will be the most common case.
                if (foundNodes.Count == 1)
                {
                    // Get the first node in the list.
                    var firstNode = foundNodes.First();

                    // Figure out if it's a group node.
                    if (firstNode is GroupNode group)
                    {
                        // We've found another group.
                        // Is this the last item in the group?
                        if (enumerable.MoveNext())
                        {
                            // It's not, so promote the parent node we found
                            // and continue to the next iteration.
                            parentNode = group;
                            continue;
                        }

                        // It's the last item in the group and the last item was a group.
                        // Return the group.
                        var getResult = GetGroupInfo(group);
                        if (getResult.IsDefined(out var groupInfo))
                        {
                            return FromGroupInfo(groupInfo);
                        }
                        else
                        {
                            return FromError(getResult);
                        }
                    }
                    else if (firstNode is CommandNode commandNode)
                    {
                        // We only found a single result, so there are no command overloads to worry about.
                        // Find out if this is the last item in the gorup.
                        if (enumerable.MoveNext())
                        {
                            // It's not, which means we terminated prematurely. Return an error.
                            return new NotFoundError($"Cound not find any nodes matching the specified search options.");
                        }

                        var getResult = GetCommandInfo(commandNode);
                        if (getResult.IsDefined(out var commandInfo))
                        {
                            return FromCommandInfo(commandInfo);
                        }
                        else
                        {
                            return FromError(getResult);
                        }
                    }
                }

                // More than one result was found.
                // If they're not all commands, then they're either all groups or they're mixed.
                if (!foundNodes.All(x => x is CommandNode))
                {
                    // We found multiple matches and at least one is a group node..
                    if (foundNodes.Any(x => x is GroupNode))
                    {
                        var firstNodes = foundNodes.Where(x => x is GroupNode);
                        if (firstNodes.Count() > 1)
                        {
                            // You cannot have multiple sibling group nodes with the same key.
                            return new InvalidOperationError($"Multiple sibling group nodes were found with the same key.");
                        }

                        // We have additional search terms.
                        // Promote the group and search its children.
                        var firstNode = firstNodes.First() as GroupNode;
                        if (enumerable.MoveNext())
                        {
                            parentNode = firstNode!;
                            continue;
                        }

                        // We do not have any additional search terms.
                        // Should we return the group info or the command info?
                        // TODO: Make determination
                        List<ICommandInfo> commandInfoList = new();
                        foreach (var node in foundNodes.OfType<CommandNode>())
                        {
                            var getResult = GetCommandInfo(node);
                            if (getResult.IsDefined(out var commandInfo))
                            {
                                commandInfoList.Add(commandInfo);
                            }
                        }

                        var groupInfoResult = GetGroupInfo(firstNode!);
                        if (groupInfoResult.IsDefined(out var groupInfo))
                        {
                            return FromCommandBranch(new CommandBranch(groupInfo, commandInfoList.ToArray()));
                        }

                        return FromCommandInfos(commandInfoList.ToArray());
                    }
                }

                // They are all commands.
                List<ICommandInfo> commandInfos = new();
                foreach (var node in foundNodes.Cast<CommandNode>())
                {
                    var getResult = GetCommandInfo(node);
                    if (getResult.IsDefined(out var commandInfo))
                    {
                        commandInfos.Add(commandInfo);
                    }
                }

                return FromCommandInfos(commandInfos.ToArray());
            }

            // return new InvalidOperationError("The search operation failed.");
        }

        /// <inheritdoc />
        public Result<IGroupInfo> GetGroupInfo(Type commandGroupType, bool buildChildGroups = true)
        {
            var keys = WalkKeys(commandGroupType);
            if (!keys.Any())
            {
                return new ArgumentInvalidError(nameof(commandGroupType), $"Provided type argument must be a kind of CommandGroup.");
            }

            // Walk the command tree.
            IParentNode parentNode = _commandService.Tree.Root;
            foreach (var key in keys)
            {
                var group = parentNode.Children.FirstOrDefault(x => x is IParentNode && x.Key.Equals(key));

                if (group is IParentNode parent)
                {
                    parentNode = parent;
                }

                return new NotFoundError($"Could not find any groups with the name '{key}' registered with the CommandService.");
            }

            // At this point, parentNode should contain the commandGroup we were searching for.
            var targetGroup = parentNode as GroupNode;
            if (!targetGroup!.Key.Equals(keys.Last()))
            {
                return new InvalidOperationError($"An error occurred while walking the command tree that caused the wrong group to be returned.");
            }

            return GetGroupInfo(targetGroup, buildChildGroups);
        }

        /// <inheritdoc />
        public Result<IGroupInfo> GetGroupInfo<TCommandGroup>(bool buildChildGroups = true)
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
                    continue;
                }

                if (child is GroupNode group)
                {
                    var infoResult = GetGroupInfo(group);
                    if (infoResult.IsDefined(out var groupInfo))
                    {
                        rootGroups.Add(groupInfo);
                    }
                    continue;
                }
            }

            return new RootInfo(rootGroups.AsReadOnly(), rootCommands.AsReadOnly());
        }

        /// <summary>
        /// Gets a <see cref="ICommandInfo"/> for the provided <see cref="CommandNode"/>.
        /// </summary>
        /// <param name="node">The <see cref="CommandNode"/> to inspect.</param>
        /// <returns>An <see cref="ICommandInfo"/> built from the provided node.</returns>
        internal static Result<ICommandInfo> GetCommandInfo(CommandNode node)
        {
            var name = node.Key;
            var description = node.Description;
            var aliases = node.Aliases;

            var conditionInfo = GetConditionInfo(node);
            var argumentInfo = node.CommandMethod.GetParameters();

            var commandInfo = new CommandInfo(name, description, aliases, false, null, conditionInfo, argumentInfo);

            var hiddenAttribute = node.CommandMethod.GetCustomAttribute<HiddenFromHelpAttribute>();
            if (hiddenAttribute is not null)
            {
                return commandInfo with
                {
                    Hidden = true,
                    HiddenFromHelpComment = hiddenAttribute.Comment
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
        internal Result<IGroupInfo> GetGroupInfo(GroupNode node, bool buildChildGroups = true)
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
                    continue;
                }

                if (child is CommandNode command)
                {
                    var infoResult = GetCommandInfo(command);
                    if (infoResult.IsDefined(out var commandInfo))
                    {
                        childCommands.Add(commandInfo);
                    }
                    continue;
                }
            }

            var info = new GroupInfo(name, description, aliases, false, null, childCommands.AsReadOnly(), childGroups.AsReadOnly());

            var hiddenAttribute = node.GetType().GetCustomAttribute<HiddenFromHelpAttribute>();
            if (hiddenAttribute is not null)
            {
                return info with
                {
                    Hidden = true,
                    HiddenFromHelpComment = hiddenAttribute.Comment
                };
            }

            return info;
        }

        /// <summary>
        /// Gets a read-only list of <see cref="IConditionInfo"/>s for the provided <see cref="CommandNode"/>.
        /// </summary>
        /// <param name="node">The <see cref="CommandNode"/> to inspect.</param>
        /// <returns>A read-only list of <see cref="IConditionInfo"/>s built from the provided <see cref="CommandNode"/>.</returns>
        internal static IReadOnlyList<IConditionInfo> GetConditionInfo(CommandNode node)
        {
            static IEnumerable<IConditionPropertyInfo> GetPropertyInfo(object instance)
            {
                foreach (var prop in instance.GetType().GetProperties())
                {
                    yield return new ConditionPropertyInfo(prop.Name, prop.CanRead, prop.CanWrite, prop.PropertyType, prop.GetValue(instance));
                }
            }

            var conditionInfos = new List<IConditionInfo>();

            foreach (var attribute in node.CommandMethod.GetCustomAttributes<ConditionAttribute>())
            {
                var type = attribute.GetType();

                var propInfo = GetPropertyInfo(attribute).ToArray();

                conditionInfos.Add(new ConditionInfo(type.Name, type.GetDescriptionOrDefault(), propInfo));
            }

            return conditionInfos;
        }

        /// <summary>
        /// Gets the keys of a group starting with the name of the highest parent in the command tree and ending with the name of this node.
        /// </summary>
        /// <param name="groupNode">The node to walk with.</param>
        /// <returns>An ordered collection of node keys terminating with the selected group.</returns>
        internal static IEnumerable<string> WalkKeys(GroupNode groupNode)
        {
            List<string> names = new();

            names.Add(groupNode.Key);

            var node = groupNode.Parent;
            while (node is GroupNode group)
            {
                names.Add(group.Key);
                node = group.Parent;
            }

            return names.AsEnumerable().Reverse();
        }

        /// <summary>
        /// Gets the keys of a group starting with the name of the highest parent in the command tree and ending with the name of this node.
        /// </summary>
        /// <param name="groupType">The node to walk with.</param>
        /// <returns>An ordered collection of node keys terminating with the selected group.</returns>
        internal static IEnumerable<string> WalkKeys(Type groupType)
        {
            List<string> names = new();

            Type node = groupType;
            while (true)
            {
                var groupAttribute = node.GetCustomAttribute<GroupAttribute>();

                if (groupAttribute is null)
                {
                    break;
                }

                names.Add(groupAttribute.Name);

                var declaringType = node.DeclaringType;
                if (declaringType is null)
                {
                    break;
                }
                node = declaringType;
            }

            return names.AsEnumerable().Reverse();
        }
    }
}
