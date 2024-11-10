//
//  EnumAutocompleteProvider.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FuzzySharp;
using JetBrains.Annotations;
using Remora.Commands.Extensions;
using Remora.Commands.Localization;

namespace Remora.Commands.Autocomplete.Providers;

/// <summary>
/// Provides an autocomplete provider for <see cref="TEnum"/>.
/// </summary>
/// <typeparam name="TEnum">The type of the enum to provide suggestions for.</typeparam>
[PublicAPI]
public sealed class EnumAutocompleteProvider<TEnum>(ILocalizationProvider localizationProvider) : IAutocompleteProvider<TEnum>
    where TEnum : struct, Enum
{
    /// <inheritdoc />
    public ValueTask<IReadOnlyList<ICommandOptionChoice>> GetSuggestionsAsync<TCommandDataOption>
    (
        IReadOnlyList<TCommandDataOption> options,
        string userInput,
        CancellationToken ct = default
    )
        where TCommandDataOption : ICommandDataOption
    {
        var choices = EnumExtensions.GetEnumChoices<TEnum>
        (
            localizationProvider,
            (name, value) => new CommandOptionChoice(name, value)
        );
        return new ValueTask<IReadOnlyList<ICommandOptionChoice>>
        (
            choices.OrderByDescending(choice => Fuzz.Ratio(userInput, choice.Name))
                .Take(25)
                .ToList()
        );
    }

    // TEMP: Just an experiment
    private sealed class CommandOptionChoice(string name, string value) : ICommandOptionChoice
    {
        public string Name { get; } = name;

        public string Value { get; } = value;
    }
}
