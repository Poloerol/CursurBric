using System;
using System.Collections.Generic;

public class PlayerProfile
{
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime JoinDate { get; set; }
    public int Rating { get; set; }
    public string PreferredPosition { get; set; }
    public List<Achievement> Achievements { get; set; }
    public Dictionary<string, int> BiddingStats { get; set; }
    public Dictionary<string, int> PlayingStats { get; set; }

    public PlayerProfile()
    {
        Achievements = [];
        BiddingStats = [];
        PlayingStats = [];
        JoinDate = DateTime.Now;
        Rating = 1500; // Başlangıç ELO puanı
    }
}

public class Achievement
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime UnlockedDate { get; set; }
    public string Icon { get; set; }
} 