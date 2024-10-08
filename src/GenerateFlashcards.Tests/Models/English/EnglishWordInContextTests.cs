﻿using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;
using GenerateFlashcards.Models.English;
using GenerateFlashcards.Models.Spanish;
using GenerateFlashcards.Tests.TestInfrastructure;

namespace GenerateFlashcards.Tests.Models.English;

[TestClass]
[TestCategory("RequiresGenerativeAi")]
public class EnglishWordInContextTests
{
    private readonly GenerativeFill _generativeFill = GenerativeFillTestFactory.CreateInstance();

    [TestMethod]
    public async Task WordCatShouldBeRecognizedAndClassifiedAsNoun()
    {
        // Arrange
        var input = new EnglishWordInContext() { Word = "cat" };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.Word.Should().Be("cat");
        output.WordBaseForm.Should().Be("a cat");
        output.SentenceExample.Should().NotBeNullOrEmpty();
        output.PartOfSpeech.Should().Be(EnglishPartOfSpeech.Noun);
    }

    [TestMethod]
    public async Task WordRunShouldBeRecognizedAndClassifiedAsVerb()
    {
        // Arrange
        var input = new EnglishWordInContext() { Word = "run" };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.Word.Should().Be("run");
        output.WordBaseForm.Should().Be("to run");
        output.SentenceExample.Should().NotBeNullOrEmpty();
        output.PartOfSpeech.Should().Be(EnglishPartOfSpeech.Verb);
    }

    [TestMethod]
    public async Task WordBlueShouldBeRecognizedAndClassifiedAsAdjective()
    {
        // Arrange
        var input = new EnglishWordInContext() { Word = "blue" };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.Word.Should().Be("blue");
        output.WordBaseForm.Should().Be("blue");
        output.SentenceExample.Should().NotBeNullOrEmpty();
        output.PartOfSpeech.Should().Be(EnglishPartOfSpeech.Adjective);
    }

}
