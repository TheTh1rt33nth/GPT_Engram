namespace GPT_Engram.Models
{
    public class GameReport
    {
        public string Id { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public int Player1Score { get; set; }
        public int Player2Score { get; set; }
        public string Player1Faction { get; set; }
        public string Player2Faction { get; set; }
        public int PointLimit { get; set; }
        public string GameType { get; set; } // "competitive" or "friendly"
    }
    public class GameReportChunk
    {
        public string ReportId { get; set; }
        public string ChunkText { get; set; }
        public List<float> EmbeddingVector { get; set; }
    }
}
