public class PlayerStatistics
{
    public string PlayerName { get; set; }
    public int GamesPlayed { get; set; }
    public int ContractsMade { get; set; }
    public int ContractsDefeated { get; set; }
    public int SlamsBid { get; set; }
    public int SlamsMade { get; set; }
    public double AverageScore { get; set; }
    public Position MostPlayedPosition { get; set; }
}

public enum Position
{
    South,
    West,
    North,
    East
} 