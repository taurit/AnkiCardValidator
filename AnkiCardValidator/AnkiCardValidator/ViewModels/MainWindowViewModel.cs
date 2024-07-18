﻿using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AnkiCardValidator.ViewModels;

[AddINotifyPropertyChangedInterface]
public class MainWindowViewModel
{
    public ObservableCollection<CardViewModel> Flashcards { get; set; } = [];
    public CardViewModel? SelectedCard { get; set; } = null;
}

[AddINotifyPropertyChangedInterface]
public record MeaningViewModel(string EnglishEquivalent, string Definition);

[AddINotifyPropertyChangedInterface]
[DebuggerDisplay("{Note}")]
public sealed class CardViewModel(
    // raw source data
    AnkiNote note,
    bool isReverseCard,

    // derived from source data locally
    FlashcardDirection noteDirection,

    List<AnkiNote> duplicatesQuestion,
    int? frequencyPositionQuestion,
    int? frequencyPositionAnswer,
    int numDefinitionsForQuestion,
    int numDefinitionsForAnswer,

    // derived from source data using ChatGPT
    CefrClassification cefrLevelQuestion,
    string? qualityIssues,
    string? rawResponseFromChatGptApi
)
{
    // reference to the evaluated note
    public AnkiNote Note { get; } = note;
    public bool IsReverseCard { get; } = isReverseCard;

    // quality signals calculated locally
    public FlashcardDirection NoteDirection { get; } = noteDirection;

    [DependsOn(nameof(NoteDirection))]
    public string CardDirectionFlag => (NoteDirection == FlashcardDirection.FrontTextInPolish && !IsReverseCard)
        ? "\ud83c\uddf5\ud83c\uddf1" // Polish flag emoji
        : "\ud83c\uddea\ud83c\uddf8" // Spanish flag emoji
    ;

    [DependsOn(nameof(IsReverseCard), nameof(Note))]
    public string Question => IsReverseCard ? Note.BackText : Note.FrontText;

    [DependsOn(nameof(IsReverseCard), nameof(Note))]
    public string Answer => IsReverseCard ? Note.FrontText : Note.BackText;

    public int? FrequencyPositionQuestion { get; } = frequencyPositionQuestion;
    public int? FrequencyPositionAnswer { get; } = frequencyPositionAnswer;

    public int NumDefinitionsForQuestion { get; } = numDefinitionsForQuestion;
    public int NumDefinitionsForAnswer { get; } = numDefinitionsForAnswer;

    public ObservableCollection<AnkiNote> DuplicatesOfQuestion { get; } = new(duplicatesQuestion);

    // data received from ChatGPT
    public string? RawResponseFromChatGptApi { get; set; } = rawResponseFromChatGptApi;

    public CefrClassification CefrLevelQuestion { get; set; } = cefrLevelQuestion;

    public string? QualityIssues { get; set; } =
        (qualityIssues is not null &&
         (
             qualityIssues.StartsWith("None") ||
             qualityIssues.StartsWith("Fine") ||
             qualityIssues.StartsWith("No issues") ||
             qualityIssues.StartsWith("No notable issues") ||
             qualityIssues.StartsWith("No significant issues") ||
             qualityIssues.StartsWith("All good") ||
             qualityIssues.StartsWith("No need for extra notes") ||
             qualityIssues.StartsWith("Duplicate flashcard") ||
             qualityIssues.StartsWith("Duplicate entry")
         )
        )
            ? null
            : qualityIssues;

    public ObservableCollection<Meaning> Meanings { get; init; } = [];

    // data derived from ChatGPT response
    [DependsOn(nameof(QualityIssues))] private bool HasQualityIssues => !String.IsNullOrWhiteSpace(QualityIssues);



    [DependsOn(nameof(CefrLevelQuestion), nameof(HasQualityIssues), nameof(Meanings), nameof(NumDefinitionsForQuestion), nameof(NumDefinitionsForAnswer))]
    public int Penalty =>
        // missing information about CEFR level
        (this.CefrLevelQuestion == CefrClassification.Unknown ? 1 : 0) +

        // words with CEFR level C1 and higher should be prioritized down until I learn basics
        (this.CefrLevelQuestion >= CefrClassification.C1 ? 1 : 0) +

        // words with CEFR level C2 should be prioritized down even more than B2
        (this.CefrLevelQuestion >= CefrClassification.C2 ? 1 : 0) +

        // the more individual meanings word has, the more confusing learning it with flashcards might be
        (Meanings.Count > 0 ? Meanings.Count - 1 : 0) +

        // ChatGPT raised at least one quality issue
        (HasQualityIssues ? 2 : 0) +

        // Question appears to have duplicates in the Anki collection
        DuplicatesOfQuestion.Count +

        // number of terms on the side of the flashcard. For example, if the front contains text 'mnich, zakonnik', this will be 2
        // (the ideal number is 1)
        (NumDefinitionsForQuestion - 1) +
        (NumDefinitionsForAnswer - 1) +

        // no frequency data - this can be false negative, if term is a sentence, or HTML tags weren't sanitized.
        // I can improve false alarms with heuristics
        (FrequencyPositionQuestion.HasValue ? 0 : 2) +
        (FrequencyPositionAnswer.HasValue ? 0 : 2) +

        // frequency data exists and suggests that Spanish word is used very infrequently
        (FrequencyPositionQuestion.HasValue ? CalculateFrequencyPenalty(FrequencyPositionQuestion.Value) : 0) +

        // same for polish side
        (FrequencyPositionAnswer.HasValue ? CalculateFrequencyPenalty(FrequencyPositionAnswer.Value) : 0);

    private int CalculateFrequencyPenalty(int position) => position switch
    {
        < 10000 => 0,
        < 20000 => 1,
        < 30000 => 2,
        < 40000 => 3,
        _ => 4
    };
}
