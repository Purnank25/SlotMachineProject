using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    // INspector FIelds

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


    // EVents

    [HideInInspector] public UnityEvent<int> onCreditsChanged;   
    [HideInInspector] public UnityEvent<int> onBetChanged;       
    [HideInInspector] public UnityEvent<int, WinType> onSpinResult; 
    [HideInInspector] public UnityEvent onSpinStarted;
    [HideInInspector] public UnityEvent onFreeSpinAwarded;


    // STates

    private int _credits;
    private int _currentBet;
    private int _freeSpinsRemaining;
    private bool _isSpinning;

    // Publicly readable state
    public int Credits => _credits;
    public int CurrentBet => _currentBet;
    public int FreeSpinsRemaining => _freeSpinsRemaining;
    public bool IsSpinning => _isSpinning;



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

    public void IncreaseBet()
    {
        if (_isSpinning) return;
        _currentBet = Mathf.Min(_currentBet + 1, config.maxBet);
        onBetChanged.Invoke(_currentBet);
    }

 
    public void DecreaseBet()
    {
        if (_isSpinning) return;
        _currentBet = Mathf.Max(_currentBet - 1, config.minBet);
        onBetChanged.Invoke(_currentBet);
    }

   
    public void BetMax()
    {
        if (_isSpinning) return;
        _currentBet = config.maxBet;
        onBetChanged.Invoke(_currentBet);
    }

    private IEnumerator SpinRoutine(bool isFreeSpin)
    {
        _isSpinning = true;
        onSpinStarted.Invoke();

    
        if (isFreeSpin)
        {
            _freeSpinsRemaining--;
            uiManager.ShowMessage($"FREE SPIN! ({_freeSpinsRemaining} remaining)");
        }
        else
        {
            ModifyCredits(-_currentBet);
        }

        int[] spinResult = RNGService.GenerateSpinResult(symbolPool, config.reelCount);

        reelAnimator?.SetSpinning(true);
        foreach (ReelController reel in reels)
            reel.StartSpin();

        yield return new WaitForSeconds(config.baseSpinDuration);

        for (int i = 0; i < reels.Count; i++)
        {
            reels[i].StopSpin(symbolPool[spinResult[i]]);
            yield return new WaitForSeconds(config.reelStopDelay);
        }

        yeild return new WaitForSeconds(0.4f);

        reelAnimator?.SetSpinning(false);

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

        if (_freeSpinsRemaining > 0)
        {
            yield return new WaitForSeconds(0.8f);
            RequestSpin();
        }
    }
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