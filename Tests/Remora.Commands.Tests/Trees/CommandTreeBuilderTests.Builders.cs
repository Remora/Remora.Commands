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
using Remora.Commands.Trees;
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
            Assert.Equal(tree.Root.Children.Count, 1);
            Assert.Equal(command!.Key, "command");
        }
    }
}
