﻿using CoreLibrary.Models;
using CoreLibrary.Services.GenerativeAiClients;
using CoreLibrary.Services.ObjectGenerativeFill;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GenerateFlashcards.Tests.TestInfrastructure;

internal class GenerativeFillTestFactory
{
    internal static GenerativeFill CreateInstance()
    {
        // read API configuration
        var config = new ConfigurationBuilder().AddUserSecrets<GenerativeFillTestFactory>().Build();
        var openAiDeveloperKey = config["OPENAI_DEVELOPER_KEY"];
        var openAiOrganizationId = config["OPENAI_ORGANIZATION_ID"];
        var azureOpenAiEndpoint = config["AZURE_OPENAI_ENDPOINT"];
        var azureOpenAiKey = config["AZURE_OPENAI_KEY"];
        var openAiCredentials = new OpenAiCredentials(azureOpenAiEndpoint, azureOpenAiKey, openAiOrganizationId, openAiDeveloperKey);

        // create ChatGPT client instance
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ChatGptClient>();

        var cacheRootFolder = Path.Combine(Path.GetTempPath(), "FlashcardSpaceToolkitCaches", "GenerateFlashcards.Tests.ChatGptClient");
        Directory.CreateDirectory(cacheRootFolder);
        var chatGptClient = new ChatGptClient(logger, openAiCredentials, cacheRootFolder);

        // create instance of system under test
        var generativeFillCacheFolder = Path.Combine(Path.GetTempPath(), "FlashcardSpaceToolkitCaches", "GenerateFlashcards.Tests.GenerativeFill");
        var settings = new GenerativeFillSettings(generativeFillCacheFolder);
        var gfLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<GenerativeFill>();
        var generativeFill = new GenerativeFill(gfLogger, chatGptClient, settings);
        return generativeFill;
    }
}

