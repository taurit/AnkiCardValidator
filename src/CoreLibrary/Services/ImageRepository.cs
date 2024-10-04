﻿using CoreLibrary.Models;
using CoreLibrary.Services.Embeddings;
using CoreLibrary.Utilities;

namespace CoreLibrary.Services;

/// <summary>
/// Allows search for image candidates matching the prompt in the repository of previously generated images.
/// Potentially allows to save time on image generation which is by far the most time-consuming part of flashcard generation.
/// </summary>
public class ImageRepository(ImageRepositorySettings settings, EmbeddingsService embeddingsService)
{
    private readonly List<StableDiffusionImage>? _images = null;
    private List<StableDiffusionImage> LoadExistingImagesMetadata()
    {
        if (_images is not null)
            return _images;

        var images = new List<StableDiffusionImage>();

        var files = Directory.GetFiles(settings.ImageRepositoryFolder, "*.jpg");
        foreach (var file in files)
        {
            var parameters = StableDiffusionHelper.GetStableDiffusionParametersFromImage(file);
            if (parameters is null)
            {
                continue;
            }
            images.Add(new StableDiffusionImage(file, parameters));
        }
        return images;
    }

    internal async Task<List<StableDiffusionImageSimilarity>> FindMatchingImageCandidates(string prompt, int width, int height)
    {
        // filter by width and height first
        var images = LoadExistingImagesMetadata();
        images = images.Where(x => x.Parameters.Width == width && x.Parameters.Height == height).ToList();

        // temporary: filter by quality because I have some dev images with ~10 steps in my repository
        images = images.Where(x => x.Parameters.Steps > 20).ToList();

        // filter by prompt - use Embeddings to match the prompt with some threshold
        var promptWithoutStyleKeywords = StableDiffusionKeywordRemover.RemoveStyleKeywordsFromPrompt(prompt);
        var embedding = await embeddingsService.CreateEmbedding(promptWithoutStyleKeywords);

        // prime embeddings cache in batch operations
        var promptsInRepo = images.Select(x => x.Parameters.PromptWithoutStyleKeywords).ToList();
        await embeddingsService.PrimeEmbeddingsCache(promptsInRepo);

        var imageSimilarities = new List<StableDiffusionImageSimilarity>();
        foreach (var imageInRepo in images)
        {
            var repoEmbedding = await embeddingsService
                .CreateEmbedding(imageInRepo.Parameters.PromptWithoutStyleKeywords);

            var similarityValue = embeddingsService.CosineSimilarity(embedding, repoEmbedding);
            var similarityRecord = new StableDiffusionImageSimilarity(imageInRepo, similarityValue);

            imageSimilarities.Add(similarityRecord);
        }

        // sort by similarity
        imageSimilarities = imageSimilarities
            .OrderByDescending(x => x.SimilarityScore)
            .Where(x => x.SimilarityScore > 0.70) // arbitrary, but: ideal match is 1.0,
                                                  // very close match 0.86,
                                                  // 0.60 is borderline useful
            .ToList();

        return imageSimilarities;
    }
}

public record ImageRepositorySettings(string ImageRepositoryFolder);
internal record StableDiffusionImage(string FilePath, StableDiffusionParameters Parameters);
internal record StableDiffusionImageSimilarity(StableDiffusionImage Image, double SimilarityScore);
