//
//  ReturnTypeCommandGroup.cs
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

using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Results;

#pragma warning disable CS1591, SA1600

namespace Remora.Commands.Tests.Data.Modules;

public class ReturnTypeCommandGroup : CommandGroup
{
    [Command("a")]
    public Task<IResult> A()
    {
        return Task.FromResult<IResult>(Result.FromSuccess());
    }

    [Command("b")]
    public ValueTask<IResult> B()
    {
        return new ValueTask<IResult>(Result.FromSuccess());
    }

    [Command("c")]
    public Task<Result> C()
    {
        return Task.FromResult(Result.FromSuccess());
    }

    [Command("d")]
    public ValueTask<Result> D()
    {
        return new ValueTask<Result>(Result.FromSuccess());
    }

    [Command("e")]
    public Task<Result<string>> E()
    {
        return Task.FromResult(Result<string>.FromSuccess("success"));
    }

    [Command("f")]
    public ValueTask<Result<string>> F()
    {
        return new ValueTask<Result<string>>("success");
    }
}
