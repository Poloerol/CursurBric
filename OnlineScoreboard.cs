using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;
using CursurBric;
using System.Text;

public class OnlineScoreboard
{
    private readonly string apiUrl = "https://your-api-url.com/scores";
    private readonly HttpClient client;
    private bool isConnected;

    public OnlineScoreboard()
    {
        client = new HttpClient();
    }

    public async Task<bool> CheckConnection()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync(apiUrl + "/ping");
            isConnected = response.IsSuccessStatusCode;
            return isConnected;
        }
        catch
        {
            isConnected = false;
            MessageBox.Show("Sunucu bağlantısı kurulamadı!");
            return false;
        }
    }

    public async Task<bool> UploadScore(GameHistory game)
    {
        if (!isConnected && !await CheckConnection())
            return false;

        try
        {
            var json = JsonConvert.SerializeObject(game);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl, content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<GameHistory>> GetLeaderboard()
    {
        try
        {
            var response = await client.GetStringAsync($"{apiUrl}/leaderboard");
            return JsonConvert.DeserializeObject<List<GameHistory>>(response);
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<GameHistory>> GetPlayerHistory(string playerName)
    {
        try
        {
            var response = await client.GetStringAsync($"{apiUrl}/player/{playerName}");
            return JsonConvert.DeserializeObject<List<GameHistory>>(response);
        }
        catch
        {
            return [];
        }
    }
} 