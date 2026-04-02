using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    [Header("Text References")]
    public TextMeshProUGUI rankTopLeft;
    public TextMeshProUGUI suitTopLeft;
    public TextMeshProUGUI rankCentre;
    public TextMeshProUGUI suitCentre;
    public TextMeshProUGUI rankBottomRight;
    public TextMeshProUGUI suitBottomRight;

    [Header("Background")]
    public Image backgroundImage;

    [Header("Colors")]
    public Color redSuitColor   = new Color(0.91f, 0.25f, 0.25f);
    public Color blackSuitColor = new Color(0.83f, 0.77f, 0.63f);
    public Color cardFaceColor  = new Color(0.12f, 0.08f, 0.04f);

    public CardData Data { get; private set; }

    public void SetData(CardData data)
    {
        Data = data;
        if (data == null) return;

        Color col = data.IsRed ? redSuitColor : blackSuitColor;

        SetText(rankTopLeft,     data.RankLabel,  col);
        SetText(suitTopLeft,     data.SuitSymbol, col);
        SetText(rankCentre,      data.RankLabel,  col);
        SetText(suitCentre,      data.SuitSymbol, col);
        SetText(rankBottomRight, data.RankLabel,  col);
        SetText(suitBottomRight, data.SuitSymbol, col);

        if (backgroundImage)
            backgroundImage.color = cardFaceColor;
    }

    void SetText(TextMeshProUGUI tmp, string text, Color col)
    {
        if (tmp == null) return;
        tmp.text  = text;
        tmp.color = col;
    }
}