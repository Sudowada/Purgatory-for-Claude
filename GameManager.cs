using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Cloverpit turn loop. HorizontalCardHolder handles all visual hand management.
/// GameManager handles selection rules, scoring, and decay.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("References")]
    public DeckManager deckManager;

    [Header("Rules")]
    public int cardsPerTurn = 3;

    [Header("Events")]
    public UnityEvent<ScoreResult>             OnHandScored;
    public UnityEvent<int>                     OnScoreChanged;
    public UnityEvent<int>                     OnTurnChanged;
    public UnityEvent<List<CloverpitCardData>> OnDecayApplied;

    public int TotalScore { get; private set; }
    public int TurnNumber { get; private set; } = 1;

    private List<CloverpitCardData> _selected       = new();
    private bool                    _playInProgress = false;

    private void Start()
    {
        OnTurnChanged?.Invoke(TurnNumber);
    }

    // ── Selection ─────────────────────────────────────────────────────────────

    public void OnCardSelectEvent(Card card, bool isSelected)
    {
        if (_playInProgress) return;

        var data = card.GetComponent<CloverpitCardData>();
        if (data == null) return;

        if (isSelected)
        {
            if (_selected.Count >= cardsPerTurn)
            {
                card.Deselect();
                return;
            }
            _selected.Add(data);
        }
        else
        {
            if (data.IsForcedPlay)
            {
                // Snap forced card back to selected
                card.selected = true;
                card.transform.localPosition = new Vector3(0, card.selectionOffset, 0);
                return;
            }
            _selected.Remove(data);
        }
    }

    // ── Play ──────────────────────────────────────────────────────────────────

    public void PlaySelectedCards()
    {
        if (_playInProgress) return;
        if (!CanPlay) return;
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        _playInProgress = true;

        // Auto-add forced cards
        foreach (var card in deckManager.Hand.Where(c => c.IsForcedPlay))
            if (!_selected.Contains(card))
                _selected.Add(card);

        if (_selected.Count == 0)
        {
            _playInProgress = false;
            yield break;
        }

        // Score
        var result  = ScoringSystem.ScoreHand(_selected);
        TotalScore += result.Total;
        OnHandScored?.Invoke(result);
        OnScoreChanged?.Invoke(TotalScore);

        yield return new WaitForSeconds(0.4f);

        // Deselect played cards visually, remove from hand tracking
        foreach (var data in _selected)
        {
            var card = data.GetComponent<Card>();
            if (card != null) card.Deselect();
        }

        deckManager.RemoveFromHand(new List<CloverpitCardData>(_selected));
        _selected.Clear();

        // Decay pass
        var decayed = deckManager.ApplyDecayToHand();
        OnDecayApplied?.Invoke(decayed);

        yield return new WaitForSeconds(0.2f);

        // Assign new deck data to any unassigned cards
        deckManager.FillHand();

        TurnNumber++;
        OnTurnChanged?.Invoke(TurnNumber);

        _playInProgress = false;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    public bool CanPlay =>
        !_playInProgress &&
        (_selected.Count > 0 || deckManager.Hand.Any(c => c.IsForcedPlay));

    public IEnumerable<CloverpitCardData> ForcedCards =>
        deckManager.Hand.Where(c => c.IsForcedPlay);
}