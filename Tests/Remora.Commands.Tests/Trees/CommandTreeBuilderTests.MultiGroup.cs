//
//  CommandTreeBuilderTests.MultiGroup.cs
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

using Remora.Commands.Tests.Data.DummyModules;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Xunit;

namespace Remora.Commands.Tests.Trees;

public static partial class CommandTreeBuilderTests
{
    /// <summary>
    /// Tests parsing of command groups, split across multiple modules.
    /// </summary>
    public class MultiModule
    {
        /// <summary>
        /// Tests whether a <see cref="NamedGroupWithCommands"/> can be correctly parsed into a tree.
        /// </summary>
        [Fact]
        public void ParsesGroupsWithSameNameCorrectly()
        {
            var builder = new CommandTreeBuilder();
            builder.RegisterModule<NamedGroupWithCommands>();
            builder.RegisterModule<NamedGroupWithAdditionalCommands>();

            var tree = builder.Build();
            var root = tree.Root;

            Assert.Single(root.Children);
            var groupNode = Assert.IsType<GroupNode>(root.Children[0]);

            Assert.Equal("a", groupNode.Key);

            Assert.Equal(2, groupNode.GroupTypes.Count);
            Assert.Equal(typeof(NamedGroupWithCommands), groupNode.GroupTypes[0]);
            Assert.Equal(typeof(NamedGroupWithAdditionalCommands), groupNode.GroupTypes[1]);

            Assert.Equal(6, groupNode.Children.Count);
            var command1 = Assert.IsType<CommandNode>(groupNode.Children[0]);
            var command2 = Assert.IsType<CommandNode>(groupNode.Children[1]);
            var command3 = Assert.IsType<CommandNode>(groupNode.Children[2]);
            var command4 = Assert.IsType<CommandNode>(groupNode.Children[3]);
            var command5 = Assert.IsType<CommandNode>(groupNode.Children[4]);
            var command6 = Assert.IsType<CommandNode>(groupNode.Children[5]);

            Assert.Equal("b", command1.Key);
            Assert.Equal("c", command2.Key);
            Assert.Equal("d", command3.Key);
            Assert.Equal("e", command4.Key);
            Assert.Equal("f", command5.Key);
            Assert.Equal("g", command6.Key);
        }
    }
}
