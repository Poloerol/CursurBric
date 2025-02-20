using System;
using System.Text.Json.Serialization;
using CursurBric;

namespace CursurBric
{
    public enum Vulnerability
    {
        None,
        NorthSouth,
        EastWest,
        Both
    }

    public class GameHistory
    {
        [JsonPropertyName("contract")]
        public string? Contract { get; set; }

        [JsonPropertyName("declarer")]
        public string? Declarer { get; set; }

        [JsonPropertyName("tricks")]
        public int Tricks { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("vulnerability")]
        public Vulnerability Vulnerability { get; set; }

        [JsonPropertyName("playedAt")]
        public DateTime PlayedAt { get; set; }

        [JsonPropertyName("players")]
        public string[] Players { get; set; } = new string[4]; // Güney, Batı, Kuzey, Doğu

        [JsonPropertyName("isTournamentGame")]
        public bool IsTournamentGame { get; set; }

        [JsonPropertyName("tournamentName")]
        public string? TournamentName { get; set; }
    }
}