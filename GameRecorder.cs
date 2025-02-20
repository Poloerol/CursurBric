using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class GameRecorder
{
    private readonly string recordingsPath = "game_recordings";
    private readonly List<GameRecord> currentGame;
    private bool isRecording;

    public class GameRecord
    {
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("action")]
        public required string Action { get; set; }

        [JsonProperty("player")]
        public required string Player { get; set; }

        [JsonProperty("details")]
        public required string Details { get; set; }

        [JsonProperty("gameState")]
        public Dictionary<string, object>? GameState { get; set; }
    }

    public GameRecorder()
    {
        Directory.CreateDirectory(recordingsPath);
        currentGame = [];
        isRecording = false;
    }

    public void StartRecording()
    {
        currentGame.Clear();
        isRecording = true;
        RecordEvent("GameStart", "System", "Yeni oyun başladı");
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        RecordEvent("GameEnd", "System", "Oyun bitti");
        SaveRecording();
        isRecording = false;
    }

    public void RecordEvent(string action, string player, string details, Dictionary<string, object>? gameState = null)
    {
        if (!isRecording) return;

        currentGame.Add(new GameRecord
        {
            Timestamp = DateTime.Now,
            Action = action,
            Player = player,
            Details = details,
            GameState = gameState
        });
    }

    private void SaveRecording()
    {
        var filename = Path.Combine(recordingsPath, $"game_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        var json = JsonConvert.SerializeObject(currentGame, Formatting.Indented);
        File.WriteAllText(filename, json);
    }

    public List<GameRecord> LoadRecording(string filename)
    {
        var path = Path.Combine(recordingsPath, filename);
        if (!File.Exists(path)) return [];

        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<List<GameRecord>>(json) ?? [];
    }

    public List<string> GetRecordingsList()
    {
        return Directory.GetFiles(recordingsPath, "*.json")
                        .Select(Path.GetFileName)
                        .Select(fileName => fileName!)
                        .ToList();
    }

    public void PlaybackRecording(string filename, Action<GameRecord> onEventPlayback)
    {
        var recording = LoadRecording(filename);
        if (recording == null || recording.Count == 0) return;

        foreach (var record in recording)
        {
            onEventPlayback(record);
            // Gerçek zamanlı oynatma için bekleme eklenebilir
            // await Task.Delay(1000);
        }
    }
}