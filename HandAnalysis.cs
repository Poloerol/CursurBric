using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

public class HandAnalysis
{
    public List<Card> Hand { get; set; }
    public int HighCardPoints { get; set; }
    public int DistributionPoints { get; set; }
    public Dictionary<Suit, int> SuitLengths { get; set; }
    public List<string> Suggestions { get; set; }
    public Dictionary<string, double> BiddingProbabilities { get; set; }

    public HandAnalysis(List<Card> hand)
    {
        Hand = hand;
        SuitLengths = [];
        Suggestions = [];
        BiddingProbabilities = [];
        AnalyzeHand();
    }

    private void AnalyzeHand()
    {
        CalculatePoints();
        AnalyzeDistribution();
        GenerateSuggestions();
        CalculateBiddingProbabilities();
    }

    private void CalculatePoints()
    {
        HighCardPoints = 0;
        foreach (var card in Hand)
        {
            switch (card.Rank)
            {
                case Rank.As: HighCardPoints += 4; break;
                case Rank.Papaz: HighCardPoints += 3; break;
                case Rank.Kız: HighCardPoints += 2; break;
                case Rank.Vale: HighCardPoints += 1; break;
            }
        }
    }

    private void AnalyzeDistribution()
    {
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            int length = Hand.Count(c => c.Suit == suit);
            SuitLengths[suit] = length;
            
            // Dağılım puanları
            if (length == 0) DistributionPoints += 3; // Şikan
            else if (length == 1) DistributionPoints += 2; // Singleton
            else if (length == 2) DistributionPoints += 1; // Dubleton
        }
    }

    private void GenerateSuggestions()
    {
        // 5-3-3-2 dağılımı kontrolü
        if (SuitLengths.Values.OrderByDescending(x => x).SequenceEqual([5, 3, 3, 2]))
            Suggestions.Add("5-3-3-2 dağılımı: 1NT açışı düşünülebilir");

        // Uzun renk önerisi
        var longestSuit = SuitLengths.OrderByDescending(x => x.Value).First();
        if (longestSuit.Value >= 5)
            Suggestions.Add($"{longestSuit.Value} adet {longestSuit.Key}: Bu renkte açış düşünülebilir");

        // Yüksek kart puanı önerileri
        if (HighCardPoints >= 20)
            Suggestions.Add("20+ HCP: 2NT veya güçlü açış düşünülebilir");
        else if (HighCardPoints >= 15)
            Suggestions.Add("15-19 HCP: 1NT veya 1 seviyesinde açış düşünülebilir");
    }

    private void CalculateBiddingProbabilities()
    {
        // Basit olasılık hesaplamaları
        int totalPoints = HighCardPoints + DistributionPoints;
        
        BiddingProbabilities["1NT"] = totalPoints >= 15 && totalPoints <= 17 ? 0.8 : 0.2;
        BiddingProbabilities["Pas"] = totalPoints < 12 ? 0.9 : 0.1;
        
        foreach (var suit in SuitLengths)
        {
            if (suit.Value >= 5)
            {
                double prob = 0.6 + (suit.Value - 5) * 0.1;
                BiddingProbabilities[$"1 {suit.Key}"] = Math.Min(prob, 0.9);
            }
        }
    }
} 