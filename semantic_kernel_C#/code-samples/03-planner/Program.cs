using Azure;
using Azure.AI.OpenAI;
using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.SemanticKernel;
using Kernel = Microsoft.SemanticKernel.Kernel;
using Microsoft.SemanticKernel.Planning.Handlebars;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Planning.Handlebars;

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
       var kernel = builder.Build();
       
        var GetCurrentDirectory = Directory.GetCurrentDirectory();
        var ParentDirectory = Directory.GetParent(GetCurrentDirectory);
        var GrandParentDirectory = Directory.GetParent(ParentDirectory.FullName);

        var writerPlugin = kernel.ImportPluginFromPromptDirectory(Path.Combine(GrandParentDirectory.FullName ,"plugins","prompt_templates", "writerPlugin"));
        
        string filePath = "../../data/chatgpt.txt";
        string fileContent =  File.ReadAllText(filePath);

        string ask = $"summarise this text: {fileContent} and email it to sam@gmail.com";
        
        Console.WriteLine("Asking A Response From The GPT Engine!!!");
        
        var planner = new HandlebarsPlanner(new HandlebarsPlannerOptions() { AllowLoops = true });
        var plan = await planner.CreatePlanAsync(kernel, ask);

        var serializedPlan = plan.ToString();

        var result = await plan.InvokeAsync(kernel);

        var chatResponse = result.ToString();

        Console.WriteLine("THE PLAN IS:" );
        Console.WriteLine(serializedPlan);

        Console.WriteLine("RESPONSE FROM CHAT ENGINE: ");
        Console.WriteLine(chatResponse);

        

        

       
    }
}


