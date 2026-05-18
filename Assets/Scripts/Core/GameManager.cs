using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central game controller. Owns player credits, bet size, spin state,
/// and free-spin inventory. Coordinates ReelController and UIManager.
///
/// Attach to a persistent GameObject in the scene (e.g. "GameManager").
/// </summary>
public class GameManager : MonoBehaviour
{
    // ------------------------------------------------------------------
    // Inspector Fields
    // ------------------------------------------------------------------

    [Header("Configuration")]
    [Tooltip("Shared game settings ScriptableObject")]
    [SerializeField] private GameConfig config;

    [Tooltip("All symbols available in this game, ordered by index")]
    [SerializeField] private List<SlotSymbol> symbolPool;

    [Header("Reels")]
    [Tooltip("One ReelController per physical reel, left to right")]
    [SerializeField] private List<ReelController> reels;

    [Header("UI")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PopupController popupController;

    [Header("Machine Animation")]
    [Tooltip("Drives the slot-machine1..5 frame animation during spin")]
    [SerializeField] private ReelAnimator reelAnimator;

    // ------------------------------------------------------------------
    // Events (UI listens to these)
    // ------------------------------------------------------------------

    [HideInInspector] public UnityEvent<int> onCreditsChanged;   // new credit total
    [HideInInspector] public UnityEvent<int> onBetChanged;       // new bet value
    [HideInInspector] public UnityEvent<int, WinType> onSpinResult; // payout, winType
    [HideInInspector] public UnityEvent onSpinStarted;
    [HideInInspector] public UnityEvent onFreeSpinAwarded;

    // ------------------------------------------------------------------
    // State
    // ------------------------------------------------------------------

    private int _credits;
    private int _currentBet;
    private int _freeSpinsRemaining;
    private bool _isSpinning;

    // Publicly readable state
    public int Credits => _credits;
    public int CurrentBet => _currentBet;
    public int FreeSpinsRemaining => _freeSpinsRemaining;
    public bool IsSpinning => _isSpinning;

    // ------------------------------------------------------------------
    // Unity Lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        ValidateSetup();
        _credits = config.startingCredits;
        _currentBet = config.defaultBet;
    }

    private void Start()
    {
        // Push initial state to UI
        onCreditsChanged.Invoke(_credits);
        onBetChanged.Invoke(_currentBet);
    }

    // ------------------------------------------------------------------
    // Public API (called by UIManager button events)
    // ------------------------------------------------------------------

    /// <summary>
    /// Begins a spin. Deducts the bet (unless free spin), generates result,
    /// fires animations, then resolves payout.
    /// </summary>
    public void RequestSpin()
    {
        if (_isSpinning) return;

        bool isFreeSpin = _freeSpinsRemaining > 0;

        if (!isFreeSpin && _credits < _currentBet)
        {
            uiManager.ShowMessage("Not enough credits!");
            return;
        }

        StartCoroutine(SpinRoutine(isFreeSpin));
    }

    /// <summary>Increases bet by one step, clamped to config.maxBet.</summary>
    public void IncreaseBet()
    {
        if (_isSpinning) return;
        _currentBet = Mathf.Min(_currentBet + 1, config.maxBet);
        onBetChanged.Invoke(_currentBet);
    }

    /// <summary>Decreases bet by one step, clamped to config.minBet.</summary>
    public void DecreaseBet()
    {
        if (_isSpinning) return;
        _currentBet = Mathf.Max(_currentBet - 1, config.minBet);
        onBetChanged.Invoke(_currentBet);
    }

    /// <summary>Sets bet to max allowed value.</summary>
    public void BetMax()
    {
        if (_isSpinning) return;
        _currentBet = config.maxBet;
        onBetChanged.Invoke(_currentBet);
    }

    // ------------------------------------------------------------------
    // Spin Coroutine
    // ------------------------------------------------------------------

    private IEnumerator SpinRoutine(bool isFreeSpin)
    {
        _isSpinning = true;
        onSpinStarted.Invoke();

        // 1. Deduct bet or consume free spin
        if (isFreeSpin)
        {
            _freeSpinsRemaining--;
            uiManager.ShowMessage($"FREE SPIN! ({_freeSpinsRemaining} remaining)");
        }
        else
        {
            ModifyCredits(-_currentBet);
        }

        // 2. Generate RNG result BEFORE animation starts (fair – result is decided upfront)
        int[] spinResult = RNGService.GenerateSpinResult(symbolPool, config.reelCount);

        // 3. Start machine body animation + all reels spinning simultaneously
        reelAnimator?.SetSpinning(true);
        foreach (ReelController reel in reels)
            reel.StartSpin();

        // 4. Wait for minimum spin time
        yield return new WaitForSeconds(config.baseSpinDuration);

        // 5. Stop reels one by one with a delay between each
        for (int i = 0; i < reels.Count; i++)
        {
            reels[i].StopSpin(symbolPool[spinResult[i]]);
            yield return new WaitForSeconds(config.reelStopDelay);
        }

        // 6. Wait for the last reel's snap animation to finish
        yield return new WaitForSeconds(0.4f);

        // 6. Stop machine body animation
        reelAnimator?.SetSpinning(false);

        // 7. Evaluate result
        int payout = PayoutCalculator.Evaluate(spinResult, symbolPool, _currentBet, out WinType winType);

        if (payout > 0)
        {
            ModifyCredits(payout);
            onSpinResult.Invoke(payout, winType);

            // Show the win popup (popup.png overlay)
            Sprite winSprite = symbolPool[spinResult[0]].sprite;
            popupController?.ShowWin(payout, winSprite, winType);

            // Bonus: award free spins on scatter win
            if (winType == WinType.Scatter)
            {
                _freeSpinsRemaining += config.freeSpinsAwarded;
                onFreeSpinAwarded.Invoke();
                uiManager.ShowMessage($"BONUS! {config.freeSpinsAwarded} Free Spins!");
            }
        }
        else
        {
            onSpinResult.Invoke(0, WinType.None);
        }

        _isSpinning = false;

        // Auto-spin if free spins remain
        if (_freeSpinsRemaining > 0)
        {
            yield return new WaitForSeconds(0.8f);
            RequestSpin();
        }
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private void ModifyCredits(int delta)
    {
        _credits = Mathf.Max(0, _credits + delta);
        onCreditsChanged.Invoke(_credits);
    }

    private void ValidateSetup()
    {
        if (config == null)
            Debug.LogError("[GameManager] GameConfig is not assigned!");
        if (symbolPool == null || symbolPool.Count == 0)
            Debug.LogError("[GameManager] Symbol pool is empty!");
        if (reels == null || reels.Count == 0)
            Debug.LogError("[GameManager] No ReelControllers assigned!");
        if (uiManager == null)
            Debug.LogError("[GameManager] UIManager is not assigned!");
    }
}
