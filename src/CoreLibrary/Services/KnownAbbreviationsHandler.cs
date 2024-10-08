namespace CoreLibrary.Services;

public static class KnownAbbreviationsHandler
{
    private class Replacement(string withDot, string withFullWidthDot)
    {
        public readonly string WithDot = withDot;
        public readonly string WithFullWidthDot = withFullWidthDot;

        public readonly string SpaceWithDot = $" {withDot}";
        public readonly string SpaceWithFullWidthDot = $" {withFullWidthDot}";
        public readonly string NewlineWithDot = $"\n{withDot}";
        public readonly string NewlineWithFullWidthDot = $"\n{withFullWidthDot}";
    }

    private static readonly string[] KnownAbbreviationsWithDot =
    [
        "Mr.",
        "див.",
        "ст.",
        "н.е.",
        "тис.",
        "грн.",
        //"до.",
        "т.д.",
        "обл.",
        "кв.",
        "с.г.",
        "млн.",
        "дн.",
        "р.",
        "просп.",
        "вул.",
        "буд.",
        "км.",
        "рр.",
        "зб.",
        "кн.",
        "Np.",
        "Dr.",
        "Prof.",
        "Nr.",
        "Tzn.",
        "Gł.",
        "Ul.",
        "Mec.",
        "Ks.",
        "Pl.",
        "Ogł.",
        "Max.",
        "Min.",
        "C.k.",
        "św.",
        "Płk.",
        "Śp.",
        "Tj.",
        "Br.",
        "Poz.",
        "Dr.",
        "Prof.",
        "Mrs.",
        "Ms.",
        "Jr.",
        "Sr.",
        "Inc.",
        "Co.",
        "St.",
        "Ave.",
        "U.S.",
        "U.K.",
        "E.g.",
        "I.e."
    ];

    private static readonly Replacement[] Replacements = KnownAbbreviationsWithDot
        .Select(x => new Replacement(x, x.Replace('.', '．')))
        .ToArray();

    private static string ReplaceFirstOccurrence(string source, string oldValue, string newValue)
    {
        int index = source.IndexOf(oldValue);

        if (index >= 0) // Check if oldValue was found
        {
            return source.Remove(index, oldValue.Length).Insert(index, newValue);
        }

        return source;
    }

    public static string ReplaceDotWithFullWidthDotInAbbreviations(string bookContent)
    {
        foreach (var replacement in Replacements)
        {
            bookContent = bookContent.Replace(replacement.SpaceWithDot, replacement.SpaceWithFullWidthDot);
            bookContent = bookContent.Replace(replacement.NewlineWithDot, replacement.NewlineWithFullWidthDot);

            // https://stackoverflow.com/questions/717855/why-is-function-isprefix-faster-than-startswith-in-c
            // hack: use StringComparison.Ordinal to avoid slow getting of CurrentCulture with each call (big performance issue)
            if (bookContent.StartsWith(replacement.WithDot, StringComparison.Ordinal))
            {
                bookContent = ReplaceFirstOccurrence(bookContent, replacement.WithDot, replacement.WithFullWidthDot);
            }

        }
        return bookContent;
    }

    public static string ReplaceFullWidthDotWithDotInAbbreviations(string bookContent)
    {
        foreach (var replacement in Replacements)
        {
            bookContent = bookContent.Replace(replacement.WithFullWidthDot, replacement.WithDot);
        }
        return bookContent;
    }
}
