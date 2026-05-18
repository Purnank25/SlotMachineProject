using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages every UI element in the game:
///   - Credits / Bet / Win displays
///   - Spin / Bet+/- / Max Bet buttons
///   - Win celebration overlay
///   - Toast messages
///   - Free spin counter badge
///
/// Attach to a UIManager GameObject. Drag references in Inspector.
/// Subscribe to GameManager events from Awake().
/// </summary>
public class UIManager : MonoBehaviour
{
    // ------------------------------------------------------------------
    // Inspector References
    // ------------------------------------------------------------------

    [Header("GameManager")]
    [SerializeField] private GameManager gameManager;

    [Header("Info Labels")]
    [SerializeField] private TMP_Text creditsLabel;
    [SerializeField] private TMP_Text betLabel;
    [SerializeField] private TMP_Text winLabel;
    [SerializeField] private TMP_Text freeSpinsLabel;   // badge on SPIN button

    [Header("Buttons")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button increaseBetButton;
    [SerializeField] private Button decreaseBetButton;
    [SerializeField] private Button maxBetButton;

    [Header("Win Overlay")]
    [Tooltip("Panel that appears on a win (set alpha to 0 when hidden)")]
    [SerializeField] private CanvasGroup winOverlay;

    [SerializeField] private TMP_Text winOverlayAmount;

    [Tooltip("Particle system that plays on a win")]
    [SerializeField] private ParticleSystem winParticles;

    [Header("Toast")]
    [Tooltip("Small message popup (TMP_Text inside a panel)")]
    [SerializeField] private TMP_Text toastLabel;

    [SerializeField] private CanvasGroup toastGroup;

    // ------------------------------------------------------------------
    // Private State
    // ------------------------------------------------------------------

    private Coroutine _toastCoroutine;

    // ------------------------------------------------------------------
    // Unity Lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        // Subscribe to GameManager events
        gameManager.onCreditsChanged.AddListener(UpdateCreditsDisplay);
        gameManager.onBetChanged.AddListener(UpdateBetDisplay);
        gameManager.onSpinResult.AddListener(HandleSpinResult);
        gameManager.onSpinStarted.AddListener(HandleSpinStarted);
        gameManager.onFreeSpinAwarded.AddListener(UpdateFreeSpinBadge);

        // Wire button callbacks
        spinButton.onClick.AddListener(gameManager.RequestSpin);
        increaseBetButton.onClick.AddListener(gameManager.IncreaseBet);
        decreaseBetButton.onClick.AddListener(gameManager.DecreaseBet);
        maxBetButton.onClick.AddListener(gameManager.BetMax);

        // Initial state
        HideWinOverlay();
        HideToast();
        UpdateFreeSpinBadge();
    }

    // ------------------------------------------------------------------
    // Event Handlers
    // ------------------------------------------------------------------

    /// <summary>Updates the credits display with comma-formatted number.</summary>
    private void UpdateCreditsDisplay(int credits)
    {
        creditsLabel.text = $"CREDITS\n{credits:N0}";
    }

    /// <summary>Updates the bet label.</summary>
    private void UpdateBetDisplay(int bet)
    {
        betLabel.text = $"BET\n{bet}";
    }

    /// <summary>Called when spin animation completes; shows win or clears win display.</summary>
    private void HandleSpinResult(int payout, WinType winType)
    {
        SetSpinButtonInteractable(true);

        if (payout > 0)
        {
            winLabel.text = $"WIN\n{payout:N0}";
            ShowWinOverlay(payout, winType);
        }
        else
        {
            winLabel.text = "WIN\n-";
            HideWinOverlay();
        }
    }

    /// <summary>Called the moment spinning begins — disable buttons, clear win.</summary>
    private void HandleSpinStarted()
    {
        SetSpinButtonInteractable(false);
        winLabel.text = "WIN\n-";
        HideWinOverlay();
    }

    // ------------------------------------------------------------------
    // Win Overlay
    // ------------------------------------------------------------------

    private void ShowWinOverlay(int payout, WinType winType)
    {
        string header = winType switch
        {
            WinType.AllWilds  => "★ JACKPOT ★",
            WinType.Scatter   => "★ BONUS WIN ★",
            _                 => "★ YOU WIN ★"
        };

        winOverlayAmount.text = $"{header}\n+{payout:N0}";
        winOverlay.alpha = 1f;
        winOverlay.gameObject.SetActive(true);

        if (winParticles != null)
            winParticles.Play();

        // Auto-hide after 2 seconds
        StartCoroutine(HideWinOverlayAfterDelay(2.5f));
    }

    private void HideWinOverlay()
    {
        winOverlay.alpha = 0f;
        winOverlay.gameObject.SetActive(false);

        if (winParticles != null && winParticles.isPlaying)
            winParticles.Stop();
    }

    private IEnumerator HideWinOverlayAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Fade out
        float elapsed = 0f;
        float fadeDuration = 0.4f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            winOverlay.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        HideWinOverlay();
    }

    // ------------------------------------------------------------------
    // Free Spin Badge
    // ------------------------------------------------------------------

    private void UpdateFreeSpinBadge()
    {
        if (gameManager.FreeSpinsRemaining > 0)
        {
            freeSpinsLabel.gameObject.SetActive(true);
            freeSpinsLabel.text = $"FREE x{gameManager.FreeSpinsRemaining}";
        }
        else
        {
            freeSpinsLabel.gameObject.SetActive(false);
        }
    }

    // ------------------------------------------------------------------
    // Toast Messages
    // ------------------------------------------------------------------

    /// <summary>Shows a brief toast message at the bottom of the screen.</summary>
    public void ShowMessage(string message)
    {
        if (_toastCoroutine != null)
            StopCoroutine(_toastCoroutine);
        _toastCoroutine = StartCoroutine(ToastRoutine(message));
    }

    private IEnumerator ToastRoutine(string message)
    {
        toastLabel.text = message;
        toastGroup.alpha = 1f;
        toastGroup.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        // Fade out
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            toastGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.3f);
            yield return null;
        }

        HideToast();
    }

    private void HideToast()
    {
        toastGroup.alpha = 0f;
        toastGroup.gameObject.SetActive(false);
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private void SetSpinButtonInteractable(bool interactable)
    {
        spinButton.interactable = interactable;
        increaseBetButton.interactable = interactable;
        decreaseBetButton.interactable = interactable;
        maxBetButton.interactable = interactable;
    }
}
