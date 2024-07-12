using Microsoft.SemanticKernel;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.ComponentModel;
using Microsoft.SemanticKernel.Planning.Handlebars;
using Kernel = Microsoft.SemanticKernel.Kernel; 
using System;

public class Secrets {
    private static readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    public static string  openaiKey = config["OPENAI_KEY"];
    public static string  openaiEndpoint = config["OPENAI_ENDPOINT"];
    public static string  openaiChatModel = config["OPENAI_CHAT_MODEL"];
    public static string  clientId = config["CLIENT_ID"];
    public static string tenantId = config["TENANT_ID"];
}

public static class Token {
    public static string accessToken {get;set;}
}

public class GraphPlugin 
{
    [KernelFunction, Description("To list the calendar events of the user such as meetings etc.")]
    public async Task<string> ListCalendarEvents([Description("the query of the user")]string userQuery)
    {
        string url = "https://graph.microsoft.com/v1.0/me/events?$select=subject,body,bodyPreview,organizer,attendees,start,end,location";
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Token.accessToken}");
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage response = await client.GetAsync(url);
        string responseString = await response.Content.ReadAsStringAsync();
        
        

        OpenAIClient openAIClient = new OpenAIClient(new Uri(Secrets.openaiEndpoint!), new AzureKeyCredential(Secrets.openaiKey!));

        string systemMessage = @$"You are a helpful AI assistant meant to assist the user by answering their queries related to knowing the calendar events in
        the microsoft graph API. you will be presented with the user query that the user asked and a JSON response of the graph API. Extract
        information from that JSON response based on the user query and present it to the user in a readable format.";

        string systemPrompt = @$"The user query is: {userQuery}. The JSON response from the graph API is: {responseString}. 
        Extract information from the JSON response based on the user query and present it to the user in a readable format.";

        
        ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions()
        {
            Messages =
            {
                new ChatRequestSystemMessage(systemMessage),
                new ChatRequestUserMessage(systemPrompt),
            },
            MaxTokens = 400,
            Temperature = 0.7f,
            DeploymentName = Secrets.openaiChatModel
        };

    ChatCompletions finalResponse = openAIClient.GetChatCompletions(chatCompletionsOptions);
    string completion = finalResponse.Choices[0].Message.Content;

      return completion;


    }

    
}

public class pluginInvocationClass {
    public static async Task Main()
    {
        string clientId = Secrets.clientId!;
        string authority = $"https://login.microsoftonline.com/{Secrets.tenantId}/v2.0";
        string[] scopes = new string[] { "User.Read", "Calendars.Read", "Calendars.ReadWrite"}; // Use the appropriate scope for Graph API
        
         var app = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(authority)
                .Build();

        var result =  app.AcquireTokenWithDeviceCode(scopes, callback =>
                {
                    Console.WriteLine(callback.Message);
                    return Task.FromResult(0);
                }).ExecuteAsync().Result;

                // Print the access token
        Console.WriteLine(result.AccessToken);
        Console.WriteLine(result.AccessToken);
        Token.accessToken = result.AccessToken;

        Console.WriteLine("----------------------------XXXXXXXXXXXXXXXXXXXXX-----------------------------");


        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(Secrets.openaiChatModel!, Secrets.openaiEndpoint!, Secrets.openaiKey!);
        var kernel = builder.Build();

        var graphPlugin = kernel.ImportPluginFromType<GraphPlugin>();

        var planner = new HandlebarsPlanner(new HandlebarsPlannerOptions() { AllowLoops = true });

        Console.WriteLine("Enter the query: ");
        string user_query = Console.ReadLine();
        
        try {
            var plan = await planner.CreatePlanAsync(kernel, user_query);

            var resultResponse = await plan.InvokeAsync(kernel);

            string chatResponse = resultResponse.ToString();

            Console.WriteLine(chatResponse);
        }
        catch(Exception error)
        {
            Console.WriteLine(error.Message);
    }
}
}

      
      



        


    


