using GPT_Engram.Services;

var builder = WebApplication.CreateBuilder(args);

string openAiApiKey = builder.Configuration["OpenAI:ApiKey"] ?? string.Empty;
if (string.IsNullOrWhiteSpace(openAiApiKey))
{
    throw new Exception("OpenAI API Key not configured. Set in appsettings.json or user-secrets.");
}
var embeddingService = new EmbeddingService(openAiApiKey);
var gameReportService = new GameReportService(embeddingService);
var warhammerChatService = new WarhammerChatService(embeddingService, gameReportService, openAiApiKey);

builder.Services.AddSingleton(embeddingService);
builder.Services.AddSingleton(gameReportService);
builder.Services.AddSingleton(warhammerChatService);
builder.Services.AddControllers();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "Warhammer 40k Game Reports ChatBot is running. POST to /api/chat/ask with JSON { 'userQuery': '...' }");

await gameReportService.InitializeAsync();

app.Run();
