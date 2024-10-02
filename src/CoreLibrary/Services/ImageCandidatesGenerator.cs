﻿using CoreLibrary.Utilities;
using Microsoft.Extensions.Logging;

namespace CoreLibrary.Services;


public class ImageCandidatesGenerator(
        ImageGenerator imageGenerator,
        StableDiffusionPromptProvider promptProvider,
        ILogger<ImageCandidatesGenerator> logger)
{
    public async Task<List<GeneratedImage>> GenerateImageVariants(string termEnglish, string sentenceEnglish, ImageGenerationProfile profile)
    {
        var results = new List<GeneratedImage>();

        const int cfgScaleMin = 3;
        const int cfgScaleMax = 6;

        int numExperiments = profile == ImageGenerationProfile.PublicDeck ? 4 : 2;

        // `numImagesInExperiment` is a number of images in a batch, which all share the same prompt and other parameters but differ in seed.
        // In private deck I favor variants over independent keywords, because variants are faster to generate (the model stays in VRAM etc.)
        int numImagesInExperiment = profile == ImageGenerationProfile.PublicDeck ? 2 : 3;

        decimal cfgScaleStep = numExperiments == 1 ? 0 : ((decimal)cfgScaleMax - cfgScaleMin) / (numExperiments - 1);

        for (int i = 0; i < numExperiments; i++)
        {
            logger.LogDebug("Generating images for experiment {ExperimentNumber}/{NumExperiments}", i + 1, numExperiments);

            // I want the seed to be deterministic, but also unique for each term or sentence.
            // If we just use `i` as the seed, the same small set of random keywords will be reused for all terms,
            // leading to images in the same style.
            //
            // It could be a good thing if we look for a consistent style across all flashcards, but also boring
            // if we get 400 images with the 'horror,gothic' theme.
            var seed = i
                + sentenceEnglish.GetHashCodeStableInt() // add deterministic variety to the seed
                ;

            var prompt = promptProvider.CreateGoodPrompt(termEnglish, sentenceEnglish, seed);
            int cfgScaleForExperiment = cfgScaleMin + (int)(cfgScaleStep * i);

            logger.LogInformation("CFG={CfgScale}, Prompt: {Prompt}", cfgScaleForExperiment, prompt.PromptText);

            // for Stable Diffusion I also want a deterministic seed that changes, because using the same one for all images leads to images with similar composition.
            var images = await imageGenerator.GenerateImageBatch(prompt, numImagesInExperiment, cfgScaleForExperiment, seed, GeneratedImageAspectRatio.Wide, profile);

            results.AddRange(images);
        }

        return results;
    }
}
