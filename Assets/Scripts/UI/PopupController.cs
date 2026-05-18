using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls two popup panels:
///
///   1. WIN POPUP  — shown on any winning spin
///      Background: popup.png (1730×873, RGBA)
///      Contains: win amount label, symbol icon, close button
///
///   2. CONFIRM POPUP — Yes/No dialog (e.g. "Play again? / Quit?")
///      Background: popup.png (reused)
///      Buttons: Yes_No_Btn.png sliced into Left (Yes) and Right (No) halves
///
/// HOW TO SLICE Yes_No_Btn.png:
///   Texture Type: Sprite (2D and UI) → Sprite Mode: Multiple
///   Sprite Editor → Slice → Grid By Cell Size → 495 × 690
///   You'll get two sprites: Yes_No_Btn_0 (Yes, left) and Yes_No_Btn_1 (No, right)
///
/// HIERARCHY:
///   Canvas
///   └── WinPopup (CanvasGroup, alpha=0 hidden)
///       ├── PopupBackground  (Image → popup.png)
///       ├── WinAmountLabel   (TMP_Text)
///       ├── WinSymbolIcon    (Image — set at runtime)
///       └── CloseButton      (Button)
///   └── ConfirmPopup (CanvasGroup, alpha=0 hidden)
///       ├── PopupBackground  (Image → popup.png)
///       ├── MessageLabel     (TMP_Text)
///       ├── YesButton        (Image → Yes_No_Btn_0)
///       └── NoButton         (Image → Yes_No_Btn_1)
/// </summary>
public class PopupController : MonoBehaviour
{
    // ------------------------------------------------------------------
    // Inspector: Win Popup
    // ------------------------------------------------------------------

    [Header("Win Popup")]
    [SerializeField] private CanvasGroup winPopupGroup;
    [SerializeField] private TMP_Text    winAmountLabel;
    [SerializeField] private Image       winSymbolIcon;
    [SerializeField] private Button      winCloseButton;

    [Header("Win Popup Timing")]
    [SerializeField] private float autoCloseDelay = 3f;
    [SerializeField] private float fadeDuration   = 0.25f;

    // ------------------------------------------------------------------
    // Inspector: Confirm Popup (Yes/No)
    // ------------------------------------------------------------------

    [Header("Confirm Popup")]
    [SerializeField] private CanvasGroup confirmPopupGroup;
    [SerializeField] private TMP_Text    confirmMessageLabel;
    [SerializeField] private Button      yesButton;
    [SerializeField] private Button      noButton;

    // ------------------------------------------------------------------
    // Private State
    // ------------------------------------------------------------------

    private Coroutine _autoCloseRoutine;
    private Action    _onYes;
    private Action    _onNo;

    // ------------------------------------------------------------------
    // Unity Lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        HideImmediate(winPopupGroup);
        HideImmediate(confirmPopupGroup);

        winCloseButton.onClick.AddListener(() => StartCoroutine(FadeOut(winPopupGroup)));
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
    }

    // ------------------------------------------------------------------
    // Public API — Win Popup
    // ------------------------------------------------------------------

    /// <summary>
    /// Show the win popup with the payout amount and the winning symbol sprite.
    /// Closes automatically after <see cref="autoCloseDelay"/> seconds.
    /// </summary>
    public void ShowWin(int payout, Sprite symbolSprite, WinType winType)
    {
        if (_autoCloseRoutine != null)
            StopCoroutine(_autoCloseRoutine);

        string header = winType switch
        {
            WinType.AllWilds => "✦ JACKPOT ✦",
            WinType.Scatter  => "✦ BONUS ✦",
            _                => "✦ YOU WIN ✦"
        };

        winAmountLabel.text = $"{header}\n+{payout:N0} CREDITS";

        if (winSymbolIcon != null && symbolSprite != null)
            winSymbolIcon.sprite = symbolSprite;

        _autoCloseRoutine = StartCoroutine(ShowThenAutoClose());
    }

    private IEnumerator ShowThenAutoClose()
    {
        yield return StartCoroutine(FadeIn(winPopupGroup));
        yield return new WaitForSeconds(autoCloseDelay);
        yield return StartCoroutine(FadeOut(winPopupGroup));
    }

    // ------------------------------------------------------------------
    // Public API — Confirm Popup
    // ------------------------------------------------------------------

    /// <summary>
    /// Show a Yes/No confirmation popup.
    /// <paramref name="onYes"/> fires if the player taps Yes.
    /// <paramref name="onNo"/> fires if the player taps No.
    /// </summary>
    public void ShowConfirm(string message, Action onYes, Action onNo = null)
    {
        confirmMessageLabel.text = message;
        _onYes = onYes;
        _onNo  = onNo;
        StartCoroutine(FadeIn(confirmPopupGroup));
    }

    private void OnYesClicked()
    {
        StartCoroutine(FadeOut(confirmPopupGroup));
        _onYes?.Invoke();
    }

    private void OnNoClicked()
    {
        StartCoroutine(FadeOut(confirmPopupGroup));
        _onNo?.Invoke();
    }

    // ------------------------------------------------------------------
    // Fade Helpers
    // ------------------------------------------------------------------

    private IEnumerator FadeIn(CanvasGroup group)
    {
        group.gameObject.SetActive(true);
        group.blocksRaycasts = true;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        group.alpha = 1f;
    }

    private IEnumerator FadeOut(CanvasGroup group)
    {
        group.blocksRaycasts = false;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        HideImmediate(group);
    }

    private static void HideImmediate(CanvasGroup group)
    {
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.gameObject.SetActive(false);
    }
}
