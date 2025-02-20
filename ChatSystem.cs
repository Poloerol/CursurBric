using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ChatSystem
{
    private Form chatForm;
    private RichTextBox chatBox;
    private TextBox messageBox;
    private ClientWebSocket webSocket;
    private bool isConnected;
    private readonly string webSocketUrl = "ws://your-server/chat";
    private readonly Dictionary<string, TabPage> privateChatTabs;

    public ChatSystem()
    {
        privateChatTabs = [];
        InitializeChat();
    }

    private void InitializeChat()
    {
        chatForm = new Form
        {
            Text = "Briç Sohbet",
            Size = new Size(400, 600),
            StartPosition = FormStartPosition.CenterScreen
        };

        var tabControl = new TabControl
        {
            Dock = DockStyle.Fill
        };

        // Genel sohbet sekmesi
        var generalTab = CreateChatTab("Genel");
        tabControl.TabPages.Add(generalTab);

        // Takım sohbeti sekmesi
        var teamTab = CreateChatTab("Takım");
        tabControl.TabPages.Add(teamTab);

        // Oyun sohbeti sekmesi
        var gameTab = CreateChatTab("Oyun");
        tabControl.TabPages.Add(gameTab);

        chatForm.Controls.Add(tabControl);

        // Bağlantı durumu
        var statusStrip = new StatusStrip();
        var statusLabel = new ToolStripStatusLabel("Bağlantı bekleniyor...");
        statusStrip.Items.Add(statusLabel);
        chatForm.Controls.Add(statusStrip);

        ConnectToServer();
    }

    private TabPage CreateChatTab(string name)
    {
        var tab = new TabPage(name);
        
        chatBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            Size = new Size(380, 480)
        };

        messageBox = new TextBox
        {
            Dock = DockStyle.Bottom,
            Size = new Size(300, 30)
        };
        messageBox.KeyPress += (s, e) =>
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SendMessage(messageBox.Text, name);
                messageBox.Clear();
                e.Handled = true;
            }
        };

        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5)
        };
        panel.Controls.Add(chatBox);
        panel.Controls.Add(messageBox);

        tab.Controls.Add(panel);
        return tab;
    }

    private async Task ConnectToServer()
    {
        try
        {
            webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(new Uri(webSocketUrl), CancellationToken.None);
            isConnected = true;
            StartListening();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Bağlantı hatası: {ex.Message}");
        }
    }

    private async Task StartListening()
    {
        var buffer = new byte[1024];
        while (isConnected)
        {
            try
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    DisplayMessage(message);
                }
            }
            catch
            {
                isConnected = false;
                break;
            }
        }
    }

    private async Task SendMessage(string message, string channel)
    {
        if (!isConnected) return;

        var data = Encoding.UTF8.GetBytes($"{channel}:{message}");
        await webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private void DisplayMessage(string message)
    {
        if (chatBox.InvokeRequired)
        {
            chatBox.Invoke(new Action(() => DisplayMessage(message)));
            return;
        }

        chatBox.AppendText($"{DateTime.Now:HH:mm} - {message}\n");
        chatBox.ScrollToCaret();
    }

    public void Show()
    {
        chatForm.Show();
    }
} 