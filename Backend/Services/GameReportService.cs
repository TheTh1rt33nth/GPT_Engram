using GPT_Engram.Models;
using System.Text.Json;

namespace GPT_Engram.Services
{
    public class GameReportService
    {
        private readonly EmbeddingService _embeddingService;
        private readonly List<GameReportChunk> _allChunks = new List<GameReportChunk>();

        private const string EmbeddedReportsFileName = "ReportsChunks.json";
        private const string SourceReportsFileName = "Reports.json";

        public GameReportService(EmbeddingService embeddingService)
        {
            _embeddingService = embeddingService;
        }

        public List<GameReportChunk> GetAllChunks() => _allChunks;

        public async Task InitializeAsync()
        {
            Console.WriteLine("Initializing embedding...");
            string chunksFilePath = Path.Combine(AppContext.BaseDirectory, "Data", EmbeddedReportsFileName);
            if (File.Exists(chunksFilePath))
            {
                var existingChunksJson = await File.ReadAllTextAsync(chunksFilePath);
                var existingChunks = JsonSerializer.Deserialize<List<GameReportChunk>>(existingChunksJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (existingChunks != null)
                {
                    _allChunks.Clear();
                    _allChunks.AddRange(existingChunks);
                }
                Console.WriteLine($"Embedding taken from cache; {_allChunks.Count} vectors loaded");
                return;
            }

            string originalReportsPath = Path.Combine(AppContext.BaseDirectory, "Data", SourceReportsFileName);
            if (!File.Exists(originalReportsPath))
                throw new FileNotFoundException($"Game reports JSON not found at {originalReportsPath}");

            var jsonText = await File.ReadAllTextAsync(originalReportsPath);
            var gameReports = JsonSerializer.Deserialize<List<GameReport>>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (gameReports == null) return;

            foreach (var report in gameReports)
            {
                string textToEmbed = $"Game Report ID: {report.Id}.\n" +
                                      $"Player 1: {report.Player1} ({report.Player1Faction}), Score: {report.Player1Score}.\n" +
                                      $"Player 2: {report.Player2} ({report.Player2Faction}), Score: {report.Player2Score}.\n" +
                                      $"Point Limit: {report.PointLimit}, GameType: {report.GameType}.\n";

                var embedding = await _embeddingService.GetEmbeddingAsync(textToEmbed);

                var chunk = new GameReportChunk
                {
                    ReportId = report.Id,
                    ChunkText = textToEmbed,
                    EmbeddingVector = embedding
                };

                _allChunks.Add(chunk);
            }

            var newChunksJson = JsonSerializer.Serialize(_allChunks, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(chunksFilePath, newChunksJson);
            Console.WriteLine($"Embedding generated and saved; {_allChunks.Count} vectors loaded");
        }
    }
}
