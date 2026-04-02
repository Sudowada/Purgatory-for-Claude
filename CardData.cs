using UnityEngine;

// Suits
public enum Suit { Spades, Hearts, Diamonds, Clubs }

// Ranks — numeric value matches card points (Ace = 14)
public enum Rank
{
    Two   = 2,
    Three = 3,
    Four  = 4,
    Five  = 5,
    Six   = 6,
    Seven = 7,
    Eight = 8,
    Nine  = 9,
    Ten   = 10,
    Jack  = 11,
    Queen = 12,
    King  = 13,
    Ace   = 14
}

/// <summary>
/// Cloverpit-specific card data: rank, suit, decay state.
/// Attach alongside Mix&amp;Jam's Card component on the card prefab.
/// GameManager reads/writes this; Card.cs handles all drag/selection feel.
/// </summary>
[RequireComponent(typeof(Card))]
public class CloverpitCardData : MonoBehaviour
{
    [Header("Card Identity")]
    public Rank rank;
    public Suit suit;

    [Header("Decay State (Runtime)")]
    [SerializeField] private int _decayAmount = 0;

    // ── Derived helpers ──────────────────────────────────────────────────────

    /// <summary>Face value (2–14)</summary>
    public int FaceValue => (int)rank;

    /// <summary>Effective value after decay. Never goes below 0.</summary>
    public int EffectiveValue => Mathf.Max(0, FaceValue - _decayAmount);

    /// <summary>Low cards are 2–6.</summary>
    public bool IsLow => FaceValue <= 6;

    /// <summary>Card is red (Hearts or Diamonds).</summary>
    public bool IsRed => suit == Suit.Hearts || suit == Suit.Diamonds;

    /// <summary>Current accumulated decay.</summary>
    public int DecayAmount => _decayAmount;

    /// <summary>True when decay has reached or exceeded face value.</summary>
    public bool IsFullyDecayed => _decayAmount >= FaceValue;

    /// <summary>
    /// A fully decayed card must be played next turn — the GameManager
    /// marks this flag after the turn's decay pass.
    /// </summary>
    public bool IsForcedPlay   { get; set; } = false;

    /// <summary>True once DeckManager has assigned rank/suit to this card.</summary>
    public bool IsInitialized  { get; private set; } = false;

    // ── Mutation ─────────────────────────────────────────────────────────────

    /// <summary>Increment decay by 1. Marks card as forced if fully decayed.</summary>
    public void ApplyDecay()
    {
        _decayAmount++;
        if (IsFullyDecayed)
            IsForcedPlay = true;
    }

    /// <summary>Reset to a fresh un-decayed state (e.g. when drawn from deck).</summary>
    public void ResetDecay()
    {
        _decayAmount = 0;
        IsForcedPlay  = false;
        IsInitialized = true;
    }

    // ── Display helpers ──────────────────────────────────────────────────────

    public string RankDisplay()
    {
        return rank switch
        {
            Rank.Jack  => "J",
            Rank.Queen => "Q",
            Rank.King  => "K",
            Rank.Ace   => "A",
            _          => FaceValue.ToString()
        };
    }

    public string SuitDisplay()
    {
        return suit switch
        {
            Suit.Spades   => "♠",
            Suit.Hearts   => "♥",
            Suit.Diamonds => "♦",
            Suit.Clubs    => "♣",
            _             => "?"
        };
    }

    public string ToDisplayString() => $"{RankDisplay()}{SuitDisplay()} (decay:{_decayAmount})";
}