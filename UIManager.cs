using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Handles all HUD updates for Cloverpit.
/// Wire the UnityEvents in GameManager's inspector to the matching methods here.
///
/// Expected UI hierarchy (example):
///   Canvas
///   ├── ScoreText          (TMP_Text)
///   ├── TurnText           (TMP_Text)
///   ├── DeckCountText      (TMP_Text)
///   ├── PlayButton         (Button)
///   ├── ForcedWarning      (GameObject — shown when forced cards exist)
///   └── BreakdownPanel
///       ├── BreakdownTitle (TMP_Text)
///       ├── EntryContainer (Transform — breakdown rows instantiated here)
///       └── TotalText      (TMP_Text)
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public TMP_Text scoreText;
    public TMP_Text turnText;
    public TMP_Text deckCountText;
    public Button   playButton;
    public GameObject forcedWarning;

    [Header("Breakdown Panel")]
    public GameObject   breakdownPanel;
    public TMP_Text     breakdownTotalText;
    public Transform    entryContainer;
    public GameObject   breakdownEntryPrefab; // needs a TMP_Text child

    [Header("References")]
    public GameManager  gameManager;
    public DeckManager  deckManager;

    [Header("Colours")]
    public Color colourNormal  = new Color(0.91f, 0.87f, 0.78f);
    public Color colourBonus   = new Color(1f,    0.84f, 0f);
    public Color colourPenalty = new Color(1f,    0.27f, 0.27f);

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        if (breakdownPanel != null)
            breakdownPanel.SetActive(false);

        RefreshHUD(0);
    }

    private void Update()
    {
        // Keep play button state in sync each frame
        if (playButton != null)
            playButton.interactable = gameManager.CanPlay;

        // Show forced warning
        if (forcedWarning != null)
        {
            bool hasForced = false;
            foreach (var _ in gameManager.ForcedCards) { hasForced = true; break; }
            forcedWarning.SetActive(hasForced);
        }
    }

    // ── Event handlers (subscribe in Inspector via UnityEvents) ──────────────

    /// <summary>Called by GameManager.OnScoreChanged</summary>
    public void OnScoreChanged(int newScore)
    {
        RefreshHUD(newScore);
    }

    /// <summary>Called by GameManager.OnTurnChanged</summary>
    public void OnTurnChanged(int turn)
    {
        if (turnText != null)
            turnText.text = $"Turn {turn}";

        if (deckCountText != null)
            deckCountText.text = $"Deck: {deckManager.CardsRemaining}";
    }

    /// <summary>Called by GameManager.OnHandScored</summary>
    public void OnHandScored(ScoreResult result)
    {
        ShowBreakdown(result);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void OnPlayButtonClicked()
    {
        gameManager.PlaySelectedCards();
    }

    private void RefreshHUD(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
            scoreText.transform
                     .DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f)
                     .SetEase(Ease.OutElastic);
        }
    }

    private void ShowBreakdown(ScoreResult result)
    {
        if (breakdownPanel == null) return;

        // Clear previous entries
        foreach (Transform child in entryContainer)
            Destroy(child.gameObject);

        breakdownPanel.SetActive(true);

        // Spawn one row per breakdown entry
        foreach (var entry in result.Breakdown)
        {
            if (breakdownEntryPrefab == null) break;

            var row  = Instantiate(breakdownEntryPrefab, entryContainer);
            var txt  = row.GetComponentInChildren<TMP_Text>();
            if (txt == null) continue;

            string sign   = entry.Points >= 0 ? "+" : "";
            txt.text  = $"{entry.Label}  {sign}{entry.Points}";
            txt.color = entry.IsPenalty ? colourPenalty
                      : entry.IsBonus   ? colourBonus
                      : colourNormal;
        }

        // Total line
        if (breakdownTotalText != null)
        {
            string sign = result.Total >= 0 ? "+" : "";
            breakdownTotalText.text  = $"{sign}{result.Total} pts";
            breakdownTotalText.color = result.Total >= 0 ? colourBonus : colourPenalty;

            breakdownTotalText.transform
                .DOPunchScale(Vector3.one * 0.25f, 0.35f, 6, 0.5f);
        }

        // Auto-hide after a few seconds
        CancelInvoke(nameof(HideBreakdown));
        Invoke(nameof(HideBreakdown), 3.5f);
    }

    private void HideBreakdown()
    {
        if (breakdownPanel != null)
            breakdownPanel.SetActive(false);
    }
}
