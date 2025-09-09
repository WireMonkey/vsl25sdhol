using Microsoft.Extensions.AI;
using AiChat.Web.Components;
using AiChat.Web.Services;
using AiChat.Web.Services.Ingestion;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Connect directly to local Ollama installation
builder.AddOllamaApiClient("chat", settings =>
{
    settings.Endpoint = new Uri(builder.Configuration["Ollama:Endpoint"]!);
    settings.SelectedModel = builder.Configuration["Ollama:ChatModel"]!;
}).AddChatClient()
      .UseFunctionInvocation()
      .UseOpenTelemetry(configure: c => c.EnableSensitiveData = builder.Environment.IsDevelopment());

builder.AddOllamaApiClient("embeddings", settings =>
{
    settings.Endpoint = new Uri(builder.Configuration["Ollama:Endpoint"]!);
    settings.SelectedModel = builder.Configuration["Ollama:EmbeddingsModel"]!;
}).AddEmbeddingGenerator();

builder.AddQdrantClient("vectordb");
builder.Services.AddQdrantCollection<Guid, IngestedChunk>("data-aichat-chunks");
builder.Services.AddQdrantCollection<Guid, IngestedDocument>("data-aichat-documents");
builder.Services.AddScoped<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// By default, we ingest PDF files from the /wwwroot/Data directory. You can ingest from
// other sources by implementing IIngestionSource.
// Important: ensure that any content you ingest is trusted, as it may be reflected back
// to users or could be a source of prompt injection risk.
await DataIngestor.IngestDataAsync(
    app.Services,
    new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));

app.Run();
