using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.SemanticKernel;
using Kernel = Microsoft.SemanticKernel.Kernel;
using System;


public  class Globals 
{
    private static readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    public static string key = config["OPENAI_KEY"];
    public static string endpoint = config["OPENAI_ENDPOINT"];
    public static string model = config["OPENAI_MODEL"];

}

public class Program 
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
        var contactInformationFunction =basicPlugin["contact_information"];

        var arguments = new KernelArguments() {
            ["name"] = "kuljot",
            ["contact_number"] = "123456789",
            ["email_id"]="hello@gmail.com",
            ["address"]="123, abc street, xyz city"
        };

        var response = await kernelBuild.InvokeAsync(contactInformationFunction, arguments);
        Console.WriteLine(response);
            }
}