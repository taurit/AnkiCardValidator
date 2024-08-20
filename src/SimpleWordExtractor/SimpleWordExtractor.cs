﻿using CoreLibrary.Interfaces;

namespace SimpleWordExtractor;

/// <summary>
/// An oversimplified implementation of the IExtractWords interface.
/// It's provided to demonstrate the interface and eventual use in tests, not for production use.
/// </summary>
public class SimpleWordExtractor : IExtractWords
{
    /// <summary>
    /// Key is a word, value is a list of sentences where the word is found.
    /// </summary>
    readonly Dictionary<string, List<string>> _wordsAndSentences = new();

    public async Task<List<ExtractedWord>> ExtractWords(string inputFileName)
    {
        var inputFileContent = await File.ReadAllTextAsync(inputFileName);

        var sentences = inputFileContent.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());

        foreach (var sentence in sentences)
        {
            var words = sentence.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
                AddWordToDictionary(word, sentence);
        }

        return _wordsAndSentences.Select(x => new ExtractedWord(x.Key, x.Value)).ToList();
    }

    private void AddWordToDictionary(string word, string parentSentence)
    {
        if (!_wordsAndSentences.ContainsKey(word))
            _wordsAndSentences.Add(word, []);

        _wordsAndSentences[word].Add(parentSentence);
    }
}