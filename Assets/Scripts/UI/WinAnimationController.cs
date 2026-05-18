using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays per-symbol win animations (glow, pulse, shake) after a winning spin.
///
/// Attach to the same GameObject as GameManager, or a dedicated FX object.
/// Link one GlowImage per reel slot — these sit on top of the symbol display.
/// </summary>
public class WinAnimationController : MonoBehaviour
{
    // ------------------------------------------------------------------
    // Inspector Fields
    // ------------------------------------------------------------------

    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Tooltip("One Image overlay per reel, used to flash the glow effect")]
    [SerializeField] private List<Image> reelGlowOverlays;

    [Header("Glow Settings")]
    [SerializeField] private Color jackpotGlowColor  = new Color(1f, 0.84f, 0f, 0.8f);   // gold
    [SerializeField] private Color normalWinGlowColor = new Color(1f, 1f, 0.3f, 0.6f);   // yellow
    [SerializeField] private Color scatterGlowColor   = new Color(0.3f, 1f, 0.5f, 0.7f); // green

    [Header("Pulse Settings")]
    [SerializeField] private float pulseDuration = 0.25f;
    [SerializeField] private float pulseScale    = 1.15f;
    [SerializeField] private int   pulseCount    = 3;

    // ------------------------------------------------------------------
    // Unity Lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        // Hide all glows at startup
        SetGlowsAlpha(0f);

        // Subscribe to game events
        gameManager.onSpinResult.AddListener(OnSpinResult);
        gameManager.onSpinStarted.AddListener(OnSpinStarted);
    }

    // ------------------------------------------------------------------
    // Event Handlers
    // ------------------------------------------------------------------

    private void OnSpinResult(int payout, WinType winType)
    {
        if (payout <= 0)
        {
            SetGlowsAlpha(0f);
            return;
        }

        Color glowColor = winType switch
        {
            WinType.AllWilds => jackpotGlowColor,
            WinType.Scatter  => scatterGlowColor,
            _                => normalWinGlowColor
        };

        StartCoroutine(PlayWinAnimation(glowColor, winType == WinType.AllWilds));
    }

    private void OnSpinStarted()
    {
        StopAllCoroutines();
        SetGlowsAlpha(0f);
        ResetReelScales();
    }

    // ------------------------------------------------------------------
    // Animation
    // ------------------------------------------------------------------

    private IEnumerator PlayWinAnimation(Color glowColor, bool isJackpot)
    {
        // Set glow color on all overlays
        foreach (Image glow in reelGlowOverlays)
            glow.color = glowColor;

        // Pulse the glow and scale the reels
        for (int pulse = 0; pulse < pulseCount; pulse++)
        {
            // Fade in glow
            yield return StartCoroutine(FadeGlows(0f, 1f, pulseDuration * 0.5f));

            // Scale up reel containers
            yield return StartCoroutine(ScaleReels(1f, pulseScale, pulseDuration * 0.5f));

            // Fade out glow
            yield return StartCoroutine(FadeGlows(1f, 0f, pulseDuration * 0.5f));

            // Scale back down
            yield return StartCoroutine(ScaleReels(pulseScale, 1f, pulseDuration * 0.5f));
        }

        // If jackpot: do a final sustained glow
        if (isJackpot)
        {
            yield return StartCoroutine(FadeGlows(0f, 0.6f, 0.3f));
            yield return new WaitForSeconds(1.5f);
            yield return StartCoroutine(FadeGlows(0.6f, 0f, 0.5f));
        }
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private IEnumerator FadeGlows(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            SetGlowsAlpha(alpha);
            yield return null;
        }
        SetGlowsAlpha(toAlpha);
    }

    private IEnumerator ScaleReels(float fromScale, float toScale, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.Lerp(fromScale, toScale, elapsed / duration);
            foreach (Image glow in reelGlowOverlays)
                glow.transform.localScale = Vector3.one * s;
            yield return null;
        }
        foreach (Image glow in reelGlowOverlays)
            glow.transform.localScale = Vector3.one * toScale;
    }

    private void SetGlowsAlpha(float alpha)
    {
        foreach (Image glow in reelGlowOverlays)
        {
            if (glow == null) continue;
            Color c = glow.color;
            c.a = alpha;
            glow.color = c;
        }
    }

    private void ResetReelScales()
    {
        foreach (Image glow in reelGlowOverlays)
        {
            if (glow != null)
                glow.transform.localScale = Vector3.one;
        }
    }
}
