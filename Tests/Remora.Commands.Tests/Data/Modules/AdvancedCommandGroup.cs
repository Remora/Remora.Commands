//
//  AdvancedCommandGroup.cs
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

using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Results;

#pragma warning disable CS1591, SA1600

namespace Remora.Commands.Tests.Data.Modules;

[Group("test")]
public class AdvancedCommandGroup : CommandGroup
{
    [Command("positional-and-named")]
    public Task<IResult> PositionalAndNamed(string first, [Option("another")] string second)
    {
        return Task.FromResult<IResult>(Result.FromSuccess());
    }

    [Command("named-and-positional")]
    public Task<IResult> NamedAndPositional([Option("first")] string first, string second)
    {
        return Task.FromResult<IResult>(Result.FromSuccess());
    }

    [Command("mixed")]
    public Task<IResult> Mixed
    (
        [Option("first")] string first,
        string second,
        [Option("third")] string third,
        [Switch("enable")] bool option = false
    )
    {
        return Task.FromResult<IResult>(Result.FromSuccess());
    }

    [Command("positional-greedy")]
    public Task<IResult> PositionalGreedy([Greedy] string greedy)
    {
        return Task.FromResult<IResult>(Result.FromSuccess());
    }

    [Command("positional-greedy-with-named-after")]
    public Task<IResult> PositionalGreedyWithNamedAfter([Greedy] string greedy, [Option("second")] string second)
    {
        return Task.FromResult<IResult>(Result.FromSuccess());
    }

    [Command("positional-greedy-with-named-before")]
    public Task<IResult> PositionalGreedyWithNamedBefore([Option("first")] string first, [Greedy] string greedy)
    {
        return Task.FromResult<IResult>(Result.FromSuccess());
    }

    [Command("positional-greedy-with-positional-before")]
    public Task<IResult> PositionalGreedyWithPositionalBefore(string first, [Greedy] string greedy)
    {
        return Task.FromResult<IResult>(Result.FromSuccess());
    }

    [Command("named-greedy")]
    public Task<IResult> NamedGreedy([Greedy, Option("greedy")] string greedy)
    {
        return Task.FromResult<IResult>(Result.FromSuccess());
    }

    [Command("named-greedy-with-named-after")]
    public Task<IResult> NamedGreedyWithNamedAfter([Greedy, Option("greedy")] string greedy, [Option("second")] string second)
    {
        return Task.FromResult<IResult>(Result.FromSuccess());
    }

    [Command("named-greedy-with-named-before")]
    public Task<IResult> NamedGreedyWithNamedBefore([Option("first")] string first, [Greedy, Option("greedy")] string greedy)
    {
        return Task.FromResult<IResult>(Result.FromSuccess());
    }

    [Command("named-greedy-with-positional-before")]
    public Task<IResult> NamedGreedyWithPositionalBefore(string first, [Greedy, Option("greedy")] string greedy)
    {
        return Task.FromResult<IResult>(Result.FromSuccess());
    }
}
