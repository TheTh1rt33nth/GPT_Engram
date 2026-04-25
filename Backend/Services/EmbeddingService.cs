using System.Text.Json;
using System.Text;

namespace GPT_Engram.Services
{
    public class OpenAiEmbeddingRequest
    {
        public string Model { get; set; } = "text-embedding-ada-002";
        public List<string> Input { get; set; } = new List<string>();
    }

    public class OpenAiEmbeddingResponse
    {
        public List<OpenAiEmbeddingData> Data { get; set; }
    }

    public class OpenAiEmbeddingData
    {
        public List<float> Embedding { get; set; }
        public int Index { get; set; }
    }

    public class EmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _openAiApiKey;
        private const string OpenAiEmbeddingUrl = "https://api.openai.com/v1/embeddings";
        private JsonSerializerOptions _serializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public EmbeddingService(string openAiApiKey)
        {
            _openAiApiKey = openAiApiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");
        }

        public async Task<List<float>> GetEmbeddingAsync(string text)
        {
            var requestObj = new OpenAiEmbeddingRequest
            {
                Model = "text-embedding-ada-002",
                Input = new List<string> { text }
            };

            var jsonContent = JsonSerializer.Serialize(requestObj, _serializationOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(OpenAiEmbeddingUrl, content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var embeddingResponse = JsonSerializer.Deserialize<OpenAiEmbeddingResponse>(responseString, _serializationOptions);

            return embeddingResponse?.Data?[0]?.Embedding ?? new List<float>();
        }
    }
}
