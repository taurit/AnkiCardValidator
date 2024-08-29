﻿using CoreLibrary.Services.GenerativeAiClients;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreLibrary.Services.ObjectGenerativeFill;

/// <summary>
/// Helps process arrays of items instead of single item, to save on input tokens
/// </summary>
public class GenerativeFill(IGenerativeAiClient generativeAiClient, string generativeFillCacheFolder)
{
    // exposed for testing
    internal string GenerativeFillCacheFolder { get; } = generativeFillCacheFolder;

    const string SystemChatMessage = "Your job is to transform array of input objects into array of output objects. Adhere to the JSON schema and use property descriptions for guidelines.";

    /// <summary>
    ///  Excluded from DI so library client doesn't have to be aware of that internal class.
    /// </summary>
    private readonly GenerativeFillSchemaProvider _schemaProvider = new();
    private readonly GenerativeFillCache _cache = new(generativeFillCacheFolder);


    public async Task<T> FillMissingProperties<T>(string modelId, string modelClassId, T inputElement) where T : ObjectWithId, new()
    {
        var inputElements = new List<T> { inputElement };
        var response = await FillMissingProperties(modelId, modelClassId, inputElements);
        return response.Single();
    }

    public async Task<List<T>> FillMissingProperties<T>(string modelId, string modelClassId, List<T> inputItems) where T : ObjectWithId, new()
    {
        // build prompt
        var promptTemplate = "Input contains array of items to process (in the `Items` property):\n" +
                               "\n" +
                               "```json\n" +
                               "{0}\n" +
                               "```\n" +
                               "\n" +
                               "The output should be in JSON and contain array of output items (with one output item for each input item, linked by Id).";

        var outputItems = _cache.FillFromCacheWherePossible(modelClassId, SystemChatMessage, promptTemplate, inputItems);
        var itemsThatRequireApiCall = outputItems.Where(x => x.Id is null).ToList();

        // assign consecutive IDs to input elements
        for (var i = 0; i < outputItems.Count; i++)
            outputItems[i].Id = i + 1; // start from 1, just in case AI is trained to treat "0" differently

        if (itemsThatRequireApiCall.Count > 0)
        {
            var schema = _schemaProvider.GenerateJsonSchemaForArrayOfItems<T>();

            var chunks = itemsThatRequireApiCall.Chunk(20);

            foreach (var chunk in chunks)
            {
                var itemsThatRequireApiCallChunk = chunk.ToList();
                var inputSerialized = SerializeInput(itemsThatRequireApiCallChunk);
                var prompt = String.Format(promptTemplate, inputSerialized);
                var response = await generativeAiClient.GetAnswerToPrompt(modelId, modelClassId, SystemChatMessage, prompt, GenerativeAiClientResponseMode.StructuredOutput, schema);
                var apiResultItems = DeserializeResponse<T>(response, itemsThatRequireApiCallChunk.Count);
                var newOutputItems = ReplacePlaceholdersWithFullObjects(outputItems, apiResultItems);

                _cache.SaveToCache(modelClassId, SystemChatMessage, promptTemplate, newOutputItems);
                outputItems = newOutputItems;
            }

        }

        // return output
        return outputItems;
    }

    private static string SerializeInput<T>(List<T> inputObjects) where T : ObjectWithId, new()
    {
        var inputSerializationOptions = new JsonSerializerOptions();
        inputSerializationOptions.Converters.Add(new JsonStringEnumConverter());
        inputSerializationOptions.Converters.Add(new GenerativeFillSerializationConverter<T>(SerializationSetting.IdAndInputs));

        // wrapping array with an object because OpenAI API doesn't like JSON arrays as root element
        var inputArrayAsObject = new ArrayOfItemsWithIds<T>(inputObjects);
        var inputSerialized = JsonSerializer.Serialize(inputArrayAsObject, inputSerializationOptions);
        return inputSerialized;
    }

    private static List<T> DeserializeResponse<T>(string response, int numInputElements) where T : ObjectWithId, new()
    {
        var deserializationOptions = new JsonSerializerOptions();
        deserializationOptions.Converters.Add(new JsonStringEnumConverter());
        var resultObject = JsonSerializer.Deserialize<ArrayOfItemsWithIds<T>>(response, deserializationOptions);
        var resultItems = resultObject.Items;

        if (resultItems.Count != numInputElements)
        {
            throw new InvalidOperationException("Number of items in response doesn't match number of items in input.");
        }

        return resultItems;
    }

    private static List<T> ReplacePlaceholdersWithFullObjects<T>(List<T> partiallyFilledList, List<T> elementsFromApi) where T : ObjectWithId
    {
        var completelyFilledList = new List<T>();

        foreach (var element in partiallyFilledList)
        {
            // find element in API response
            var apiResult = elementsFromApi.SingleOrDefault(x => x.Id == element.Id);

            if (apiResult == null)
            {
                // object already read from cache earlier; rewrite to output
                completelyFilledList.Add(element);
            }
            else
            {
                // fill out the missing properties first
                CloneValuesOfPropertiesWithoutAttributes(element, apiResult);
                completelyFilledList.Add(apiResult);
            }
        }
        return completelyFilledList;
    }

    private static void CloneValuesOfPropertiesWithoutAttributes<T>(T element, T apiResult) where T : ObjectWithId
    {
        var properties = typeof(T).GetProperties();
        foreach (var prop in properties)
        {
            if (prop.Name == "Id")
                continue;

            var aiFilledProperty = prop.GetCustomAttribute<FillWithAIAttribute>() is not null;
            if (!aiFilledProperty)
            {
                var propertyValueInApiResponseObject = prop.GetValue(element);
                prop.SetValue(apiResult, propertyValueInApiResponseObject);
            }
        }
    }
}