Remora.Commands
===============

Remora.Commands is a platform-agnostic command library that handles parsing and
dispatch of typical *nix getopts-style command invocations - that is, 
Remora.Commands turns this:

```
!things add new-thing --description "My thing!" --enable-indexing
```

into a call to this:

```cs
[Group("things")]
public class ThingCommands : CommandGroup
{
    public Task<IResult> AddThing
    (
        string name, // = "new-thing"
        [Option("description") string description, // = "My thing!"
        [Switch("enable-indexing") bool enableIndexing = false // = true
    )
    {
        return _thingService.AddThing(name, description, enableIndexing);
    }
}
```

## Familiar Syntax
Inspired by and closely following *nix-style getopts syntax, Remora.Commands 
allows you to define commands in a variety of ways, leveraging familiar and 
widespread principles of command-line tooling. Currently, the library supports 
the following option syntax types:

  * Positional options
    - `T value`
  * Named options
    - `[Option('v')] T value` (short)
    - `[Option("value")] T value` (long)
    - `[Option('v', "value")] T value` (short and long)
  * Switches
    - `[Switch('e')] bool value = false` (short)
    - `[Switch("enable")] bool value = false` (long)
    - `[Switch('e', "enable")] bool value = false` (short and long)
  * Collections
    - `IEnumerable<T> values` (positional)
    - `[Option('v', "values")] IEnumerable<T> values` (named, as above)
    - `[Range(Min = 1, Max = 2)] IEnumerable<T> values` (constrained)
  * Verbs

Short names are specified using a single dash (`-`), and long options by two 
dashes (`--`). As an added bonus, you may combine short-name switches, similar
to what's supported by GNU `tar`. You can even place a normal named option at 
the end, followed by a value.

That is, both of the invocations below are valid (consider `x`, `v` and `z` as 
switches, and `f` as a named option).
```
my-command -xvz
my-command -xvf file.bin
```

The library also supports "greedy" options, which can simplify usage in certain
cases, allowing users to omit quotes. A greedy option treats multiple subsequent
values as one combined value, concatenating them automatically. Concatenation is
done with a single space in between each value. 

Making an option greedy is a simple matter of applying the `Greedy` attribute, 
and it can be combined with `Option` for both named and positional greedy 
parameters. Collections and switches, for which the greedy behaviour makes 
little sense, simply ignore the attribute.

```
[Greedy] T value
```

## Ease of use
It's dead easy to get started with Remora.Commands.

  1. Declare a command group. Groups may be nested to form verbs, or chains of 
    prefixes to a command.
        ```cs
        [Group("my-prefix")]
        public class MyCommands : CommandGroup
        {
        }
        ```
  2. Declare a command
        ```cs
        [Group("my-prefix")]
        public class MyCommands : CommandGroup
        {
            [Command("my-name")]
            public Task<IResult> MyCommand()
            {
                // ...
            }
        }
        ```
  3. Set up the command service with dependency injection
        ```cs
        var services = new ServiceCollection()
            .AddCommands()
            .AddCommandGroup<MyCommands>()
            .BuildServiceProvider();
        ```
  4. From any input source, parse and execute!
        ```cs
        private readonly CommandService _commandService;

        public async Task<IResult> MyInputHandler
        (
            string userInput, 
            CancellationToken ct
        )
        {
            var executionResult = await _commandService.TryExecuteAsync
            (
                userInput,
                ct
            );

            if (executionResult.IsSuccess)
            {
                return executionResult;
            }

            _logger.Error("Oh no!");
            _logger.Error("Anyway");
        }
        ```

## Flexibility
Command groups can be nested and combined in countless ways - registering 
multiple groups with the same name merges them under the same prefix, nameless
groups merge their commands with their outer level, and completely different
command group classes can share their prefixes unhindered.

For example, the structure below, when registered...

```cs
[Group("commands"]
public class MyFirstGroup : CommandGroup 
{
    [Command("do-thing")]
    public Task<IResult> MyCommand() { }
}

[Group("commands"]
public class MySecondGroup : CommandGroup 
{
    [Group("subcommands")
    public class MyThirdGroup : CommandGroup
    {
        [Command("do-thing")]
        public Task<IResult> MyCommand() { }
    }
}
```

produces the following set of available commands:

```
commands do-thing
commands subcommands do-thing
```

Commands themselves can be overloaded using normal C# syntax, and the various
argument syntax variants (that is, positional, named, switches, and collections)
can easily be mixed and matched.

Even the types recognized and parsed by Remora.Commands can be extended using 
`AbstractTypeParser<TType>` - if you can turn a string into an instance of your
type, Remora.Commands can parse it.

```cs
public class MyParser : AbstractTypeParser<MyType>
{
    public override ValueTask<RetrieveEntityResult<MyType>> TryParse
    (
        string value, 
        CancellationToken ct
    )
    {
        return new ValueTask<RetrieveEntityResult<MyType>>
        (
            !MyType.TryParse(value, out var result)
            ? RetrieveEntityResult<short>.FromError
              (
                  $"Failed to parse \"{value}\" as an instance of MyType."
              )
            : RetrieveEntityResult<short>.FromSuccess(result)
        );
    }
}
```

```cs
var services = new ServiceCollection()
    .AddCommands()
    .AddCommandGroup<MyCommands>()
    .AddSingletonParser<MyParser>()
    .BuildServiceProvider();
```

And, since parsers are instantiated with dependency injection, you can even
create parsers that fetch entities from a database, that look things up online,
that integrate with the rest of your application seamlessly... the possibilities
are endless!

By default, Remora.Commands provides builtin parsers for the following types:
  * `string`
  * `char`
  * `bool`
  * `byte`
  * `sbyte`
  * `ushort`
  * `short`
  * `uint`
  * `int`
  * `ulong`
  * `long`
  * `float`
  * `double`
  * `decimal`
  * `BigInteger`
  * `DateTimeOffset`

## Installation
Get it on [NuGet][1]!

## Thanks
Heavily inspired by [CommandLineParser][2], a great library for parsing *nix
getopts-style arguments from the command line itself.

Icon by [Twemoji][3], licensed under CC-BY 4.0.

[1]: http://nuget.org/packages/Remora.Commands
[2]: https://github.com/commandlineparser/commandline
[3]: https://twemoji.twitter.com/
