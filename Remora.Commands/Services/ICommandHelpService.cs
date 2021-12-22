//
//  ICommandHelpService.cs
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

using Remora.Commands.CommandInformation;
using Remora.Commands.Groups;
using Remora.Results;

namespace Remora.Commands.Services
{
    /// <summary>
    /// A service which handles retrieving informational classes for building command help.
    /// </summary>
    public interface ICommandHelpService
    {
        /// <summary>
        /// Gets information about the specified command.
        /// </summary>
        /// <param name="commandPath">A case-insensitive, period-delimited path to the command. E.g.: <i>config.groups.create</i>.</param>
        /// <returns>A <see cref="CommandInfo"/> representing the retrieved command.</returns>
        Result<ICommandInfo> GetCommandInfo(string commandPath);

        /// <summary>
        /// Gets information about the specified group.
        /// </summary>
        /// <param name="groupPath">A case-insensitive, period-delimited path to the group. E.g.: <i>config.groups</i>.</param>
        /// <param name="buildChildGroups">If true, child nodes will be created and populated. If false, the <see cref="IGroupInfo.ChildGroups"/> collection will be empty.</param>
        /// <returns>A <see cref="GroupInfo"/> representing the retrieved group.</returns>
        Result<IGroupInfo> GetGroupInfo(string groupPath, bool buildChildGroups = false);

        /// <inheritdoc cref="GetGroupInfo(string, bool)"/>
        /// <typeparam name="TCommandGroup">The type of the command group to gather information on.</typeparam>
        Result<IGroupInfo> GetGroupInfo<TCommandGroup>(bool buildChildGroups = false)
            where TCommandGroup : CommandGroup;

        /// <summary>
        /// Gets information about all registered command groups and their commands.
        /// </summary>
        /// <returns>A read-only list of <see cref="GroupInfo"/>s representing the retrieved groups.</returns>
        Result<IRootInfo> GetAllCommands();
    }
}
