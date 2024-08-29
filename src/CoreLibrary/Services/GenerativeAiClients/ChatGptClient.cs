﻿using CoreLibrary.Utilities;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace CoreLibrary.Services.GenerativeAiClients;

public class ChatGptClient(ILogger logger, string openAiOrganization, string openAiDeveloperKey, string persistentCacheRootFolder) : IGenerativeAiClient
{
    public async Task<string> GetAnswerToPrompt(string modelId, string modelClassId, string systemChatMessage, string prompt,
        GenerativeAiClientResponseMode mode, string? outputSchema = null)
    {
        if (mode is GenerativeAiClientResponseMode.StructuredOutput && outputSchema is null)
            throw new ArgumentException("Schema is required for StructuredOutput mode.");
        if (mode is not GenerativeAiClientResponseMode.StructuredOutput && outputSchema is not null)
            throw new ArgumentException("Schema is only allowed for StructuredOutput mode.");

        var openAiClientOptions = new OpenAIClientOptions { OrganizationId = openAiOrganization };
        ChatClient client = new(model: modelId, new ApiKeyCredential(openAiDeveloperKey), openAiClientOptions);

        var stableHash = (prompt + outputSchema).GetHashCodeStable();
        // proper extensions helps debugging (e.g. VS Code highlights JSON files if they have proper extension only)
        var cacheFileExtension = mode == GenerativeAiClientResponseMode.PlainText ? "txt" : "json";
        var responseCacheFileName = $"{modelClassId}_{stableHash}.{cacheFileExtension}";
        var responseToPromptFileName = Path.Combine(persistentCacheRootFolder, responseCacheFileName);

        if (File.Exists(responseToPromptFileName))
        {
            logger.LogDebug("Response for the prompt found in cache, re-using.");
            var fileContent = await File.ReadAllTextAsync(responseToPromptFileName);
            return fileContent;
        }
        logger.LogDebug($"Response not found in cache, retrieving response from model {modelId} (class: {modelClassId})...");

        var options = new ChatCompletionOptions();

        switch (mode)
        {
            case GenerativeAiClientResponseMode.PlainText:
                options.ResponseFormat = ChatResponseFormat.Text;
                break;
            case GenerativeAiClientResponseMode.JsonMode:
                options.ResponseFormat = ChatResponseFormat.JsonObject;
                break;
            case GenerativeAiClientResponseMode.StructuredOutput:
                options.ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    "transformed_input_schema",
                    BinaryData.FromString(outputSchema!),
                    "Output schema containing an array of output items, each of which corresponds to one input item and is linked to it by the Id property."
                    );
                break;
        }

        List<ChatMessage> messages =
        [
            new SystemChatMessage(systemChatMessage),
            new UserChatMessage(prompt)
        ];

        ChatCompletion completion = await client.CompleteChatAsync(messages, options);
        var responseToPrompt = completion.Content[0].Text;

        await File.WriteAllTextAsync(responseToPromptFileName, responseToPrompt);
        logger.LogDebug($"Query finished, the usage was {completion.Usage.InputTokens} input tokens and {completion.Usage.OutputTokens} output tokens.");

        return responseToPrompt;
    }
}