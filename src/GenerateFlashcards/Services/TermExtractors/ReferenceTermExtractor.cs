﻿using CoreLibrary;

namespace GenerateFlashcards.Services.TermExtractors;

/// <summary>
/// An oversimplified implementation of the IExtractTerms interface.
/// 
/// It's provided to demonstrate the interface and for eventual use in tests, but not
/// for production use where there are more edge cases to handle.
/// </summary>
public class ReferenceTermExtractor : IExtractTerms
{
    public async Task<List<Note>> ExtractTerms(List<string> sentences, string contentInputLanguage)
    {
        var notes = new List<Note>();

        foreach (var sentence in sentences)
        {
            var words = sentence.Split([' ', '\n', '\r', '\t', ',', ';'], StringSplitOptions.RemoveEmptyEntries);
            notes.AddRange(words.Select(word => new Note(word, sentence, word, PartOfSpeech.Unknown, [])));
        }

        return notes;
    }
}