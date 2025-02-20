using System.Text.Json.Serialization;

public enum Suit
{
    Kupa,    // Hearts
    Karo,    // Diamonds
    Sinek,   // Clubs
    Maça     // Spades
}

public enum Rank
{
    İki = 2,
    Üç,
    Dört,
    Beş,
    Altı,
    Yedi,
    Sekiz,
    Dokuz,
    On,
    Vale,    // Jack
    Kız,     // Queen
    Papaz,   // King
    As       // Ace
}

public class Card
{
    [JsonPropertyName("suit")]
    public Suit Suit { get; private set; }

    [JsonPropertyName("rank")]
    public Rank Rank { get; private set; }

    public Card(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;
    }

    public override string ToString()
    {
        return $"{Rank} {Suit}";
    }
}