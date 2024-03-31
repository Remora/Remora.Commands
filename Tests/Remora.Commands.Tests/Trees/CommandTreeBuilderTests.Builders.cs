//
//  CommandTreeBuilderTests.Builders.cs
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
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Commands.Services;
using Remora.Commands.Tests.Data.DummyModules;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Xunit;

namespace Remora.Commands.Tests.Trees;

public static partial class CommandTreeBuilderTests
{
    /// <summary>
    /// Tests builder-related functionality.
    /// </summary>
    public class Builders
    {
        /// <summary>
        /// Asserts that a command group built from a builder is registered in the tree correctly.
        /// </summary>
        [Fact]
        public void RegistersCommandGroupSuccessfully()
        {
            var services = new ServiceCollection().AddCommands();

            services.AddCommandTree().CreateCommand().WithName("command").WithInvocation((_, _, _) => default);

            var provider = services.BuildServiceProvider();
            var tree = provider.GetRequiredService<CommandTree>();

            var command = tree.Root.Children.FirstOrDefault();
            Assert.Single(tree.Root.Children);
            Assert.Equal("command", command!.Key);
        }

        /// <summary>
        /// Asserts that the builder merges a group into its constituent sibling correctly.
        /// </summary>
        [Fact]
        public void MergesSimpleGroupCorrectly()
        {
            var services = new ServiceCollection().AddCommands();

            services.AddCommandTree()
                    .WithCommandGroup<NamedGroupWithAdditionalCommands>()
                    .CreateCommandGroup().WithName("a").AddCommand().WithName("h").WithInvocation((_, _, _) => default);

            var provider = services.BuildServiceProvider();
            var tree = provider.GetRequiredService<CommandTree>();

            var group = tree.Root.Children.FirstOrDefault() as GroupNode;

            Assert.Single(tree.Root.Children);
            Assert.Equal("a", group!.Key);

            Assert.Equal(4, group.Children.Count);
        }
    }
}
