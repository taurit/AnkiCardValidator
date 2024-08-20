﻿using CoreLibrary.Interfaces;
using GenerateFlashcards.Commands;
using ReferenceImplementations;

namespace GenerateFlashcards.Services;

/// <summary>
/// Provides instances of building blocks like:
/// - <see cref="IExtractSentences"/>
/// - <see cref="IExtendNotes"/>
/// - <see cref="IGenerateOutput"/>
///
/// ... that best fit user-provided parameters (like input and output languages).
/// 
/// Some languages and alphabets benefit from having specialized implementations of these interfaces,
/// while others might be fine using the most generic one.
/// </summary>
internal class BuildingBlocksProvider(
    ReferenceSentenceExtractor referenceWordExtractor,
    ReferenceTermExtractor referenceTermExtractor
)
{

    internal IExtractSentences SelectBestSentenceExtractor(GenerateFlashcardsCommandSettings settings)
    {
        return referenceWordExtractor;
    }

    public IExtractTerms SelectBestTermExtractor(GenerateFlashcardsCommandSettings settings)
    {
        return referenceTermExtractor;
    }
}
