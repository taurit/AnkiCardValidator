﻿using CoreLibrary.Utilities;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace CoreLibrary.Services;

public record ImageGeneratorSettings(string CacheFolder);

/// <summary>
/// Calls API of AUTOMATIC1111's stable-diffusion-webui to generate good-looking images.
/// </summary>
public class ImageGenerator(HttpClient httpClient, ILogger<ImageGenerator> logger,
    StableDiffusionPromptProvider stableDiffusionPromptProvider, ImageGeneratorSettings settings)
{
    public async Task<List<GeneratedImage>> GenerateImageVariants(string termEnglish, string sentenceEnglish, int numImages)
    {
        var prompt = stableDiffusionPromptProvider.CreateGoodPrompt(termEnglish, sentenceEnglish);

        return await GenerateImageVariants(prompt, numImages);
    }

    private async Task<List<GeneratedImage>> GenerateImageVariants(StableDiffusionPrompt stableDiffusionPrompt, int numImagesToGenerate)
    {
        // Call API of AUTOMATIC1111's stable-diffusion-webui

        var width = 1024;
        var height = 1024;
        var numSteps = 24;
        var cfgScale = 5;
        var samplerName = "DPM++ 2M";
        var seed = 30456;
        var modelCheckpointId = new OverrideSettingsModel("sd_xl_base_1.0");
        var refinerCheckpointId = "sd_xl_refiner_1.0";
        var refinerSwitchAt = 0.7m;

        var requestPayloadModel = new TextToImageRequestModel(
            stableDiffusionPrompt.PromptText,
            stableDiffusionPrompt.NegativePromptText,
            width, height, numImagesToGenerate, numSteps, cfgScale, samplerName,
            seed, modelCheckpointId, refinerCheckpointId, refinerSwitchAt);
        var cacheFileName = GenerateCacheFileName(requestPayloadModel);

        if (File.Exists(cacheFileName))
        {
            logger.LogInformation("Using cached images for prompt {Prompt}", stableDiffusionPrompt.PromptText);
            var cachedResultSerialized = await File.ReadAllTextAsync(cacheFileName);
            var cachedResult = JsonSerializer.Deserialize<List<GeneratedImage>>(cachedResultSerialized);
            if (cachedResult == null)
                throw new InvalidOperationException($"Failed to deserialize images cached in {cacheFileName}");

            return cachedResult;
        }

        // Call API
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        var response = await httpClient.PostAsJsonAsync("http://localhost:7860/sdapi/v1/txt2img", requestPayloadModel);
        var responseModel = await response.Content.ReadFromJsonAsync<TextToImageResponseModel>();
        if (responseModel == null)
        {
            logger.LogError("Failed to generate image. Response was empty.");
            return new List<GeneratedImage>();
        }

        var arrayOfGenImages = responseModel.Images.Select(i => new GeneratedImage(i)).ToList();

        // cache the response
        await File.WriteAllTextAsync(cacheFileName, JsonSerializer.Serialize(arrayOfGenImages));
        return arrayOfGenImages;
    }

    private string GenerateCacheFileName(TextToImageRequestModel request)
    {
        settings.CacheFolder.EnsureDirectoryExists();

        var serializedRequest = JsonSerializer.Serialize(request);
        var fingerprint = serializedRequest.GetHashCodeStable(5);
        return Path.Combine(settings.CacheFolder,
            $"{request.Prompt.GetFilenameFriendlyString(20)}_{request.Width}x{request.Height}_{request.NumImages}_{fingerprint}.json");
    }

    /// <summary>
    /// Tests if local Stable Diffusion API is running.
    ///
    /// For image generation, this service must be configured and running locally:
    /// https://github.com/AUTOMATIC1111/stable-diffusion-webui/
    /// </summary>
    public async Task<bool> IsAlive()
    {
        logger.LogDebug("Testing if Stable Diffusion API is running...");

        var testUrl = "http://127.0.0.1:7860/favicon.ico";
        var timeout = TimeSpan.FromSeconds(1);
        var testHttpClient = new HttpClient { Timeout = timeout };
        try
        {
            var response = await testHttpClient.GetAsync(testUrl);
            logger.LogDebug("Stable Diffusion API is running. Received status code {StatusCode}", response.StatusCode);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            logger.LogWarning("Stable Diffusion API is not running. Can't generate images.");
            return false;
        }
    }

}
