﻿namespace AnkiCardValidator.Utilities;

public static class DuplicateDetector
{
    private static readonly DuplicateDetectionEqualityComparer DuplicateDetectionEqualityComparer = new DuplicateDetectionEqualityComparer();

    /// <summary>
    /// Heuristically detects possible duplicates in a list of notes.
    /// </summary>
    /// <returns>
    /// Notes with suspiciously similar content of the *front* side.
    /// </returns>
    public static List<AnkiNote> DetectDuplicatesFront(AnkiNote flashcard, List<AnkiNote> allNotes)
    {
        return allNotes
            .Where(note => note != flashcard && DuplicateDetector.DuplicateDetectionEqualityComparer.Equals(note.FrontSide, flashcard.FrontSide))
            .ToList();
    }

    public static List<AnkiNote> DetectDuplicatesBack(AnkiNote flashcard, List<AnkiNote> allNotes)
    {
        return allNotes
            .Where(note => note != flashcard && DuplicateDetector.DuplicateDetectionEqualityComparer.Equals(note.BackSide, flashcard.BackSide))
            .ToList();
    }
}

public class DuplicateDetectionEqualityComparer : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y)
    {
        if (x is null || y is null)
        {
            return false;
        }

        var xNormalized = ConvertToComparableForm(x);
        var yNormalized = ConvertToComparableForm(y);
        return xNormalized == yNormalized;
    }

    public int GetHashCode(string obj)
    {
        return ConvertToComparableForm(obj).GetHashCode();
    }

    private static string ConvertToComparableForm(string input)
    {
        return input.ToLowerInvariant();
    }
}
