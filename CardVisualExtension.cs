using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardVisualExtension : MonoBehaviour
{
    [Header("Colours")]
    public Color overlayColor = new Color(0.31f, 0.12f, 0f, 1f);

    [Range(0f, 1f)]
    public float maxOverlayAlpha = 0.7f;

    private CardVisual        _cardVisual;
    private CloverpitCardData _data;

    private Image      _decayOverlay;
    private TMP_Text   _decayCounterText;
    private GameObject _lowBadge;
    private GameObject _forcedBadge;

    private void Awake()
    {
        _cardVisual = GetComponent<CardVisual>();
    }

    private void Start()
    {
        // CardVisual.Initialize() is called from Card.Start()
        // Both Start() methods run in the same frame, order not guaranteed
        // So we subscribe to be called once Initialize sets parentCard
        if (_cardVisual == null)
        {
            enabled = false; // no CardVisual on this object, stop running
            return;
        }

        // Try immediately in case Card.Start already ran
        TryInit();
    }

    // Called by DeckManager after decay, or can be called anytime
    public void RefreshVisuals()
    {
        if (_data == null) return;
        RefreshDecayOverlay();
        RefreshDecayCounter();
        RefreshForcedBadge();
        RefreshLowBadge();
    }

    // CardVisual calls Initialize() which sets parentCard — we poll once per
    // frame until it's set, then stop polling immediately
    private void Update()
    {
        if (TryInit())
            enabled = false; // disable Update — job done, no more per-frame cost
    }

    private bool TryInit()
    {
        if (_cardVisual == null) return false;
        if (_cardVisual.parentCard == null) return false;

        _data = _cardVisual.parentCard.GetComponent<CloverpitCardData>();
        if (_data == null) return false; // card doesn't have our component, skip silently

        FindUIRefs();
        RefreshVisuals();
        return true;
    }

    private void FindUIRefs()
    {
        _decayOverlay     = FindChild<Image>("DecayOverlay");
        _decayCounterText = FindChild<TMP_Text>("DecayCounterText");
        _lowBadge         = FindChildObject("LowBadge");
        _forcedBadge      = FindChildObject("ForcedBadge");

        if (_decayOverlay != null)
        {
            Color c = overlayColor;
            c.a = 0f;
            _decayOverlay.color = c;
        }

        if (_forcedBadge      != null) _forcedBadge.SetActive(false);
        if (_decayCounterText != null) _decayCounterText.gameObject.SetActive(false);
        if (_lowBadge         != null) _lowBadge.SetActive(false);
    }

    private void RefreshDecayOverlay()
    {
        if (_decayOverlay == null) return;
        float pct = _data.FaceValue > 0
            ? Mathf.Clamp01((float)_data.DecayAmount / _data.FaceValue)
            : 0f;

        Color c = overlayColor;
        c.a = pct * maxOverlayAlpha;
        _decayOverlay.color = c;

        var rt = _decayOverlay.rectTransform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, pct);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void RefreshDecayCounter()
    {
        if (_decayCounterText == null) return;
        bool show = _data.DecayAmount > 0;
        _decayCounterText.gameObject.SetActive(show);
        if (show) _decayCounterText.text = $"-{_data.DecayAmount}";
    }

    private void RefreshForcedBadge()
    {
        if (_forcedBadge == null) return;
        _forcedBadge.SetActive(_data.IsForcedPlay);
    }

    private void RefreshLowBadge()
    {
        if (_lowBadge == null) return;
        _lowBadge.SetActive(_data.IsLow && !_data.IsFullyDecayed);
    }

    private T FindChild<T>(string childName) where T : Component
    {
        var t = FindDeep(transform, childName);
        return t != null ? t.GetComponent<T>() : null;
    }

    private GameObject FindChildObject(string childName)
    {
        var t = FindDeep(transform, childName);
        return t != null ? t.gameObject : null;
    }

    private static Transform FindDeep(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindDeep(child, name);
            if (result != null) return result;
        }
        return null;
    }
}








