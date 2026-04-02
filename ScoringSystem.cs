using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// One line in the scoring breakdown shown to the player.
/// </summary>
public class ScoreEntry
{
    public string Label;
    public int    Points;
    public bool   IsBonus;    // golden colour in UI
    public bool   IsPenalty;  // red colour in UI
}

/// <summary>
/// Full result returned after scoring a played hand.
/// </summary>
public class ScoreResult
{
    public int              Total;
    public List<ScoreEntry> Breakdown = new();
    public string           ComboName;  // e.g. "Low Three-of-a-Kind", "Run", …
}

/// <summary>
/// Pure scoring logic for Cloverpit.
/// No MonoBehaviour — call ScoreHand() from GameManager.
///
/// Rules summary
/// ─────────────
/// • High cards  (7–A)  → face value as points
/// • Low cards   (2–6)  → face value × 2
/// • All-low 3-of-a-kind or run (no decay involved) → base^2
/// • All-low combo with ≥1 decayed card             → base^1.5
/// • Standard combos (mixed/high)  3oak             → base × 1.5
/// • Standard run                                   → base × 1.3
/// • Fully decayed card played                      → –face value (penalty)
/// • Decayed card's effective value is used for combo matching
/// </summary>
public static class ScoringSystem
{
    // ─────────────────────────────────────────────────────────────────────────
    public static ScoreResult ScoreHand(List<CloverpitCardData> cards)
    {
        var result = new ScoreResult();
        if (cards == null || cards.Count == 0) return result;

        // ── 1. Per-card base points ──────────────────────────────────────────
        int baseTotal   = 0;
        bool allLow     = cards.All(c => c.IsLow);
        bool anyDecayed = cards.Any(c => c.DecayAmount > 0 && !c.IsFullyDecayed);

        foreach (var card in cards)
        {
            if (card.IsFullyDecayed)
            {
                // Penalty handled separately below
                continue;
            }

            int pts = card.IsLow ? card.EffectiveValue * 2 : card.EffectiveValue;
            baseTotal += pts;

            string label = card.IsLow
                ? $"{card.RankDisplay()}{card.SuitDisplay()} (low ×2)"
                : $"{card.RankDisplay()}{card.SuitDisplay()}";

            if (card.DecayAmount > 0)
                label += $" [decayed -{card.DecayAmount}]";

            result.Breakdown.Add(new ScoreEntry { Label = label, Points = pts });
        }

        // ── 2. Detect combo ──────────────────────────────────────────────────
        var liveCards      = cards.Where(c => !c.IsFullyDecayed).ToList();
        var effectiveVals  = liveCards.Select(c => c.EffectiveValue).ToList();

        bool isThreeOfAKind = liveCards.Count >= 3
            && effectiveVals.Distinct().Count() == 1;

        bool isRun = liveCards.Count >= 3 && IsConsecutiveRun(effectiveVals);

        float comboExponent   = 1f;
        float comboMultiplier = 1f;
        bool  usesExponent    = false;

        if (isThreeOfAKind)
        {
            result.ComboName = allLow ? "Low Three-of-a-Kind" : "Three-of-a-Kind";

            if (allLow)
            {
                usesExponent   = true;
                comboExponent  = anyDecayed ? 1.5f : 2f;
                result.ComboName += anyDecayed ? " (decayed)" : "";
            }
            else
            {
                comboMultiplier = 1.5f;
            }
        }
        else if (isRun)
        {
            result.ComboName = allLow ? "Low Run" : "Run";

            if (allLow)
            {
                usesExponent  = true;
                comboExponent = anyDecayed ? 1.5f : 2f;
                result.ComboName += anyDecayed ? " (decayed)" : "";
            }
            else
            {
                comboMultiplier = 1.3f;
            }
        }

        // ── 3. Apply combo to base total ─────────────────────────────────────
        int comboTotal;
        if (usesExponent && baseTotal > 0)
        {
            comboTotal = Mathf.RoundToInt(Mathf.Pow(baseTotal, comboExponent));
            int bonus  = comboTotal - baseTotal;

            result.Breakdown.Add(new ScoreEntry
            {
                Label   = $"{result.ComboName} (^{comboExponent:0.#})",
                Points  = bonus,
                IsBonus = true
            });
        }
        else if (comboMultiplier > 1f)
        {
            comboTotal = Mathf.RoundToInt(baseTotal * comboMultiplier);
            int bonus  = comboTotal - baseTotal;

            result.Breakdown.Add(new ScoreEntry
            {
                Label   = $"{result.ComboName} (×{comboMultiplier:0.##})",
                Points  = bonus,
                IsBonus = true
            });
        }
        else
        {
            comboTotal = baseTotal;
        }

        // ── 4. Fully-decayed penalties ───────────────────────────────────────
        int penalty = 0;
        foreach (var card in cards.Where(c => c.IsFullyDecayed))
        {
            int pen = card.FaceValue;
            penalty += pen;

            result.Breakdown.Add(new ScoreEntry
            {
                Label     = $"{card.RankDisplay()}{card.SuitDisplay()} FULLY DECAYED",
                Points    = -pen,
                IsPenalty = true
            });
        }

        result.Total = comboTotal - penalty;
        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static bool IsConsecutiveRun(List<int> values)
    {
        if (values.Count < 2) return false;
        var sorted = values.OrderBy(v => v).ToList();
        for (int i = 1; i < sorted.Count; i++)
            if (sorted[i] != sorted[i - 1] + 1) return false;
        return true;
    }
}
