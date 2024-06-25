using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.IO;
using Microsoft.SemanticKernel;
using Kernel = Microsoft.SemanticKernel.Kernel;
using System.ComponentModel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;


public class Globals{
    private static readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    public static string key = config["OPENAI_KEY"];
    public static string endpoint = config["OPENAI_ENDPOINT"];
    public static string model = config["OPENAI_MODEL"];

}
public class MathPlugin
{
    [KernelFunction, Description("Add two numbers")]
    public static double Add([Description("first number")] double a, [Description("second number")] double b)
    {
        return a + b;
    }
     
    [KernelFunction, Description("Subtract two numbers")]
    public static double Subtract([Description("first number")] double a, [Description("second number")] double b)
    {
        return a - b;
    }
    
    [KernelFunction, Description("Multiply two numbers")]
    public static double Multiply([Description("first number")] double a, [Description("second number")] double b)
    {
        return a * b;
    }
    
    [KernelFunction, Description("Divide Two Numbers")]
    public static double Divide([Description("first number")] double a, [Description("second number")] double b)
    {
        return a / b;
    }

    
}


public class Program
{
    public async static Task Main()
    {
        var builder = Kernel.CreateBuilder();
        builder.Services.AddAzureOpenAIChatCompletion(Globals.model, Globals.endpoint, Globals.key);
        
        builder.Plugins.AddFromType<MathPlugin>();
        var kernel = builder.Build();
        
        ChatHistory history = [];

        history.AddUserMessage("Add 5 and 3 together to get their sum");

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    // Get the response from the AI
    var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Stream the results
    string fullMessage = "";
    var first = true;
    await foreach (var content in result)
    {
        if (content.Role.HasValue && first)
        {
            Console.Write("Assistant > ");
            first = false;
        }
        Console.Write(content.Content);
        fullMessage += content.Content;
    }
// }


        
    }
}

