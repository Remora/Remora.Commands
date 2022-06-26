//
//  TimeSpanParser.cs
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

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Remora.Commands.Results;
using Remora.Results;

namespace Remora.Commands.Parsers;

/// <summary>
/// Parses <see cref="TimeSpan"/>s.
/// </summary>
[PublicAPI]
public class TimeSpanParser : AbstractTypeParser<TimeSpan>
{
    private static readonly Regex _pattern = new
    (
        "(?<Years>\\d+(?=y))|(?<Months>\\d+(?=mo))|(?<Weeks>\\d+(?=w))|(?<Days>\\d+(?=d))|(?<Hours>\\d+(?=h))|(?<Minutes>\\d+(?=m))|(?<Seconds>\\d+(?=s))",
        RegexOptions.Compiled
    );

    /// <inheritdoc />
    public override ValueTask<Result<TimeSpan>> TryParseAsync(string? value, CancellationToken ct = default)
    {
        if (value is null)
        {
            return new(new ParsingError<TimeSpan>(value));
        }

        value = value.Trim();

        if (TimeSpan.TryParse(value, DateTimeFormatInfo.InvariantInfo, out var parsedTimespan))
        {
            return new ValueTask<Result<TimeSpan>>(parsedTimespan);
        }

        var matches = _pattern.Matches(value);
        if (matches.Count == 0)
        {
            return new ValueTask<Result<TimeSpan>>(new ParsingError<TimeSpan>(value));
        }

        var timeSpan = TimeSpan.Zero;
        foreach (var match in matches.Cast<Match>())
        {
            var groups = match.Groups
                .Cast<Group>()
                .Where(g => g.Success)
                .Skip(1)
                .Select(g => (g.Name, g.Value));

            foreach (var (key, groupValue) in groups)
            {
                if (!double.TryParse(groupValue, out var parsedGroupValue))
                {
                    return new ValueTask<Result<TimeSpan>>(new ParsingError<TimeSpan>(value));
                }

                if (key is "Years" or "Months")
                {
                    if (!int.TryParse(groupValue, out var parsedIntegerValue))
                    {
                        return new ValueTask<Result<TimeSpan>>(new ParsingError<TimeSpan>(value));
                    }

                    switch (key)
                    {
                        case "Years":
                        {
                            var now = DateTimeOffset.UtcNow;
                            var then = now.AddYears(parsedIntegerValue);

                            timeSpan += then - now;
                            break;
                        }
                        case "Months":
                        {
                            var now = DateTimeOffset.UtcNow;
                            var then = now.AddMonths(parsedIntegerValue);

                            timeSpan += then - now;
                            break;
                        }
                    }
                }
                else
                {
                    timeSpan += key switch
                    {
                        "Weeks" => TimeSpan.FromDays(parsedGroupValue * 7),
                        "Days" => TimeSpan.FromDays(parsedGroupValue),
                        "Hours" => TimeSpan.FromHours(parsedGroupValue),
                        "Minutes" => TimeSpan.FromMinutes(parsedGroupValue),
                        "Seconds" => TimeSpan.FromSeconds(parsedGroupValue),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
            }
        }

        return new ValueTask<Result<TimeSpan>>(timeSpan);
    }
}
