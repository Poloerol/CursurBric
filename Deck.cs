using System;
using System.Collections.Generic;

public class Deck
{
    private readonly List<Card> cards;
    private readonly Random random;

    public Deck()
    {
        random = new Random();
        cards = [];
        
        // Tüm kartları oluştur
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                cards.Add(new Card(suit, rank));
            }
        }
    }

    public void Shuffle()
    {
        // Fisher-Yates karıştırma algoritması
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (cards[j], cards[i]) = (cards[i], cards[j]);
        }
    }

    public Card DealCard()
    {
        if (cards.Count == 0)
            throw new InvalidOperationException("Destede kart kalmadı!");

        Card card = cards[0];
        cards.RemoveAt(0);
        return card;
    }

    public List<Card> DealHand(int cardCount)
    {
        if (cardCount > cards.Count)
            throw new InvalidOperationException("Destede yeterli kart yok!");

        List<Card> hand = [];
        for (int i = 0; i < cardCount; i++)
        {
            hand.Add(DealCard());
        }
        return hand;
    }
} 