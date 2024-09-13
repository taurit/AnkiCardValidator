﻿using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace GenerateFlashcards.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by the Spectre.Console.Cli framework")]
internal sealed class GenerateFromFrequencyDictionarySettings : CommandSettings
{
    [Description("Path to search. Defaults to current directory.")]
    [CommandArgument(0, "<inputFile>")] // <angleBrackets> mean required, [squareBrackets] mean optional
    public required string InputFilePath { get; init; }

    [Description("Name of the language of the content in the input file (typically, a foreign language we learn).")]
    [CommandOption("--inputLanguage")]
    [DefaultValue(SupportedInputLanguage.Autodetect)]
    public SupportedInputLanguage InputLanguage { get; init; }

    [Description("Name of the language to use for translation and explanations (typically, our native language).")]
    [CommandOption("--outputLanguage")]
    [DefaultValue(SupportedOutputLanguage.Unknown)]
    public required SupportedOutputLanguage OutputLanguage { get; init; }

    public override ValidationResult Validate()
    {
        // ensure the `outputLanguage` option is set
        // currently, custom validation is the only way to make option required : https://github.com/spectreconsole/spectre.console/discussions/538
        var outputLanguageIsNotSet = OutputLanguage == SupportedOutputLanguage.Unknown;
        if (outputLanguageIsNotSet)
            return ValidationResult.Error("The `--outputLanguage` must be set.");

        // make sure the input and output languages are different
        var inputLanguageName = InputLanguage.ToString();
        var outputLanguageName = OutputLanguage.ToString();
        var inputAndOutputLanguageIsTheSame = inputLanguageName == outputLanguageName;

        if (inputAndOutputLanguageIsTheSame)
            return ValidationResult.Error("The `--outputLanguage` must be different from the `--inputLanguage`.");

        if (!File.Exists(InputFilePath))
            return ValidationResult.Error($"The input file `{InputFilePath}` cannot be found.");

        if (InputLanguage == SupportedInputLanguage.Autodetect)
            return ValidationResult.Error("The `--inputLanguage` must be set explicitly (the auto-detection is not implemented yet).");

        return ValidationResult.Success();
    }
}