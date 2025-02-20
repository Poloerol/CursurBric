using System;
using System.Collections.Generic;
using System.Linq;

public class TournamentSystem
{
    public class Tournament
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Team> Teams { get; set; }
        public List<Match> Matches { get; set; }
        public TournamentType Type { get; set; }
        public TournamentStatus Status { get; set; }
        public Dictionary<string, int> Standings { get; set; }
    }

    public class Team
    {
        public string Name { get; set; }
        public List<string> Players { get; set; }
        public int Points { get; set; }
        public int VictoryPoints { get; set; }
    }

    public class Match
    {
        public Team Team1 { get; set; }
        public Team Team2 { get; set; }
        public DateTime ScheduledTime { get; set; }
        public int Team1Score { get; set; }
        public int Team2Score { get; set; }
        public bool IsCompleted { get; set; }
        public List<GameHistory> Games { get; set; }
    }

    public enum TournamentType
    {
        SingleElimination,
        DoubleElimination,
        RoundRobin,
        Swiss
    }

    public enum TournamentStatus
    {
        Upcoming,
        InProgress,
        Completed
    }

    private readonly List<Tournament> tournaments;

    public TournamentSystem()
    {
        tournaments = [];
    }

    public Tournament CreateTournament(string name, TournamentType type, DateTime startDate, DateTime endDate)
    {
        var tournament = new Tournament
        {
            Name = name,
            Type = type,
            StartDate = startDate,
            EndDate = endDate,
            Teams = [],
            Matches = [],
            Status = TournamentStatus.Upcoming,
            Standings = []
        };

        tournaments.Add(tournament);
        return tournament;
    }

    public static void AddTeam(Tournament tournament, string teamName, List<string> players)
    {
        var team = new Team
        {
            Name = teamName,
            Players = players,
            Points = 0,
            VictoryPoints = 0
        };

        tournament.Teams.Add(team);
        tournament.Standings[teamName] = 0;
    }

    public static void GenerateSchedule(Tournament tournament)
    {
        switch (tournament.Type)
        {
            case TournamentType.RoundRobin:
                GenerateRoundRobinSchedule(tournament);
                break;
            case TournamentType.Swiss:
                GenerateSwissSchedule(tournament);
                break;
            // Diğer turnuva tipleri için metodlar eklenebilir
        }

        tournament.Status = TournamentStatus.InProgress;
    }

    private static void GenerateRoundRobinSchedule(Tournament tournament)
    {
        var teams = tournament.Teams.ToList();
        if (teams.Count % 2 != 0)
            teams.Add(null); // Bye round için dummy takım

        int rounds = teams.Count - 1;
        int matchesPerRound = teams.Count / 2;

        for (int round = 0; round < rounds; round++)
        {
            for (int match = 0; match < matchesPerRound; match++)
            {
                var team1 = teams[match];
                var team2 = teams[teams.Count - 1 - match];

                if (team1 != null && team2 != null)
                {
                    tournament.Matches.Add(new Match
                    {
                        Team1 = team1,
                        Team2 = team2,
                        ScheduledTime = tournament.StartDate.AddDays(round),
                        Games = []
                    });
                }
            }

            // Takımları döndür
            teams.Insert(1, teams[^1]);
            teams.RemoveAt(teams.Count - 1);
        }
    }

    private static void GenerateSwissSchedule(Tournament tournament)
    {
        // İlk tur için rastgele eşleştirme
        var teams = tournament.Teams.OrderBy(x => Guid.NewGuid()).ToList();
        for (int i = 0; i < teams.Count; i += 2)
        {
            if (i + 1 < teams.Count)
            {
                tournament.Matches.Add(new Match
                {
                    Team1 = teams[i],
                    Team2 = teams[i + 1],
                    ScheduledTime = tournament.StartDate,
                    Games = []
                });
            }
        }
    }

    public static void RecordMatchResult(Tournament tournament, Match match, int team1Score, int team2Score)
    {
        match.Team1Score = team1Score;
        match.Team2Score = team2Score;
        match.IsCompleted = true;

        // Puanları güncelle
        UpdateStandings(tournament, match);

        // Tüm maçlar tamamlandıysa turnuvayı bitir
        if (tournament.Matches.All(m => m.IsCompleted))
        {
            tournament.Status = TournamentStatus.Completed;
        }
    }

    private static void UpdateStandings(Tournament tournament, Match match)
    {
        if (match.Team1Score > match.Team2Score)
        {
            tournament.Standings[match.Team1.Name] += 2;
            tournament.Standings[match.Team2.Name] += 0;
        }
        else if (match.Team1Score < match.Team2Score)
        {
            tournament.Standings[match.Team1.Name] += 0;
            tournament.Standings[match.Team2.Name] += 2;
        }
        else
        {
            tournament.Standings[match.Team1.Name] += 1;
            tournament.Standings[match.Team2.Name] += 1;
        }
    }

    public static List<KeyValuePair<string, int>> GetTournamentStandings(Tournament tournament)
    {
        return [.. tournament.Standings.OrderByDescending(x => x.Value)];
    }
} 