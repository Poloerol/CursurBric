using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using CursurBric;

public class StatisticsGraphs(List<GameHistory> history)
{
    private Form graphForm;
    private TabControl tabControl;
    private readonly List<GameHistory> gameHistory = history;

    public void ShowGraphs()
    {
        graphForm = new Form
        {
            Text = "Detaylı İstatistik Grafikleri",
            Size = new Size(1000, 600),
            StartPosition = FormStartPosition.CenterScreen
        };

        tabControl = new TabControl
        {
            Dock = DockStyle.Fill
        };

        // Skor dağılımı grafiği
        var scoreTab = new TabPage("Skor Dağılımı");
        var scoreChart = CreateScoreDistributionChart();
        scoreTab.Controls.Add(scoreChart);

        // Kontrat türleri grafiği
        var contractTab = new TabPage("Kontrat Türleri");
        var contractChart = CreateContractTypesChart();
        contractTab.Controls.Add(contractChart);

        // Oyuncu performans grafiği
        var performanceTab = new TabPage("Oyuncu Performansı");
        var performanceChart = CreatePlayerPerformanceChart();
        performanceTab.Controls.Add(performanceChart);

        // Vulnerability analizi
        var vulTab = new TabPage("Vulnerability Analizi");
        var vulChart = CreateVulnerabilityAnalysisChart();
        vulTab.Controls.Add(vulChart);

        tabControl.TabPages.AddRange(
        [
            scoreTab, 
            contractTab, 
            performanceTab, 
            vulTab 
        ]);

        graphForm.Controls.Add(tabControl);
        graphForm.ShowDialog();
    }

    private Chart CreateScoreDistributionChart()
    {
        var chart = new Chart
        {
            Dock = DockStyle.Fill
        };

        var area = new ChartArea();
        chart.ChartAreas.Add(area);

        var series = new Series
        {
            ChartType = SeriesChartType.Column,
            Name = "Skor Dağılımı"
        };

        // Skor aralıklarını belirle
        var scores = gameHistory.Select(g => g.Score)
                              .GroupBy(s => s / 100 * 100)
                              .OrderBy(g => g.Key);

        foreach (var scoreGroup in scores)
        {
            series.Points.AddXY(
                $"{scoreGroup.Key} - {scoreGroup.Key + 99}",
                scoreGroup.Count()
            );
        }

        chart.Series.Add(series);
        return chart;
    }

    private Chart CreateContractTypesChart()
    {
        var chart = new Chart
        {
            Dock = DockStyle.Fill
        };

        var area = new ChartArea();
        chart.ChartAreas.Add(area);

        var series = new Series
        {
            ChartType = SeriesChartType.Pie,
            Name = "Kontrat Türleri"
        };

        var contractTypes = gameHistory
            .GroupBy(g => g.Contract.Split(' ').Last())
            .OrderByDescending(g => g.Count());

        foreach (var type in contractTypes)
        {
            series.Points.AddXY(type.Key, type.Count());
        }

        chart.Series.Add(series);
        return chart;
    }

    private Chart CreatePlayerPerformanceChart()
    {
        var chart = new Chart
        {
            Dock = DockStyle.Fill
        };

        var area = new ChartArea();
        chart.ChartAreas.Add(area);

        var series = new Series
        {
            ChartType = SeriesChartType.Line,
            Name = "Oyuncu Performansı"
        };

        var playerPerformance = gameHistory
            .GroupBy(g => g.Declarer)
            .Select(g => new
            {
                Player = g.Key,
                AverageScore = g.Average(h => h.Score),
                ContractsMade = g.Count(h => h.Score > 0),
                TotalGames = g.Count()
            });

        foreach (var player in playerPerformance)
        {
            series.Points.AddXY(
                player.Player,
                (double)player.ContractsMade / player.TotalGames * 100
            );
        }

        chart.Series.Add(series);
        return chart;
    }

    private Chart CreateVulnerabilityAnalysisChart()
    {
        var chart = new Chart
        {
            Dock = DockStyle.Fill
        };

        var area = new ChartArea();
        chart.ChartAreas.Add(area);

        var series = new Series
        {
            ChartType = SeriesChartType.Column,
            Name = "Vulnerability Analizi"
        };

        var vulAnalysis = gameHistory
            .GroupBy(g => g.Vulnerability)
            .Select(g => new
            {
                Vulnerability = g.Key,
                AverageScore = g.Average(h => h.Score),
                SuccessRate = g.Count(h => h.Score > 0) / (double)g.Count() * 100
            });

        foreach (var vul in vulAnalysis)
        {
            series.Points.AddXY(
                vul.Vulnerability.ToString(),
                vul.SuccessRate
            );
        }

        chart.Series.Add(series);
        return chart;
    }
} 