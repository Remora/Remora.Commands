//
//  DocumentedCommandGroup.cs
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

using System.ComponentModel;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Results;

#pragma warning disable CS1591, SA1600, CA1822

namespace Remora.Commands.Tests.Data.Modules
{
    [Group("test")]
    public class DocumentedCommandGroup
    {
        private static readonly Task<IResult> CompletedTask = Task.FromResult<IResult>(Result.FromSuccess());

        [Command("documented-parameterless")]
        [Description("A parameterless command for parameterless purposes.")]
        public Task<IResult> Parameterless()
        {
            return CompletedTask;
        }

        [Command("documented-single-parameter")]
        [Description("A command with a single documented paramter.")]
        public Task<IResult> SingleParameter([Description("A name. Probably of a person.")] string name)
        {
            return CompletedTask;
        }

        [Command("documented-optional-single-parameter")]
        [Description("A command with a single, optional, documented parameter.")]
        public Task<IResult> SingleOptionalParameter([Description("An age, but optional.")] int age = 18)
        {
            return CompletedTask;
        }
    }
}
