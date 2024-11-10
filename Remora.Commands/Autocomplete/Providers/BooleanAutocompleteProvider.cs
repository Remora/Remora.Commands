//
//  BooleanAutocompleteProvider.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FuzzySharp;
using JetBrains.Annotations;
using Remora.Commands.Autocomplete.Factories;
using Remora.Commands.Localization;

namespace Remora.Commands.Autocomplete.Providers;

/// <summary>
/// Provides an autocomplete provider for the <see cref="bool"/> type.
/// </summary>
/// <param name="localizationProvider">A localization provider for translating boolean values into other languages.</param>
[PublicAPI]
public class BooleanAutocompleteProvider
(
    ILocalizationProvider localizationProvider,
    ICommandOptionChoiceFactory<bool> factory
) : IAutocompleteProvider<bool>
{
    /// <inheritdoc />
    public ValueTask<IReadOnlyList<TCommandOptionChoice>> GetSuggestionsAsync<TCommandOptionChoice, TCommandDataOption>
    (
        IReadOnlyList<TCommandDataOption> options,
        string userInput,
        CancellationToken ct = default
    )
        where TCommandOptionChoice : class, ICommandOptionChoice<bool>
        where TCommandDataOption : class, ICommandDataOption<bool>
    {
        List<TCommandOptionChoice> values =
        [
            CreateCommandOptionChoice<TCommandOptionChoice>(true),
            CreateCommandOptionChoice<TCommandOptionChoice>(false)
        ];

        return new ValueTask<IReadOnlyList<TCommandOptionChoice>>
        (
            values.OrderByDescending(value => Fuzz.Ratio(userInput, value.Name))
                .ToList()
        );
    }

    private TCommandOptionChoice CreateCommandOptionChoice<TCommandOptionChoice>(bool value)
        where TCommandOptionChoice : class, ICommandOptionChoice<bool>
        => factory.Create<TCommandOptionChoice>
        (
            value.ToString(),
            value,
            localizationProvider.GetTranslations(value.ToString())
        );
}
