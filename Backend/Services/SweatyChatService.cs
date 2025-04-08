using System.Text.Json;
using System.Text;

namespace GPT_Engram.Services
{
    public class ChatCompletionRequest
    {
        public string Model { get; set; } = "gpt-3.5-turbo";
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public double Temperature { get; set; } = 0.7;
    }

    public class ChatMessage
    {
        public string Role { get; set; } // system, user, assistant
        public string Content { get; set; }
    }

    public class ChatCompletionResponse
    {
        public List<ChatChoice> Choices { get; set; }
    }

    public class ChatChoice
    {
        public int Index { get; set; }
        public ChatMessage Message { get; set; }
    }

    public class WarhammerChatService
    {
        private readonly EmbeddingService _embeddingService;
        private readonly GameReportService _reportService;
        private readonly HttpClient _httpClient;
        private readonly string _openAiApiKey;
        private const string OpenAiChatUrl = "https://api.openai.com/v1/chat/completions";
        private JsonSerializerOptions _serializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public WarhammerChatService(EmbeddingService embeddingService, GameReportService reportService, string openAiApiKey)
        {
            _embeddingService = embeddingService;
            _reportService = reportService;
            _openAiApiKey = openAiApiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");
        }

        public async Task<string> AskAsync(string userQuery)
        {
            var queryEmbedding = await _embeddingService.GetEmbeddingAsync(userQuery);

            var chunks = _reportService.GetAllChunks();
            var topChunks = chunks
                .OrderByDescending(c => CosineSimilarity(c.EmbeddingVector, queryEmbedding))
                .Take(10);
            string systemPrompt = @"
                You are a Warhammer 40k game reports assistant. 
                Answer questions ONLY from the provided context.
                If the answer isn't in the context, say you're not sure and elaborate what data are you lacking.
                If user exhibits rude behavior, answer specifically: 'Go to Reddit.com, such sweats belong there'
                Context:
                " + string.Join("\n", topChunks.Select(chunk=>chunk.ChunkText));

            var requestObj = new ChatCompletionRequest
            {
                Model = "gpt-4o-mini",
                Messages = new List<ChatMessage>
                {
                    new ChatMessage { Role = "system", Content = systemPrompt },
                    new ChatMessage { Role = "user", Content = userQuery }
                },
                Temperature = 0.7
            };

            var jsonContent = JsonSerializer.Serialize(requestObj, _serializationOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(OpenAiChatUrl, content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var completionResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseString, _serializationOptions);

            return completionResponse?.Choices?[0]?.Message?.Content?.Trim() ?? "";
        }

        private static float CosineSimilarity(List<float> v1, List<float> v2)
        {
            if (v1 == null || v2 == null || v1.Count != v2.Count) return 0f;

            float dot = 0f;
            float mag1 = 0f;
            float mag2 = 0f;

            for (int i = 0; i < v1.Count; i++)
            {
                dot += v1[i] * v2[i];
                mag1 += v1[i] * v1[i];
                mag2 += v2[i] * v2[i];
            }

            float denominator = (float)(Math.Sqrt(mag1) * Math.Sqrt(mag2));
            return denominator == 0 ? 0 : dot / denominator;
        }
    }
}
