// Pseudocode Plan:
// 1. Remove hard-coded API key.
// 2. Add configuration builder: user secrets + environment variables.
// 3. Retrieve key from configuration path "OpenAI:ApiKey".
// 4. Throw clear exception if missing.
// 5. Keep rest of chat logic unchanged.
// 6. Add internal Program class so AddUserSecrets<Program>() works with top-level statements.
// Usage (run once):
//   dotnet user-secrets init
//   dotnet user-secrets set "OpenAI:ApiKey" "<your_api_key_here>"
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;

var endpoint = new Uri("https://models.github.ai/inference");
// Ensure the UserSecrets package is referenced in your project
// You can add it via NuGet Package Manager or by running:
// dotnet add package Microsoft.Extensions.Configuration.UserSecrets

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var credential = configuration["OpenAI:ApiKey"]
    ?? throw new InvalidOperationException("Missing secret 'OpenAI:ApiKey'. Set with: dotnet user-secrets set \"OpenAI:ApiKey\" \"<value>\"");

var model = "openai/gpt-4.1-nano";

var openAIOptions = new OpenAIClientOptions()
{
    Endpoint = endpoint
};

var client = new ChatClient(model, new ApiKeyCredential(credential), openAIOptions);

DisplayWelcomeMessage();

while (true)
{
    Console.Write("\nYour question: ");
    var userInput = Console.ReadLine();

    if (string.IsNullOrEmpty(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    List<ChatMessage> messages = new()
    {
        new SystemChatMessage("You are a helpful C# and .NET programming expert. Provide clear, concise answers with code examples when appropriate."),
        new UserChatMessage(userInput),
    };

    var requestOptions = new ChatCompletionOptions()
    {
        Temperature = 0.7f,
        MaxOutputTokenCount = 1000,
    };

    try
    {
        Console.WriteLine("\nAI Response:");
        Console.WriteLine("------------");
        var response = client.CompleteChat(messages, requestOptions);
        Console.WriteLine(response.Value.Content[0].Text);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

Console.WriteLine("\nThanks for using the AI Chat Console!");

static void DisplayWelcomeMessage()
{
    Console.WriteLine("VSLIVE! 2025 - AI Chat Console (Secure Version)");
    Console.WriteLine("===============================================");
    Console.WriteLine("Features:");
    Console.WriteLine("- Secure token management with .NET Secret Manager");
    Console.WriteLine("- Integration with GitHub Models API");
    Console.WriteLine("- Interactive chat interface");
    Console.WriteLine();
    Console.WriteLine("Ask me anything about C# and .NET! (type 'exit' to quit)");
}