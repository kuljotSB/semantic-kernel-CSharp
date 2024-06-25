using Azure;
using Azure.AI.OpenAI;
using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.SemanticKernel;
using Kernel = Microsoft.SemanticKernel.Kernel;

public  class Globals 
{
    private static readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    public static string key = config["OPENAI_KEY"];
    public static string endpoint = config["OPENAI_ENDPOINT"];
    public static string model = config["OPENAI_MODEL"];

}

public static class Program 
{
    public static async Task Main()
    {
        var builder = Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(Globals.model, Globals.endpoint, Globals.key);

        var kernelBuild = builder.Build();
        var GetCurrentDirectory = Directory.GetCurrentDirectory();
        var ParentDirectory = Directory.GetParent(GetCurrentDirectory);
        var GrandParentDirectory = Directory.GetParent(ParentDirectory.FullName);

        var basicPlugin = kernelBuild.ImportPluginFromPromptDirectory(Path.Combine(GrandParentDirectory.FullName ,"plugins","prompt_templates", "basic_plugin"));
        var greetingFunction = basicPlugin["greeting"];
        var arguments  = new KernelArguments() {
            ["name"] = "Kuljot",
            ["age"] = 18
        };

        var greetingResult = await kernelBuild.InvokeAsync(greetingFunction, arguments);
        Console.WriteLine(greetingResult);

    }
}

