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

using System;
using OneOf;
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
        /// Attempts to perform a search against the command tree using the provided command string.
        /// </summary>
        /// <param name="commandString">A search string, either terminating with a command group or the command node itself, but without any parameters.</param>
        /// <param name="tokenizerOptions">Tokenization options to determine how to parse the <paramref name="commandString"/>.</param>
        /// <param name="treeSearchOptions">Tree search options to determine how to perform the search.</param>
        /// <returns>One of <see cref="IGroupInfo"/>, <see cref="ICommandInfo"/>, or an immutable collection of <see cref="ICommandInfo"/>s where multiple overloads are present.</returns>
        /// <example>
        /// User executes: -help "options reload"
        /// Where "options reload" would normally be the command string passed in place of help.
        /// This would locate a "reload" command located in the "commands" module.
        /// </example>
        Result<OneOf<IGroupInfo, ICommandInfo, ICommandInfo[], ICommandBranch>> FindInfo(string commandString, Tokenization.TokenizerOptions? tokenizerOptions = null, Trees.TreeSearchOptions? treeSearchOptions = null);

        /// <summary>
        /// Gets information about the specified group.
        /// </summary>
        /// <param name="commandGroupType">The type of the command group to gather information on.</param>
        /// <param name="buildChildGroups">If true, child nodes will be created and populated. If false, the <see cref="IGroupInfo.ChildGroups"/> collection will be empty.</param>
        /// <returns>A <see cref="GroupInfo"/> representing the retrieved group.</returns>
        Result<IGroupInfo> GetGroupInfo(Type commandGroupType, bool buildChildGroups = false);

        /// <inheritdoc cref="GetGroupInfo(Type, bool)"/>
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
