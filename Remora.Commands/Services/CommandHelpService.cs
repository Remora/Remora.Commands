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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Remora.Commands.Attributes;
using Remora.Commands.CommandInformation;
using Remora.Commands.Conditions;
using Remora.Commands.Extensions;
using Remora.Commands.Groups;
using Remora.Commands.Results;
using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Remora.Commands.Services
{
    /// <inheritdoc />
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
        public Result<ICommandInfo> GetCommandInfo(string commandPath)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Result<IGroupInfo> GetGroupInfo(string groupPath, bool buildChildGroups = false)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Result<IGroupInfo> GetGroupInfo<TCommandGroup>(bool buildChildGroups = false)
            where TCommandGroup : CommandGroup
        {
            var groups = _commandService.Tree.Root.Children.Where(x => x is GroupNode && x.Key == typeof(TCommandGroup).Name).Cast<GroupNode>();

            if (!groups.Any())
            {
                return new NotFoundError($"Could not find any groups of type '{typeof(TCommandGroup).Name}' registered with the CommandService.");
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
                var fullPath = typeof(TCommandGroup).FullName;

                var group = groups.FirstOrDefault(group => fullPath.EndsWith(WalkNamespace(group)));

                if (group is null)
                {
                    return new NotFoundError($"Could not find the requested group of type '{typeof(TCommandGroup).Name}' registered with the CommandService.");
                }

                var infoResult = GetGroupInfo(group, buildChildGroups);

                return infoResult;
            }
        }

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

        private Result<ICommandInfo> GetCommandInfo(CommandNode node)
        {
            var name = node.Key;
            var description = node.Description;
            var aliases = node.Aliases;

            var hiddenAttribute = node.CommandMethod.GetCustomAttribute<HiddenFromHelpAttribute>();

            var conditionInfo = GetConditionInfo(node);
            var argumentInfo = GetCommandArgumentInfo(node);

            var commandInfo = new CommandInfo(name, description, aliases, false, null, conditionInfo, argumentInfo);

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

        private Result<IGroupInfo> GetGroupInfo(GroupNode node, bool buildChildGroups = false)
        {
            var name = node.Key;
            var description = node.Description;
            var aliases = node.Aliases;

            var hiddenAttribute = node.GetType().GetCustomAttribute<HiddenFromHelpAttribute>();

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

        private IReadOnlyList<IConditionInfo> GetConditionInfo(CommandNode node)
        {
            throw new NotImplementedException();
        }

        private IReadOnlyList<ICommandArgumentInfo> GetCommandArgumentInfo(CommandNode node)
        {
            throw new NotImplementedException();
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
