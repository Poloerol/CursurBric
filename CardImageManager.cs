using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;


namespace CursurBric // Replace 'YourNamespace' with an appropriate name
{
    public static class CardImageManager
    {
        private static Dictionary<string, Image>? cardImages;
        private static Image? cardBack;
        private const string cardImagesPath = "card_images"; // Kart resimlerinin bulunduğu klasör

        static CardImageManager()
        {
            LoadCardImages();
        }

        private static void LoadCardImages()
        {
            cardImages = [];
            try
            {
                // Kart arkaları için resim
                cardBack = Image.FromFile(Path.Combine(cardImagesPath, "back.png"));

                // Her suit ve rank için resimleri yükle
                foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                {
                    foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                    {
                        string fileName = $"{rank}_{suit}.png";
                        string filePath = Path.Combine(cardImagesPath, fileName);
                        if (File.Exists(filePath))
                        {
                            cardImages[$"{rank}_{suit}"] = Image.FromFile(filePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kart resimleri yüklenirken hata: {ex.Message}");
            }
        }

        public static Image? GetCardImage(Card card)
        {
            string key = $"{card.Rank}_{card.Suit}";
            return cardImages?.TryGetValue(key, out Image? value) == true ? value : null;
        }

        public static Image? GetCardBack()
        {
            return cardBack;
        }

        public static bool ValidateCardImages()
        {
            if (!Directory.Exists(cardImagesPath))
            {
                MessageBox.Show("Kart resimleri klasörü (card_images) bulunamadı!");
                return false;
            }

            var missingCards = new List<string>();

            // Kart arkası kontrolü
            if (!File.Exists(Path.Combine(cardImagesPath, "back.png")))
            {
                missingCards.Add("back.png");
            }

            // Tüm kartların kontrolü
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    string fileName = $"{rank}_{suit}.png";
                    if (!File.Exists(Path.Combine(cardImagesPath, fileName)))
                    {
                        missingCards.Add(fileName);
                    }
                }
            }

            if (missingCards.Any())
            {
                MessageBox.Show($"Eksik kart resimleri:\n{string.Join("\n", missingCards)}");
                return false;
            }

            return true;
        }
    }
}