using System.Web.Helpers;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.Windows.Forms;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace CursurBric
{
    public class MainForm : Form
    {
        private Deck deck;
        private readonly List<List<Card>> playerHands;
        private readonly Button dealButton;
        private Button shuffleButton;
        private Panel[] playerPanels;
        private Label[] playerLabels;
        private GroupBox bidPanel;
        private ComboBox levelComboBox;
        private ComboBox suitComboBox;
        private Button bidButton;
        private Button passButton;
        private Label currentBidLabel;
        private Label currentPlayerLabel;
        private int currentPlayerIndex;
        private int passCount;
        private string currentBid;
        private Button doubleButton;
        private Button redoubleButton;
        private Label scoreLabel;
        private readonly int[] teamScores;
        private bool isDoubled;
        private bool isRedoubled;
        private GamePhase currentPhase;
        private Vulnerability currentVulnerability;
        private List<GameHistory> gameHistory;
        private DataGridView historyGrid;
        private ComboBox vulnerabilityComboBox;
        private Button showStatsButton;
        private Chart scoreChart;
        private TextBox[] playerNameBoxes;
        private bool isTournamentMode;
        private string currentTournamentName;
        private Dictionary<string, PlayerStatistics> playerStats;
        private MenuStrip menuStrip;
        private ToolStrip toolStrip;
        private readonly TutorialMode tutorialMode;
        private readonly OnlineScoreboard onlineScoreboard;
        private readonly Dictionary<string, PlayerProfile> playerProfiles;
        private HandAnalysis currentHandAnalysis;
        private readonly AnimatedTutorial animatedTutorial;
        private readonly StatisticsGraphs statisticsGraphs;
        private readonly ChatSystem chatSystem;
        private readonly GameRecorder gameRecorder;
        private readonly TournamentSystem tournamentSystem;

        private enum GamePhase
        {
            Bidding,
            Playing,
            Scoring
        }

        private enum Vulnerability
        {
            None,
            NorthSouth,
            EastWest,
            Both
        }

        private enum GameState
        {
            NotStarted,
            Dealing,
            Bidding,
            Playing,
            Finished
        }

        private readonly GameState currentGameState = GameState.NotStarted;
        private static readonly string[] sourceArray = ["Trefl", "Karo", "Kupa", "Pik", "NT"];

        private class GameHistory
        {
            public string? Contract { get; set; } // Marking Contract as nullable - from previous fix
            public string Declarer { get; set; } // Non-nullable property causing the error
            public int Tricks { get; set; }
            public int Score { get; set; }
            public Vulnerability Vulnerability { get; set; }
            public DateTime PlayedAt { get; set; }
            public string[] Players { get; set; } = new string[4]; // Güney, Batı, Kuzey, Doğu
            [JsonProperty("ısTournamentGame")]
            public bool IsTournamentGame { get; set; }
            public string TournamentName { get; set; }
        }

        public MainForm()
        {
            deck = new Deck();
            playerHands = new List<List<Card>>();
            InitializeComponents();
            InitializeBiddingComponents();
            currentPlayerIndex = 0;
            passCount = 0;
            currentBid = "Henüz teklif yok";
            teamScores = new int[2]; // Kuzey-Güney ve Doğu-Batı takımları
            isDoubled = false;
            isRedoubled = false;
            currentPhase = GamePhase.Bidding;
            gameHistory = new List<GameHistory>();
            currentVulnerability = Vulnerability.None;
            InitializeNewComponents();
            InitializeHistoryComponents();
            InitializeTournamentComponents();
            InitializeMenuAndToolbar();
            tutorialMode = new TutorialMode();
            onlineScoreboard = new OnlineScoreboard();
            playerProfiles = new Dictionary<string, PlayerProfile>();
            animatedTutorial = new AnimatedTutorial();
            statisticsGraphs = new StatisticsGraphs(gameHistory);
            chatSystem = new ChatSystem();
            gameRecorder = new GameRecorder();
            tournamentSystem = new TournamentSystem();

            // dealButton'ı başlat
            dealButton = new Button
            {
                Text = "Kartları Dağıt",
                Location = new Point(140, 10),
                Size = new Size(120, 30)
            };
            dealButton.Click += DealButton_Click;
            Controls.Add(dealButton);
        }

        private void InitializeComponents()
        {
            Size = new Size(800, 600);
            Text = "Briç Oyunu";

            // Butonları oluştur
            shuffleButton = new Button
            {
                Text = "Desteyi Karıştır",
                Location = new Point(10, 10),
                Size = new Size(120, 30)
            };
            shuffleButton.Click += ShuffleButton_Click;
            Controls.Add(shuffleButton);

            // Oyuncu panellerini oluştur
            playerPanels = new Panel[4];
            playerLabels = new Label[4];
            string[] positions = ["Güney", "Batı", "Kuzey", "Doğu"];

            for (int i = 0; i < 4; i++)
            {
                playerLabels[i] = new Label
                {
                    Text = positions[i],
                    AutoSize = true
                };

                playerPanels[i] = new Panel
                {
                    BorderStyle = BorderStyle.FixedSingle,
                    Size = new Size(350, 150)
                };

                // Panel pozisyonlarını ayarla
                switch (i)
                {
                    case 0: // Güney
                        playerPanels[i].Location = new Point(220, 400);
                        playerLabels[i].Location = new Point(220, 380);
                        break;
                    case 1: // Batı
                        playerPanels[i].Location = new Point(10, 200);
                        playerLabels[i].Location = new Point(10, 180);
                        break;
                    case 2: // Kuzey
                        playerPanels[i].Location = new Point(220, 50);
                        playerLabels[i].Location = new Point(220, 30);
                        break;
                    case 3: // Doğu
                        playerPanels[i].Location = new Point(430, 200);
                        playerLabels[i].Location = new Point(430, 180);
                        break;
                }

                Controls.Add(playerPanels[i]);
                Controls.Add(playerLabels[i]);
            }
        }

        private void InitializeBiddingComponents()
        {
            bidPanel = new GroupBox
            {
                Text = "Deklarasyon",
                Location = new Point(10, 50),
                Size = new Size(200, 200)
            };

            currentPlayerLabel = new Label
            {
                Text = "Sıradaki Oyuncu: Güney",
                Location = new Point(10, 20),
                Size = new Size(180, 20)
            };
            bidPanel.Controls.Add(currentPlayerLabel);

            currentBidLabel = new Label
            {
                Text = "Mevcut Teklif: " + currentBid.ToString(),
                Location = new Point(10, 45),
                Size = new Size(180, 20)
            };
            bidPanel.Controls.Add(currentBidLabel);

            levelComboBox = new ComboBox
            {
                Location = new Point(10, 70),
                Size = new Size(60, 25)
            };
            for (int i = 1; i <= 7; i++)
                levelComboBox.Items.Add(i);
            bidPanel.Controls.Add(levelComboBox);

            suitComboBox = new ComboBox
            {
                Location = new Point(80, 70),
                Size = new Size(100, 25)
            };
            suitComboBox.Items.AddRange(["Trefl", "Karo", "Kupa", "Pik", "NT"]);
            bidPanel.Controls.Add(suitComboBox);

            bidButton = new Button
            {
                Text = "Artır",
                Location = new Point(10, 100),
                Size = new Size(80, 30)
            };
            bidButton.Click += BidButton_Click;
            bidPanel.Controls.Add(bidButton);

            passButton = new Button
            {
                Text = "Pas",
                Location = new Point(100, 100),
                Size = new Size(80, 30)
            };
            passButton.Click += PassButton_Click;
            bidPanel.Controls.Add(passButton);

            Controls.Add(bidPanel);
            bidPanel.Enabled = false;
        }

        private void InitializeNewComponents()
        {
            // Kontra butonları
            doubleButton = new Button
            {
                Text = "Kontra",
                Location = new Point(10, 140),
                Size = new Size(80, 30),
                Enabled = false
            };
            doubleButton.Click += DoubleButton_Click;
            bidPanel.Controls.Add(doubleButton);

            redoubleButton = new Button
            {
                Text = "Sürkontra",
                Location = new Point(100, 140),
                Size = new Size(80, 30),
                Enabled = false
            };
            redoubleButton.Click += RedoubleButton_Click;
            bidPanel.Controls.Add(redoubleButton);

            // Skor göstergesi
            scoreLabel = new Label
            {
                Text = "Skor: K-G: 0 | D-B: 0",
                Location = new Point(10, 180),
                Size = new Size(180, 20)
            };
            bidPanel.Controls.Add(scoreLabel);
        }

        private void InitializeHistoryComponents()
        {
            // Vulnerability seçici
            Label vulLabel = new()
            {
                Text = "Vulnerability:",
                Location = new Point(270, 10),
                Size = new Size(80, 20)
            };
            Controls.Add(vulLabel);

            vulnerabilityComboBox = new ComboBox
            {
                Location = new Point(350, 10),
                Size = new Size(120, 25)
            };
            vulnerabilityComboBox.Items.AddRange(Enum.GetNames(typeof(Vulnerability)));
            vulnerabilityComboBox.SelectedIndex = 0;
            vulnerabilityComboBox.SelectedIndexChanged += (s, e) =>
            {
                currentVulnerability = (Vulnerability)vulnerabilityComboBox.SelectedIndex;
            };
            Controls.Add(vulnerabilityComboBox);

            // El geçmişi grid'i
            historyGrid = new DataGridView
            {
                Location = new Point(600, 50),
                Size = new Size(180, 400),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };
            historyGrid.Columns.Add("Contract", "Kontrat");
            historyGrid.Columns.Add("Score", "Skor");
            Controls.Add(historyGrid);

            // İstatistik butonu
            showStatsButton = new Button
            {
                Text = "İstatistikler",
                Location = new Point(600, 460),
                Size = new Size(180, 30)
            };
            showStatsButton.Click += ShowStatsButton_Click;
            Controls.Add(showStatsButton);
        }

        private void InitializeTournamentComponents()
        {
            var tournamentGroup = new GroupBox
            {
                Text = "Turnuva Modu",
                Location = new Point(600, 500),
                Size = new Size(180, 100)
            };

            var tournamentCheck = new CheckBox
            {
                Text = "Turnuva Modu",
                Location = new Point(10, 20),
                AutoSize = true
            };
            tournamentCheck.CheckedChanged += (s, e) =>
            {
                isTournamentMode = tournamentCheck.Checked;
                if (isTournamentMode)
                {
                    string tournamentNameInput = ShowInputDialog("Turnuva Adı:");
                    if (!string.IsNullOrEmpty(tournamentNameInput))
                    {
                        currentTournamentName = tournamentNameInput;
                    }
                }
                {
                    Text = "Oyuncu İsimleri";
                    Location = new Point(10, 50);
                    Size = new Size(160, 30);
                }
                ;
                playerNamesButton.Click += ShowPlayerNamesDialog;

                tournamentGroup.Controls.AddRange(new Control[] { tournamentCheck, playerNamesButton });
                Controls.Add(tournamentGroup);

                // Grafik oluşturma
                scoreChart = new Chart
                {
                    Location = new Point(800, 50),
                    Size = new Size(300, 200)
                };
                var chartArea = new ChartArea();
                scoreChart.ChartAreas.Add(chartArea);
                var series = new Series("Skorlar")
                {
                    ChartType = SeriesChartType.Line
                };
                scoreChart.Series.Add(series);
                Controls.Add(scoreChart);
            };
        }

        private void InitializeMenuAndToolbar()
        {
            // Menü çubuğu
            menuStrip = new MenuStrip();
            MainMenuStrip = menuStrip;

            var fileMenu = new ToolStripMenuItem("Dosya");
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Yeni Oyun", null, (s, e) => NewGame()),
                new ToolStripMenuItem("Kaydet", null, (s, e) => SaveGame()),
                new ToolStripMenuItem("Yükle", null, (s, e) => LoadGame()),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Çıkış", null, (s, e) => Close())
            });

            var profileMenu = new ToolStripMenuItem("Profil");
            profileMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Profil Yönetimi", null, (s, e) => ShowProfileManager()),
                new ToolStripMenuItem("Başarılar", null, (s, e) => ShowAchievements()),
                new ToolStripMenuItem("İstatistikler", null, (s, e) => ShowPlayerStats())
            });

            var analysisMenu = new ToolStripMenuItem("Analiz");
            analysisMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("El Analizi", null, (s, e) => ShowHandAnalysis()),
                new ToolStripMenuItem("Oyun Geçmişi", null, (s, e) => ShowGameHistory()),
                new ToolStripMenuItem("İstatistiksel Analiz", null, (s, e) => ShowDetailedStatistics())
            });

            var onlineMenu = new ToolStripMenuItem("Online");
            onlineMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Skor Tablosu", null, async (s, e) => await ShowLeaderboard()),
                new ToolStripMenuItem("Arkadaş Listesi", null, (s, e) => ShowFriendList()),
                new ToolStripMenuItem("Turnuvalar", null, (s, e) => ShowTournaments())
            });

            var chatMenuItem = new ToolStripMenuItem("Sohbet", null, (s, e) => chatSystem.Show());
            var recordingsMenuItem = new ToolStripMenuItem("Oyun Kayıtları", null, (s, e) => ShowGameRecordings());
            var tournamentsMenuItem = new ToolStripMenuItem("Turnuvalar", null, (s, e) => ShowTournamentManager());

            var helpMenu = new ToolStripMenuItem("Yardım");
            helpMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Öğretici", null, (s, e) => ShowTutorial()),
                new ToolStripMenuItem("Kurallar", null, (s, e) => ShowRules()),
                new ToolStripMenuItem("Hakkında", null, (s, e) => ShowAbout())
            });

            menuStrip.Items.AddRange(new ToolStripItem[] {
                fileMenu,
                profileMenu,
                analysisMenu,
                onlineMenu,
                chatMenuItem,
                recordingsMenuItem,
                tournamentsMenuItem,
                helpMenu
            });

            Controls.Add(menuStrip);

            // Araç çubuğu
            toolStrip = new ToolStrip();
            toolStrip.Items.AddRange(new ToolStripItem[] {
                new ToolStripButton("Analiz", null, (s, e) => ShowHandAnalysis()),
                new ToolStripButton("Öğretici", null, (s, e) => ShowTutorial()),
                new ToolStripButton("Profil", null, (s, e) => ShowProfileManager()),
                new ToolStripSeparator(),
                new ToolStripButton("Online", null, async (s, e) => await ShowLeaderboard())
            });

            Controls.Add(toolStrip);
        }

        private void ShuffleButton_Click(object sender, EventArgs e)
        {
            deck = new Deck();
            deck.Shuffle();
            ClearAllPanels();
            dealButton.Enabled = true;
        }

        private bool ValidateGameStart()
        {
            // Kart resimleri kontrolü
            if (!CardImageManager.ValidateCardImages())
                return false;

            // Oyuncu sayısı kontrolü
            if (playerHands.Count != 4)
            {
                MessageBox.Show("4 oyuncu gerekli!");
                return false;
            }

            // Kart dağıtımı kontrolü
            foreach (var hand in playerHands)
            {
                if (hand.Count != 13)
                {
                    MessageBox.Show("Her oyuncuda 13 kart olmalı!");
                    return false;
                }
            }

            return true;
        }

        private void DealButton_Click(object sender, EventArgs e)
        {
            if (!ValidateGameStart())
                return;

            DealCards();
            dealButton.Enabled = false;
        }

        private void ClearAllPanels()
        {
            foreach (Panel panel in playerPanels)
            {
                panel.Controls.Clear();
            }
            playerHands.Clear();
        }

        private void DealCards()
        {
            ClearAllPanels();
            for (int i = 0; i < 4; i++)
            {
                List<Card> hand = deck.DealHand(13);
                playerHands.Add(hand);
                DisplayHand(hand, playerPanels[i]);
            }
            bidPanel.Enabled = true;
            currentPlayerIndex = 0;
            passCount = 0;
            currentBid = "Henüz teklif yok";
            UpdateBiddingLabels();
            isDoubled = false;
            isRedoubled = false;
            doubleButton.Enabled = false;
            redoubleButton.Enabled = false;
            currentPhase = GamePhase.Bidding;

            // El analizi yap
            if (currentPlayerIndex < playerHands.Count)
            {
                currentHandAnalysis = new HandAnalysis(playerHands[currentPlayerIndex]);

                // Otomatik öneriler
                if (currentHandAnalysis.Suggestions.Count > 0)
                {
                    var showSuggestions = MessageBox.Show(
                        "Bu el için öneriler mevcut. Görmek ister misiniz?",
                        "El Analizi",
                        MessageBoxButtons.YesNo);

                    if (showSuggestions == DialogResult.Yes)
                    {
                        ShowHandAnalysis();
                    }
                }
            }
        }

        private void DisplayHand(List<Card> hand, Panel panel)
        {
            panel.Controls.Clear();
            int cardWidth = 71;
            int cardHeight = 96;
            int overlap = 30;
            int startX = 10;
            int startY = 10;

            for (int i = 0; i < hand.Count; i++)
            {
                var cardPicture = new CardPictureBox
                {
                    Size = new Size(cardWidth, cardHeight),
                    Location = new Point(startX + i * (cardWidth - overlap), startY),
                    Image = CardImageManager.GetCardImage(hand[i]),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Tag = hand[i]
                };

                cardPicture.Click += (s, e) =>
                {
                    var pb = (CardPictureBox)s;
                    pb.IsSelected = !pb.IsSelected;

                    if (currentPhase == GamePhase.Playing)
                    {
                        // Oyun sırasında kart oynama işlemleri
                        PlayCard((Card)pb.Tag);
                    }
                };

                panel.Controls.Add(cardPicture);
            }
        }

        private bool ValidateCardPlay(Card card)
        {
            if (!ValidateGameState(GameState.Playing))
                return false;

            // Sıra kontrolü
            if (currentPlayerIndex != GetCurrentPlayerIndex())
            {
                MessageBox.Show("Sıranız değil!");
                return false;
            }

            // Renk uyumu kontrolü
            if (currentTrick.Any() && card.Suit != currentTrick[0].Suit)
            {
                var hand = playerHands[currentPlayerIndex];
                if (hand.Any(c => c.Suit == currentTrick[0].Suit))
                {
                    MessageBox.Show("Aynı renkten kart oynamalısınız!");
                    return false;
                }
            }

            return true;
        }

        private void PlayCard(Card card)
        {
            if (!ValidateCardPlay(card))
                return;

            // Kartı oynat
            // Oyun durumunu güncelle
            // Sıradaki oyuncuya geç
        }

        private bool ValidateBid(string newBid)
        {
            try
            {
                if (string.IsNullOrEmpty(newBid))
                    throw new ArgumentException("Teklif boş olamaz!");

                string[] parts = newBid.Split(' ');
                if (parts.Length != 2)
                    throw new ArgumentException("Geçersiz teklif formatı!");

                int level = int.Parse(parts[0]);
                if (level < 1 || level > 7)
                    throw new ArgumentException("Teklif seviyesi 1-7 arasında olmalı!");

                string suit = parts[1];
                if (!sourceArray.Contains(suit))
                    throw new ArgumentException("Geçersiz renk!");

                return IsValidBid(newBid); // Mevcut teklif kontrolü
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Teklif hatası: {ex.Message}");
                return false;
            }
        }

        private void BidButton_Click(object sender, EventArgs e)
        {
            if (!ValidateBid($"{levelComboBox.SelectedItem} {suitComboBox.SelectedItem}"))
                return;

            if (levelComboBox.SelectedItem == null || suitComboBox.SelectedItem == null)
            {
                MessageBox.Show("Lütfen seviye ve renk seçiniz.");
                return;
            }

            string newBid = $"{levelComboBox.SelectedItem} {suitComboBox.SelectedItem}";
            if (IsValidBid(newBid))
            {
                currentBid = newBid;
                passCount = 0;
                NextPlayer();
            }
            else
            {
                MessageBox.Show("Geçersiz teklif. Daha yüksek bir teklif vermelisiniz.");
            }
        }

        private void PassButton_Click(object? sender, EventArgs e)
        {
            passCount++;
            if (passCount == 3 && currentBid != "Henüz teklif yok")
            {
                _ = GetPlayerPosition((currentPlayerIndex + 1) % 4);
                int contract = int.Parse(currentBid.Split(' ')[0]);
                string suit = currentBid.Split(' ')[1];
                if (isRedoubled) _ = 4;
                else if (isDoubled) _ = 2;

                int tricks = ShowInputDialog("Kaç el kazanıldı?");
                if (tricks >= contract + 6)
                {
                    int score = CalculateScore(contract, suit, tricks - (contract + 6), isDoubled, isRedoubled);
                    int teamIndex = (currentPlayerIndex + 1) % 2;
                    teamScores[teamIndex] += score;
                    MessageBox.Show($"Kontrat başarılı! Kazanılan puan: {score}");
                }
                else
                {
                    int penalty = CalculatePenalty(contract, tricks, isDoubled, isRedoubled);
                    int teamIndex = ((currentPlayerIndex + 1) % 2 + 1) % 2;
                    teamScores[teamIndex] += penalty;
                    MessageBox.Show($"Kontrat başarısız! Kaybedilen puan: {penalty}");
                }

                UpdateScoreLabel();
                bidPanel.Enabled = false;
                currentPhase = GamePhase.Scoring;
            }
            else if (passCount == 4 && currentBid == "Henüz teklif yok")
            {
                MessageBox.Show("Herkes pas geçti. Eller yeniden dağıtılacak.");
                ShuffleButton_Click(null, null);
            }
            else
            {
                NextPlayer();
            }
        }

        private void NextPlayer()
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % 4;
            UpdateBiddingLabels();
        }

        private void UpdateBiddingLabels()
        {
            currentPlayerLabel.Text = "Sıradaki Oyuncu: " + GetPlayerPosition(currentPlayerIndex);
            currentBidLabel.Text = "Mevcut Teklif: " + currentBid.ToString();
        }

        private static string GetPlayerPosition(int index)
        {
            string[] positions = ["Güney", "Batı", "Kuzey", "Doğu"];
            return positions[index];
        }

        private bool IsValidBid(string newBid)
        {
            if (currentBid == "Henüz teklif yok")
                return true;

            int currentLevel = int.Parse(currentBid.Split(' ')[0]);
            int newLevel = int.Parse(newBid.Split(' ')[0]);

            if (newLevel > currentLevel)
                return true;

            if (newLevel == currentLevel)
            {
                string[] suitOrder = ["Trefl", "Karo", "Kupa", "Pik", "NT"];
                string currentSuit = currentBid.Split(' ')[1];
                string newSuit = newBid.Split(' ')[1];

                return Array.IndexOf(suitOrder, newSuit) > Array.IndexOf(suitOrder, currentSuit);
            }

            return false;
        }

        private void DoubleButton_Click(object sender, EventArgs e)
        {
            if (!isDoubled && currentBid != "Henüz teklif yok")
            {
                isDoubled = true;
                doubleButton.Enabled = false;
                redoubleButton.Enabled = true;
                currentBid = "Kontra " + currentBid;
                UpdateBiddingLabels();
                NextPlayer();
            }
        }

        private void RedoubleButton_Click(object sender, EventArgs e)
        {
            if (isDoubled && !isRedoubled)
            {
                isRedoubled = true;
                redoubleButton.Enabled = false;
                currentBid = string.Concat("Sürkontra ", currentBid.AsSpan(7)); // "Kontra " kelimesini çıkar
                UpdateBiddingLabels();
                NextPlayer();
            }
        }

        private void UpdateScoreLabel()
        {
            scoreLabel.Text = $"Skor: K-G: {teamScores[0]} | D-B: {teamScores[1]}";
        }

        private static int ShowInputDialog(string text)
        {
            Form prompt = new()
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = text,
                StartPosition = FormStartPosition.CenterScreen
            };
            NumericUpDown inputBox = new()
            {
                Left = 50,
                Top = 20,
                Width = 200,
                Minimum = 0,
                Maximum = 13
            };
            Button confirmation = new()
            {
                Text = "Tamam",
                Left = 100,
                Width = 100,
                Top = 70,
                DialogResult = DialogResult.OK
            };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(inputBox);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? (int)inputBox.Value : 0;
        }

        private int CalculateScore(int contract, string suit, int overtricks, bool doubled, bool redoubled)
        {
            bool isVulnerable = IsDeclarerVulnerable();

            int baseScore;
            // Temel puan hesaplama
            if (suit == "NT")
                baseScore = 40 + (contract - 1) * 30;
            else if (suit == "Pik" || suit == "Kupa")
                baseScore = 30 * contract;
            else
                baseScore = 20 * contract;

            // Kontra/Sürkontra çarpanı
            if (redoubled) baseScore *= 4;
            else if (doubled) baseScore *= 2;

            // Fazladan lövelerin puanı
            int overtrickScore = overtricks * (doubled ? isVulnerable ? 200 : 100 : 30);
            if (redoubled) overtrickScore *= 2;

            // Game ve Slam bonusları
            if (baseScore >= 100)
                baseScore += isVulnerable ? 500 : 300; // Game bonus
            else
                baseScore += 50; // Part score

            if (contract == 6) baseScore += isVulnerable ? 750 : 500; // Small slam
            if (contract == 7) baseScore += isVulnerable ? 1500 : 1000; // Grand slam

            return baseScore + overtrickScore;
        }

        private int CalculatePenalty(int contract, int tricks, bool doubled, bool redoubled)
        {
            int undertricks = contract + 6 - tricks;
            int penalty = 0;
            bool isVulnerable = IsDeclarerVulnerable();

            if (!doubled)
            {
                penalty = undertricks * (isVulnerable ? 100 : 50);
            }
            else
            {
                if (isVulnerable)
                {
                    penalty = undertricks * 300;
                    if (redoubled) penalty *= 2;
                }
                else
                {
                    // İlk batış
                    penalty += 100;
                    undertricks--;

                    // Sonraki batışlar
                    if (undertricks > 0)
                    {
                        penalty += undertricks * 200;
                    }

                    if (redoubled) penalty *= 2;
                }
            }

            return penalty;
        }

        private bool IsDeclarerVulnerable()
        {
            bool isNorthSouth = currentPlayerIndex % 2 == 0;
            return currentVulnerability == Vulnerability.Both ||
                   isNorthSouth && currentVulnerability == Vulnerability.NorthSouth ||
                   !isNorthSouth && currentVulnerability == Vulnerability.EastWest;
        }

        private void AddToHistory(string contract, int tricks, int score)
        {
            var entry = new GameHistory
            {
                Contract = contract,
                Declarer = GetPlayerPosition(currentPlayerIndex),
                Tricks = tricks,
                Score = score,
                Vulnerability = currentVulnerability,
                PlayedAt = DateTime.Now
            };

            gameHistory.Add(entry);
            UpdateHistoryGrid();
        }

        private void UpdateHistoryGrid()
        {
            historyGrid.Rows.Clear();
            foreach (var entry in gameHistory)
            {
                historyGrid.Rows.Add(
                    $"{entry.Contract} by {entry.Declarer}",
                    entry.Score.ToString("+#;-#;0")
                );
            }
        }

        private void ShowStatsButton_Click(object sender, EventArgs e)
        {
            var statsForm = new Form
            {
                Text = "Detaylı İstatistikler",
                Size = new Size(600, 400),
                StartPosition = FormStartPosition.CenterParent
            };

            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Genel İstatistikler Sekmesi
            var generalTab = new TabPage("Genel");
            var generalStats = CreateGeneralStatsControl;
            generalTab.Controls.Add(generalStats);

            // Oyuncu İstatistikleri Sekmesi
            var playerTab = new TabPage("Oyuncular");
            var playerStats = CreatePlayerStatsControl;
            playerTab.Controls.Add(playerStats);

            // Turnuva İstatistikleri Sekmesi
            var tournamentTab = new TabPage("Turnuvalar");
            var tournamentStats = CreateTournamentStatsControl;
            tournamentTab.Controls.Add(tournamentStats);

            tabControl.TabPages.AddRange(new TabPage[] { generalTab, playerTab, tournamentTab });
            statsForm.Controls.Add(tabControl);

            statsForm.ShowDialog();
        }

        private Control CreateGeneralStatsControl
        {
            get
            {
                var panel = new Panel { Dock = DockStyle.Fill };
                var stats = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    Dock = DockStyle.Fill,
                    Font = new Font("Consolas", 10)
                };

                // Mevcut istatistiklere ek olarak
                var vulnerableGames = gameHistory.Count(h => h.Vulnerability != Vulnerability.None);
                var doubledContracts = gameHistory.Count(h => h.Contract.Contains("Kontra"));
                var redoubledContracts = gameHistory.Count(h => h.Contract.Contains("Sürkontra"));

                stats.AppendText(GetExistingStatsText());
                stats.AppendText($"\r\nVulnerable Eller: {vulnerableGames}\r\n");
                stats.AppendText($"Kontra Edilen Kontratlar: {doubledContracts}\r\n");
                stats.AppendText($"Sürkontra Edilen Kontratlar: {redoubledContracts}\r\n");

                panel.Controls.Add(stats);
                return panel;
            }
        }

        private Control CreatePlayerStatsControl
        {
            get
            {
                var grid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    ReadOnly = true
                };

                grid.Columns.AddRange(
                [
                    new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "İsim" },
            new DataGridViewTextBoxColumn { Name = "Games", HeaderText = "Oyun" },
            new DataGridViewTextBoxColumn { Name = "Made", HeaderText = "Yapılan" },
            new DataGridViewTextBoxColumn { Name = "Defeated", HeaderText = "Batılan" },
            new DataGridViewTextBoxColumn { Name = "Slams", HeaderText = "Slam" },
            new DataGridViewTextBoxColumn { Name = "Average", HeaderText = "Ortalama" }
                    ]);

                foreach (var stat in playerStats.Values)
                {
                    grid.Rows.Add(
                        stat.PlayerName,
                        stat.GamesPlayed,
                        stat.ContractsMade,
                        stat.ContractsDefeated,
                        stat.SlamsMade,
                        stat.AverageScore.ToString("F1")
                    );
                }

                return grid;
            }
        }

        private Control CreateTournamentStatsControl
        {
            get
            {
                var grid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    ReadOnly = true
                };

                var tournamentGames = gameHistory.Where(h => h.IsTournamentGame)
                    .GroupBy(h => h.TournamentName)
                    .Select(g => new
                    {
                        Tournament = g.Key,
                        Games = g.Count(),
                        AverageScore = g.Average(h => h.Score),
                        TopScore = g.Max(h => h.Score),
                        Date = g.Min(h => h.PlayedAt)
                    });

                grid.Columns.AddRange(
                [
                    new DataGridViewTextBoxColumn { Name = "Tournament", HeaderText = "Turnuva" },
            new DataGridViewTextBoxColumn { Name = "Games", HeaderText = "El Sayısı" },
            new DataGridViewTextBoxColumn { Name = "Average", HeaderText = "Ort. Skor" },
            new DataGridViewTextBoxColumn { Name = "Top", HeaderText = "En Yüksek" },
            new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Tarih" }
                    ]);

                foreach (var t in tournamentGames)
                {
                    grid.Rows.Add(
                        t.Tournament,
                        t.Games,
                        t.AverageScore.ToString("F1"),
                        t.TopScore,
                        t.Date.ToShortDateString()
                    );
                }

                return grid;
            }
        }

        private void ShowPlayerNamesDialog(object sender, EventArgs e)
        {
            var dialog = new Form
            {
                Text = "Oyuncu İsimleri",
                Size = new Size(300, 200),
                StartPosition = FormStartPosition.CenterParent
            };

            playerNameBoxes = new TextBox[4];
            string[] positions = ["Güney", "Batı", "Kuzey", "Doğu"];

            for (int i = 0; i < 4; i++)
            {
                var label = new Label
                {
                    Text = positions[i],
                    Location = new Point(10, 20 + i * 30),
                    AutoSize = true
                };

                playerNameBoxes[i] = new TextBox
                {
                    Location = new Point(80, 20 + i * 30),
                    Size = new Size(150, 20)
                };

                dialog.Controls.AddRange(new Control[] { label, playerNameBoxes[i] });
            }

            var okButton = new Button
            {
                Text = "Tamam",
                DialogResult = DialogResult.OK,
                Location = new Point(100, 120)
            };
            dialog.Controls.Add(okButton);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                UpdatePlayerStatistics();
            }
        }

        private void UpdatePlayerStatistics()
        {
            playerStats ??= new Dictionary<string, PlayerStatistics>();

            for (int i = 0; i < 4; i++)
            {
                string name = playerNameBoxes[i].Text;
                if (!string.IsNullOrEmpty(name) && !playerStats.ContainsKey(name))
                {
                    playerStats[name] = new PlayerStatistics
                    {
                        PlayerName = name,
                        MostPlayedPosition = (Position)i
                    };
                }
            }
        }

        private void SaveGameHistory()
        {
            try
            {
                // XML olarak kaydet
                var serializer = new XmlSerializer(typeof(List<GameHistory>));
                using (var writer = new StreamWriter("game_history.xml"))
                {
                    serializer.Serialize(writer, gameHistory);
                }

                // JSON olarak kaydet
                string json = JsonConvert.SerializeObject(gameHistory, Formatting.Indented);
                File.WriteAllText("game_history.json", json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kayıt hatası: {ex.Message}");
            }
        }

        private void LoadGameHistory()
        {
            try
            {
                if (File.Exists("game_history.xml"))
                {
                    var serializer = new XmlSerializer(typeof(List<GameHistory>));
                    using (var reader = new StreamReader("game_history.xml"))
                    {
                        gameHistory = (List<GameHistory>)serializer.Deserialize(reader);
                    }
                    UpdateHistoryGrid();
                    UpdateScoreChart();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yükleme hatası: {ex.Message}");
            }
        }

        private void UpdateScoreChart()
        {
            var series = scoreChart.Series[0];
            series.Points.Clear();

            foreach (var game in gameHistory)
            {
                series.Points.AddY(game.Score);
            }
        }

        private void ShowHandAnalysis()
        {
            if (playerHands.Count == 0 || currentPlayerIndex >= playerHands.Count)
            {
                MessageBox.Show("Analiz edilecek el bulunamadı.");
                return;
            }

            currentHandAnalysis = new HandAnalysis(playerHands[currentPlayerIndex]);

            var analysisForm = new Form
            {
                Text = "El Analizi",
                Size = new Size(500, 400),
                StartPosition = FormStartPosition.CenterParent
            };

            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Puan analizi sekmesi
            var pointsTab = new TabPage("Puanlar");
            var pointsText = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill
            };
            pointsText.AppendText($"Yüksek Kart Puanı: {currentHandAnalysis.HighCardPoints}\r\n");
            pointsText.AppendText($"Dağılım Puanı: {currentHandAnalysis.DistributionPoints}\r\n");
            pointsText.AppendText($"Toplam Puan: {currentHandAnalysis.HighCardPoints + currentHandAnalysis.DistributionPoints}\r\n");
            pointsTab.Controls.Add(pointsText);

            // Dağılım analizi sekmesi
            var distributionTab = new TabPage("Dağılım");
            var distributionText = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill
            };
            foreach (var suit in currentHandAnalysis.SuitLengths)
            {
                distributionText.AppendText($"{suit.Key}: {suit.Value} kart\r\n");
            }
            distributionTab.Controls.Add(distributionText);

            // Öneriler sekmesi
            var suggestionsTab = new TabPage("Öneriler");
            var suggestionsText = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill
            };
            foreach (var suggestion in currentHandAnalysis.Suggestions)
            {
                suggestionsText.AppendText($"• {suggestion}\r\n");
            }
            suggestionsTab.Controls.Add(suggestionsText);

            tabControl.TabPages.AddRange(new TabPage[] { pointsTab, distributionTab, suggestionsTab });
            analysisForm.Controls.Add(tabControl);
            analysisForm.ShowDialog();
        }

        private void ShowProfileManager()
        {
            var profileForm = new Form
            {
                Text = "Profil Yönetimi",
                Size = new Size(400, 500),
                StartPosition = FormStartPosition.CenterParent
            };

            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Profil bilgileri sekmesi
            var infoTab = new TabPage("Bilgiler");
            var infoPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4
            };

            infoPanel.Controls.Add(new Label { Text = "İsim:" }, 0, 0);
            var nameBox = new TextBox();
            infoPanel.Controls.Add(nameBox, 1, 0);

            infoPanel.Controls.Add(new Label { Text = "E-posta:" }, 0, 1);
            var emailBox = new TextBox();
            infoPanel.Controls.Add(emailBox, 1, 1);

            infoPanel.Controls.Add(new Label { Text = "Tercih Edilen Pozisyon:" }, 0, 2);
            var positionBox = new ComboBox();
            positionBox.Items.AddRange(["Güney", "Batı", "Kuzey", "Doğu"]);
            infoPanel.Controls.Add(positionBox, 1, 2);

            var saveButton = new Button { Text = "Kaydet", Dock = DockStyle.Bottom };
            saveButton.Click += (s, e) => SaveProfile(nameBox.Text, emailBox.Text, positionBox.Text);
            infoPanel.Controls.Add(saveButton, 1, 3);

            infoTab.Controls.Add(infoPanel);

            // Başarılar sekmesi
            var achievementsTab = new TabPage("Başarılar");
            var achievementsList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details
            };
            achievementsList.Columns.Add("Başarı", 150);
            achievementsList.Columns.Add("Tarih", 100);
            achievementsTab.Controls.Add(achievementsList);

            tabControl.TabPages.AddRange(new TabPage[] { infoTab, achievementsTab });
            profileForm.Controls.Add(tabControl);
            profileForm.ShowDialog();
        }

        private void SaveProfile(string name, string email, string position)
        {
            if (!playerProfiles.TryGetValue(name, out PlayerProfile? profile))
            {
                playerProfiles[name] = new PlayerProfile
                {
                    Name = name,
                    Email = email,
                    PreferredPosition = position
                };
            }
            else
            {
                profile.Email = email;
                profile.PreferredPosition = position;
            }

            MessageBox.Show("Profil kaydedildi.");
        }

        private async Task ShowLeaderboard()
        {
            var leaderboard = await onlineScoreboard.GetLeaderboard();

            var leaderboardForm = new Form
            {
                Text = "Skor Tablosu",
                Size = new Size(400, 500),
                StartPosition = FormStartPosition.CenterParent
            };

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };

            grid.Columns.AddRange(
            [
                new DataGridViewTextBoxColumn { Name = "Player", HeaderText = "Oyuncu" },
            new DataGridViewTextBoxColumn { Name = "Score", HeaderText = "Skor" },
            new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Tarih" }
            ]);

            foreach (var game in leaderboard)
            {
                grid.Rows.Add(
                    game.Declarer,
                    game.Score,
                    game.PlayedAt.ToShortDateString()
                );
            }

            leaderboardForm.Controls.Add(grid);
            leaderboardForm.ShowDialog();
        }

        private async void ShowTutorial()
        {
            var useAnimated = MessageBox.Show(
                "Animasyonlu öğreticiyi görmek ister misiniz?",
                "Öğretici Seçimi",
                MessageBoxButtons.YesNo) == DialogResult.Yes;

            if (useAnimated)
                await animatedTutorial.ShowTutorial();
            else
                tutorialMode.ShowTutorial();
        }

        private void ShowDetailedStatistics()
        {
            statisticsGraphs.ShowGraphs();
        }

        private void ShowGameRecordings()
        {
            var recordingsForm = new Form
            {
                Text = "Oyun Kayıtları",
                Size = new Size(600, 400),
                StartPosition = FormStartPosition.CenterParent
            };

            var listBox = new ListBox
            {
                Dock = DockStyle.Left,
                Width = 200
            };
            listBox.Items.AddRange([.. gameRecorder.GetRecordingsList()]);

            var detailsBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true
            };

            listBox.SelectedIndexChanged += (s, e) =>
            {
                if (listBox.SelectedItem != null)
                {
                    var recording = gameRecorder.LoadRecording(listBox.SelectedItem.ToString());
                    detailsBox.Clear();
                    foreach (var record in recording)
                    {
                        detailsBox.AppendText($"{record.Timestamp:HH:mm:ss} - {record.Player}: {record.Action}\n");
                        detailsBox.AppendText($"Detaylar: {record.Details}\n\n");
                    }
                }
            };

            recordingsForm.Controls.AddRange(new Control[] { listBox, detailsBox });
            recordingsForm.Show();
        }

        private static void ShowTournamentManager()
        {
            var tournamentForm = new Form
            {
                Text = "Turnuva Yönetimi",
                Size = new Size(800, 600),
                StartPosition = FormStartPosition.CenterParent
            };

            var tabControl = new TabControl { Dock = DockStyle.Fill };

            // Turnuva oluşturma sekmesi
            var createTab = new TabPage("Yeni Turnuva");
            var createPanel = CreateTournamentCreationPanel();
            createTab.Controls.Add(createPanel);

            // Aktif turnuvalar sekmesi
            var activeTab = new TabPage("Aktif Turnuvalar");
            var activePanel = CreateActiveTournamentsPanel();
            activeTab.Controls.Add(activePanel);

            tabControl.TabPages.AddRange(new TabPage[] { createTab, activeTab });
            tournamentForm.Controls.Add(tabControl);
            tournamentForm.Show();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // MainForm
            // 
            ClientSize = new Size(282, 253);
            Name = "MainForm";
            Load += MainForm_Load;
            ResumeLayout(false);

        }

        private bool ValidateGameState(GameState requiredState)
        {
            if (currentGameState != requiredState)
            {
                string message = requiredState switch
                {
                    GameState.Dealing => "Önce kartlar dağıtılmalı!",
                    GameState.Bidding => "Deklare aşamasında değilsiniz!",
                    GameState.Playing => "Oyun aşamasında değilsiniz!",
                    _ => "Geçersiz oyun durumu!"
                };
                MessageBox.Show(message);
                return false;
            }
            return true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}