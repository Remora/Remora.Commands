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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FuzzySharp;
using Humanizer;
using JetBrains.Annotations;
using Remora.Commands.Attributes;
using Remora.Commands.Autocomplete.Factories;
using Remora.Commands.Localization;

namespace Remora.Commands.Autocomplete.Providers;

/// <summary>
/// Provides an autocomplete provider for <see cref="TEnum"/>.
/// </summary>
/// <typeparam name="TEnum">The type of the enum to provide suggestions for.</typeparam>
[PublicAPI]
public sealed class EnumAutocompleteProvider<TEnum>
(
    ILocalizationProvider localizationProvider,
    ICommandOptionChoiceFactory<TEnum> factory
) : IAutocompleteProvider<TEnum>
    where TEnum : unmanaged, Enum
{
    /// <inheritdoc />
    public ValueTask<IReadOnlyList<TCommandOptionChoice>> GetSuggestionsAsync<TCommandOptionChoice, TCommandDataOption>
    (
        IReadOnlyList<TCommandDataOption> options,
        string userInput,
        CancellationToken ct = default
    )
        where TCommandOptionChoice : class, ICommandOptionChoice<TEnum>
        where TCommandDataOption : class, ICommandDataOption<TEnum>
    {
        IReadOnlyList<TCommandOptionChoice> choices = EnumHelper<TCommandOptionChoice>.GetEnumChoices
        (
            localizationProvider,
            factory
        );
        return new ValueTask<IReadOnlyList<TCommandOptionChoice>>
        (
            choices.OrderByDescending(choice => Fuzz.Ratio(userInput, choice.Name))
                .Take(25)
                .ToList()
        );
    }

    /// <summary>
    /// Provides helper methods for enum types.
    /// </summary>
    private static class EnumHelper<TCommandOptionChoice>
        where TCommandOptionChoice : class, ICommandOptionChoice<TEnum>
    {
        private const int _maxChoiceNameLength = 100;
        private const int _maxChoiceValueLength = 100;

        private static readonly ConcurrentDictionary<Type, IReadOnlyList<TCommandOptionChoice>> _choiceCache = new();

        private sealed class OptionChoiceContext
        (
            ILocalizationProvider localizationProvider,
            ICommandOptionChoiceFactory<TEnum> factory
        )
        {
            public ILocalizationProvider LocalizationProvider { get; } = localizationProvider;

            public ICommandOptionChoiceFactory<TEnum> Factory { get; } = factory;
        }

        /// <summary>
        /// Gets the Discord choices that the given enumeration is composed of.
        /// </summary>
        /// <remarks>
        /// This method is relatively expensive on the first call, after which the results will be cached. This method is
        /// thread-safe.
        /// </remarks>
        /// <param name="localizationProvider">The localization provider.</param>
        /// <param name="commandOptionChoiceFactory">A factory for creating an instance of <see cref="TCommandOptionChoice"/>.</param>
        /// <returns>The choices.</returns>
        public static IReadOnlyList<TCommandOptionChoice> GetEnumChoices
        (
            ILocalizationProvider localizationProvider,
            ICommandOptionChoiceFactory<TEnum> commandOptionChoiceFactory
        )
        {
            Type enumType = typeof(TEnum);
            return _choiceCache.GetOrAdd
            (
                enumType,
                CreateOptions,
                new OptionChoiceContext(localizationProvider, commandOptionChoiceFactory)
            );
        }

        private static IReadOnlyList<TCommandOptionChoice> CreateOptions(Type enumType, OptionChoiceContext context)
        {
            TEnum[] values = GetEnumValues();
            List<TCommandOptionChoice> choices = [];

            foreach (TEnum value in values)
            {
                string enumName = Enum.GetName(enumType, value) ?? throw new InvalidOperationException();
                MemberInfo member = enumType.GetMember(enumName).Single();

                if (member.GetCustomAttribute<ExcludeFromChoicesAttribute>() is not null)
                {
                    continue;
                }

                string displayString = GetDisplayString(enumName, member);
                IReadOnlyDictionary<CultureInfo, string> localizedDisplayNames =
                    context.LocalizationProvider.GetTranslations(displayString);
                foreach ((CultureInfo locale, string localizedDisplayName) in localizedDisplayNames)
                {
                    if (localizedDisplayName.Length > _maxChoiceNameLength)
                    {
                        throw new ArgumentOutOfRangeException
                        (
                            nameof(enumType),
                            $"The localized display name for the locale {locale} of the enumeration member {enumType.Name}::{enumName} is too long (max {_maxChoiceNameLength})."
                        );
                    }
                }

                if (enumName.Length <= _maxChoiceValueLength)
                {
                    choices.Add
                    (
                        context.Factory.Create<TCommandOptionChoice>
                        (
                            displayString,
                            value,
                            localizedDisplayNames.Count > 0 ? localizedDisplayNames : default
                        )
                    );
                }
            }
            return choices;
        }

        private static string GetDisplayString(string enumName, MemberInfo enumMember)
        {
            var descriptionAttribute = enumMember.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute is not null)
            {
                return descriptionAttribute.Description;
            }

            var displayAttribute = enumMember.GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.Description
                   ?? displayAttribute?.Name
                   ?? enumName.Humanize().Transform(To.TitleCase);
        }

        private static TEnum[] GetEnumValues()
#if NET6_0_OR_GREATER
        => Enum.GetValues<TEnum>();
#else
        => (TEnum[])Enum.GetValues(typeof(TEnum));
#endif
    }
}
