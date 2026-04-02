using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the deck data only — HorizontalCardHolder handles all spawning/layout.
/// On Start, finds all Card objects in the hand and assigns rank/suit to each.
/// After a play, reassigns data to refilled cards.
/// </summary>
public class DeckManager : MonoBehaviour
{
    [Header("References")]
    public HorizontalCardHolder handHolder;
    public GameManager gameManager;

    // ── Runtime state ─────────────────────────────────────────────────────────
    private List<(Rank rank, Suit suit)> _deck      = new();
    private int                          _drawIndex = 0;

    public List<CloverpitCardData> Hand { get; private set; } = new();
    public int CardsRemaining => Mathf.Max(0, _deck.Count - _drawIndex);

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        BuildAndShuffleDeck();
    }

    private void Start()
    {
        // Wait one frame for HorizontalCardHolder to finish spawning
        StartCoroutine(InitHand());
    }

    private System.Collections.IEnumerator InitHand()
    {
        yield return new WaitForEndOfFrame();
        AssignDeckDataToHand();
        SubscribeCards();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by GameManager after cards are played.
    /// Reassigns deck data to any cards that have no data yet (freshly recycled slots).
    /// </summary>
    public void FillHand()
    {
        AssignDeckDataToHand();
        SubscribeCards();
    }

    /// <summary>
    /// Apply one decay tick to up to 3 random cards in hand.
    /// Skips cards that were just played (no longer in hand list).
    /// </summary>
    public List<CloverpitCardData> ApplyDecayToHand()
    {
        var affected = new List<CloverpitCardData>();
        var indices  = Enumerable.Range(0, Hand.Count).ToList();
        Shuffle(indices);

        int count = Mathf.Min(3, indices.Count);
        for (int i = 0; i < count; i++)
        {
            var card = Hand[indices[i]];
            card.ApplyDecay();
            card.GetComponent<CardVisualExtension>()?.RefreshVisuals();
            affected.Add(card);
        }

        return affected;
    }

    /// <summary>Remove specific cards from our hand tracking (after they're played).</summary>
    public void RemoveFromHand(List<CloverpitCardData> played)
    {
        foreach (var card in played)
            Hand.Remove(card);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Finds all Cards in the hand holder and assigns deck data to any
    /// that don't have a rank/suit assigned yet.
    /// </summary>
    private void AssignDeckDataToHand()
    {
        if (handHolder == null) return;

        // Refresh hand list from HorizontalCardHolder
        var allCards = handHolder.cards;

        Hand.Clear();

        foreach (var card in allCards)
        {
            if (card == null) continue;

            var data = card.GetComponent<CloverpitCardData>();
            if (data == null)
            {
                data = card.gameObject.AddComponent<CloverpitCardData>();
            }

            // Only assign new deck data if not yet initialized
            if (data.IsInitialized)
            {
                // Already has data (possibly decayed) — keep it
                Hand.Add(data);
                continue;
            }

            // Fresh card — draw from deck
            if (_drawIndex < _deck.Count)
            {
                var (rank, suit) = _deck[_drawIndex++];
                data.rank = rank;
                data.suit = suit;
                data.ResetDecay(); // also sets IsInitialized = true
            }

            Hand.Add(data);
        }
    }

    private void SubscribeCards()
    {
        if (handHolder == null || gameManager == null) return;

        foreach (var card in handHolder.cards)
        {
            if (card == null) continue;

            // Remove first to avoid double-subscribing
            card.SelectEvent.RemoveListener(gameManager.OnCardSelectEvent);
            card.SelectEvent.AddListener(gameManager.OnCardSelectEvent);
        }
    }

    private void BuildAndShuffleDeck()
    {
        _deck.Clear();
        _drawIndex = 0;

        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
            foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
                _deck.Add((rank, suit));

        Shuffle(_deck);
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}