using System;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Core;
using Kernel = Microsoft.SemanticKernel.Kernel;

public  class Globals 
{
    private static readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    public static string key = config["OPENAI_KEY"];
    public static string endpoint = config["OPENAI_ENDPOINT"];
    public static string model = config["OPENAI_MODEL"];

    public static string bingApiKey = config["BING_API_KEY"];

}

public class WebSearcherPlugin {

    public static async Task Main()
    { 
        //initialize the kernel
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(Globals.model!, Globals.endpoint!, Globals.key!);
        var kernel = builder.Build();
        
        //import the default bing websearcher plugin into the kernel
        var bingConnector = new BingConnector(Globals.bingApiKey!);
        kernel.ImportPluginFromObject( new WebSearchEnginePlugin(bingConnector), "bing");
        var function = kernel.Plugins["bing"]["search"];

        //take input from the user
        Console.WriteLine("Enter the search query: ");
        string userQuery = Console.ReadLine();
          
        Console.WriteLine("Sending Message To Chat Completions Model");

        //fetch the initial response from the plugin invocation
        var bingResult = await kernel.InvokeAsync(function, new() { ["query"] = userQuery});
        var stringResult = bingResult.ToString();
        stringResult = stringResult.Replace("["," ");
        stringResult = stringResult.Replace("]"," ");

        
       

       //use Azure OpenAI chat completion model to print the information in a better way
        var systemPrompt = $@"you will be provided with some jumbled information. Your task is to unjumble the information and provide a summary of the information. 
        so the thing is that the jumbled information is retrieved from the bing websearcher plugin in semantic kernel SDK and becuae the information is all jumbled up,
        we cannot simply provide it to the user, so you need to extract relevant information from the jumbled information and provide a summary of the information.
        you will be provided with both the jumbled information and the user query that was sent to the web searcher plugin of the semantic kernel SDK that caused such a jumbled information to be retrieved in the first place.
        ==============================================================
        Important Points:
        1)The user shouldn't get the jumbled information directly.
        2)The user should be able to understand the information.
        3) the user should not get to know that we used semantic kernel SDK to retrieve the information. it should appear as if you are the one that is providing the information.
        4) You should not include anything like ' Based on the user's query about the current stock prices of NVIDIA, the jumbled information from various sources can be summarized as follows:'
        ==============================================================
        the user query is : {userQuery}
        the jumbled information is :{stringResult}  ";

        var systemMessage = "you are a helpful AI assistant";

        OpenAIClient client = new OpenAIClient(new Uri(Globals.endpoint!), new AzureKeyCredential(Globals.key!));

        ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions()
        {
            Messages =
            {
                new ChatRequestSystemMessage(systemMessage),
                new ChatRequestUserMessage(systemPrompt),
            },
            MaxTokens = 400,
            Temperature = 0.7f,
            DeploymentName = Globals.model!
        };

         ChatCompletions finalResponse = client.GetChatCompletions(chatCompletionsOptions);
         string completion = finalResponse.Choices[0].Message.Content;

         Console.WriteLine(completion);

    }

}
    