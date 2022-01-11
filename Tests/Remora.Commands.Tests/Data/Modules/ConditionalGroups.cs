//
//  ConditionalGroups.cs
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
using Remora.Commands.Tests.Data.Attributes;
using Remora.Results;

#pragma warning disable CS1591, SA1600

namespace Remora.Commands.Tests.Data.Modules;

public static class ConditionalGroups
{
    [GroupCondition("booga")]
    public class UnnamedGroupWithSuccessfulCondition : CommandGroup
    {
        [Command("a")]
        public Task<IResult> A()
        {
            return Task.FromResult<IResult>(Result.FromSuccess());
        }
    }

    [Group("a")]
    [GroupCondition("booga")]
    public class NamedGroupWithSuccessfulCondition : CommandGroup
    {
        [Command("b")]
        public Task<IResult> B()
        {
            return Task.FromResult<IResult>(Result.FromSuccess());
        }
    }

    [GroupCondition("wooga")]
    public class UnnamedGroupWithUnsuccessfulCondition : CommandGroup
    {
        [Command("a")]
        public Task<IResult> A()
        {
            return Task.FromResult<IResult>(Result.FromSuccess());
        }
    }

    [Group("a")]
    [GroupCondition("wooga")]
    public class NamedGroupWithUnsuccessfulCondition : CommandGroup
    {
        [Command("b")]
        public Task<IResult> B()
        {
            return Task.FromResult<IResult>(Result.FromSuccess());
        }
    }

    public class UnnamedGroupWithSuccessfulCommandCondition : CommandGroup
    {
        [Command("a")]
        [CommandCondition("booga")]
        public Task<IResult> A()
        {
            return Task.FromResult<IResult>(Result.FromSuccess());
        }
    }

    [Group("a")]
    public class NamedGroupWithSuccessfulCommandCondition : CommandGroup
    {
        [Command("b")]
        [CommandCondition("booga")]
        public Task<IResult> B()
        {
            return Task.FromResult<IResult>(Result.FromSuccess());
        }
    }

    public class UnnamedGroupWithUnsuccessfulCommandCondition : CommandGroup
    {
        [Command("a")]
        [CommandCondition("wooga")]
        public Task<IResult> A()
        {
            return Task.FromResult<IResult>(Result.FromSuccess());
        }
    }

    [Group("a")]
    public class NamedGroupWithUnsuccessfulCommandCondition : CommandGroup
    {
        [Command("b")]
        [CommandCondition("wooga")]
        public Task<IResult> B()
        {
            return Task.FromResult<IResult>(Result.FromSuccess());
        }
    }

    public class UnnamedGroupWithSuccessfulParameterCondition : CommandGroup
    {
        [Command("a")]
        public Task<IResult> A([ParameterCondition("booga")] string value)
        {
            return Task.FromResult<IResult>(Result.FromSuccess());
        }
    }

    [Group("a")]
    public class NamedGroupWithSuccessfulParameterCondition : CommandGroup
    {
        [Command("b")]
        public Task<IResult> B([ParameterCondition("booga")] string value)
        {
            return Task.FromResult<IResult>(Result.FromSuccess());
        }
    }

    public class UnnamedGroupWithUnsuccessfulParameterCondition : CommandGroup
    {
        [Command("a")]
        public Task<IResult> A([ParameterCondition("wooga")] string value)
        {
            return Task.FromResult<IResult>(Result.FromSuccess());
        }
    }

    [Group("a")]
    public class NamedGroupWithUnsuccessfulParameterCondition : CommandGroup
    {
        [Command("b")]
        public Task<IResult> B([ParameterCondition("wooga")] string value)
        {
            return Task.FromResult<IResult>(Result.FromSuccess());
        }
    }

    [Group("a")]
    [GroupCondition("booga")]
    public class NamedGroupWithSuccessfulConditionAndInnerUnnamedGroup : CommandGroup
    {
        public class Inner : CommandGroup
        {
            [Command("b")]
            public Task<IResult> B()
            {
                return Task.FromResult<IResult>(Result.FromSuccess());
            }
        }
    }

    [Group("a")]
    [GroupCondition("booga")]
    public class NamedGroupWithSuccessfulConditionAndInnerNamedGroup : CommandGroup
    {
        [Group("b")]
        public class Inner : CommandGroup
        {
            [Command("c")]
            public Task<IResult> C()
            {
                return Task.FromResult<IResult>(Result.FromSuccess());
            }
        }
    }

    [GroupCondition("booga")]
    public class UnnamedGroupWithSuccessfulConditionAndInnerUnnamedGroup : CommandGroup
    {
        public class Inner : CommandGroup
        {
            [Command("a")]
            public Task<IResult> A()
            {
                return Task.FromResult<IResult>(Result.FromSuccess());
            }
        }
    }

    [GroupCondition("booga")]
    public class UnnamedGroupWithSuccessfulConditionAndInnerNamedGroup : CommandGroup
    {
        [Group("a")]
        public class Inner : CommandGroup
        {
            [Command("b")]
            public Task<IResult> B()
            {
                return Task.FromResult<IResult>(Result.FromSuccess());
            }
        }
    }

    [Group("a")]
    [GroupCondition("wooga")]
    public class NamedGroupWithUnsuccessfulConditionAndInnerUnnamedGroup : CommandGroup
    {
        public class Inner : CommandGroup
        {
            [Command("b")]
            public Task<IResult> B()
            {
                return Task.FromResult<IResult>(Result.FromSuccess());
            }
        }
    }

    [Group("a")]
    [GroupCondition("wooga")]
    public class NamedGroupWithUnsuccessfulConditionAndInnerNamedGroup : CommandGroup
    {
        [Group("b")]
        public class Inner : CommandGroup
        {
            [Command("c")]
            public Task<IResult> C()
            {
                return Task.FromResult<IResult>(Result.FromSuccess());
            }
        }
    }

    [GroupCondition("wooga")]
    public class UnnamedGroupWithUnsuccessfulConditionAndInnerUnnamedGroup : CommandGroup
    {
        public class Inner : CommandGroup
        {
            [Command("a")]
            public Task<IResult> A()
            {
                return Task.FromResult<IResult>(Result.FromSuccess());
            }
        }
    }

    [GroupCondition("wooga")]
    public class UnnamedGroupWithUnsuccessfulConditionAndInnerNamedGroup : CommandGroup
    {
        [Group("a")]
        public class Inner : CommandGroup
        {
            [Command("b")]
            public Task<IResult> B()
            {
                return Task.FromResult<IResult>(Result.FromSuccess());
            }
        }
    }
}
