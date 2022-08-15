//
//  CommandTreeBuilderTests.Descriptions.cs
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

using System.Linq;
using Remora.Commands.Tests.Data.DummyModules;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Xunit;

namespace Remora.Commands.Tests.Trees;

public static partial class CommandTreeBuilderTests
{
    /// <summary>
    /// Tests description functionality.
    /// </summary>
    public class Descriptions
    {
        /// <summary>
        /// Tests that descriptions are correctly included.
        /// </summary>
        [Fact]
        public void IncludesConfiguredDescriptions()
        {
            var builder = new CommandTreeBuilder();
            builder.RegisterModule<DescribedGroup>();

            var tree = builder.Build();
            var root = tree.Root;

            var group = (GroupNode)root.Children.Single();
            var command = (CommandNode)group.Children.Single();
            var parameter = command.Shape.Parameters.Single();

            Assert.Equal("Group description", group.Description);
            Assert.Equal("Command description", command.Shape.Description);
            Assert.Equal("Parameter description", parameter.Description);
        }
    }
}
