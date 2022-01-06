//
//  ICommandBranch.cs
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

using OneOf;

namespace Remora.Commands.CommandInformation
{
    /// <summary>
    /// Represents a branch within the command system.
    /// </summary>
    public interface ICommandBranch
    {
        /// <summary>
        /// Gets the command group which was found at the level searched.
        /// </summary>
        IGroupInfo GroupInfo { get; }

        /// <summary>
        /// Gets the command or all overloaded commands found at the level searched.
        /// </summary>
        OneOf<ICommandInfo, ICommandInfo[]> Commands { get; }
    }
}
