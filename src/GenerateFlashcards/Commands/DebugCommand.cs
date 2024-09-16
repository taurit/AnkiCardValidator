﻿using CoreLibrary.Services;
using CoreLibrary.Services.GenerativeAiClients.TextToSpeech;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GenerateFlashcards.Commands;

/// <summary>
/// A temporary command just for convenience of testing some pieces code with "F5" (Run) in Visual Studio.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DebugCommand(
        ImageGenerator imageGenerator,
        TextToSpeechClient ttsClient

    ) : AsyncCommand<DebugCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, DebugCommandSettings settings)
    {
        //(await ttsClient.GenerateAudioFile("La casa es grande y bonita.", SupportedInputLanguage.Spanish)).SaveToTemporaryFileAndPlay();
        //(await ttsClient.GenerateAudioFile("dom", SupportedTtsLanguage.Polish)).SaveToTemporaryFileAndPlay();
        //(await ttsClient.GenerateAudioFile("a house", SupportedTtsLanguage.English)).SaveToTemporaryFileAndPlay();

        //await Task.Delay(1000);

        //return 0;

        //CalculateBestCutOffLine();
        //return 0;

        var isAlive = await imageGenerator.IsAlive();
        if (!isAlive)
        {
            AnsiConsole.MarkupLine("[red]Image generator is not alive![/]");
            return 1;
        }

        var images = await imageGenerator.GenerateImageVariants("a cat", "A cat eating onion pizza.", 4);

        //var firstImageBase64 = images[0].Base64EncodedImage;
        //var htmlFragmentDisplayingBase64Image = $"<img src=\"data:image/png;base64,{firstImageBase64}\" />";
        //await File.WriteAllTextAsync("d:/testAAA.html", htmlFragmentDisplayingBase64Image);

        // generate preview for all images
        var htmlFragment = "<html>\n<body>\n";
        foreach (var image in images)
        {
            htmlFragment += $"<img src=\"data:image/png;base64,{image.Base64EncodedImage}\" /><br />\n";
        }
        htmlFragment += "</body>\n</html>\n";
        await File.WriteAllTextAsync("d:/testAAA.html", htmlFragment);

        // Launch html
        var processStartInfo = new ProcessStartInfo("d:/testAAA.html")
        {
            UseShellExecute = true
        };
        Process.Start(processStartInfo);

        return 0;
    }


    private record WordOccurrence(string Word, Int64 NumOccurrences);
    /// <summary>
    /// Takes frequency dictionary containing words from Wikipedia and number of their occurrences.
    /// Displays statistics about how much of the total words used is covered by the first N lines.
    /// </summary>
    private static void CalculateBestCutOffLine()
    {
        var frequencyDictionary = File.ReadAllLines(
            "d:\\Projekty\\FlashcardSpace.Toolkit\\src\\GenerateFlashcards\\Resources\\FrequencyDictionaries\\Spanish.Words.txt"
            );

        var occurrences = frequencyDictionary.Select(line => line.Split(' ')).Select(x => new WordOccurrence(x[0], Int64.Parse(x[1]))).ToList();

        // calculate total number of occurrences
        decimal totalOccurrencesInDictionary = occurrences.Sum(entry => entry.NumOccurrences);
        decimal processedRatio = 0;

        int lineNumber = 0;
        foreach (var line in occurrences)
        {
            var lineContributionToProcessedRatio = line.NumOccurrences / totalOccurrencesInDictionary;
            processedRatio += lineContributionToProcessedRatio;

            if ((lineNumber % 1000) == 0)
            {
                Console.WriteLine($"Line {lineNumber}: {processedRatio * 100:F2}% of language use covered already. ({line.Word} with {line.NumOccurrences} occurrences)");
            }
            lineNumber++;
        }
    }
}

static class AudioFileExtensions
{

    public static async void SaveToTemporaryFileAndPlay(this byte[] audioFile)
    {
        var audioFilePath = $"d:/testAAA.mp3";
        await File.WriteAllBytesAsync(audioFilePath, audioFile);
        var processStartInfoA = new ProcessStartInfo(audioFilePath) { UseShellExecute = true };
        Process.Start(processStartInfoA);
    }
}
