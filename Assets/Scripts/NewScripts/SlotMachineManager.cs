
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class SlotMachineManager : MonoBehaviour
{
  
    public enum GameState
    {
        Idle,           // Waiting for player to place bet
        Spinning,       // Reels in motion
        Evaluating,     // All reels stopped, calculating result
        ShowResult,     // Showing win/lose
    }

  
    [Header("Reels")]
    public ReelController[] reels;              // Assign Reel0, Reel1, Reel2

    [Header("Symbol Sprites")]
    [Tooltip("Must match SlotSymbol enum order: Cherry=0, Bell=1, Bar=2, Seven=3")]
    public Sprite[] symbolSprites;

    [Header("Reel Strip (shared)")]
    [Tooltip("Symbol sequence on every reel. Edit for custom odds.")]
    public SlotSymbol[] reelStrip = new SlotSymbol[]
    {
        SlotSymbol.Cherry,
        SlotSymbol.Bell,
        SlotSymbol.Cherry,
        SlotSymbol.Bar,
        SlotSymbol.Cherry,
        SlotSymbol.Bell,
        SlotSymbol.Seven,
        SlotSymbol.Bar,
        SlotSymbol.Cherry,
        SlotSymbol.Bell,
    };

    [Header("Spin Timing")]
    [Tooltip("Base spin duration (seconds) before deceleration. Each reel adds spinStaggerDelay more.")]
    public float baseSpinDuration = 1.5f;
    public float spinStaggerDelay = 0.4f;   // each successive reel stops this much later

    [Header("Player Data")]
    public int startingGold = 50;

    [Header("Events — wire up to your UI scripts")]
    public UnityEvent<int> onGoldChanged;      // passes new gold value
    public UnityEvent<GameState> onStateChanged;
    public UnityEvent<int> onWin;              // passes amount won
    public UnityEvent onLose;
    public UnityEvent onJackpot;

  
    public int Gold { get; private set; }
    public int CurrentBet { get; private set; }
    public GameState State { get; private set; }

    // Last result after a spin
    public SlotSymbol[] LastResult { get; private set; }
    public int LastPayout { get; private set; }

    private int _reelsStopped;

  
    private void Awake()
    {
        Gold = startingGold;
        SetState(GameState.Idle);

        // Inject sprites and strip into every reel
        foreach (var reel in reels)
        {
            reel.Init(symbolSprites);
            reel.strip = (SlotSymbol[])reelStrip.Clone();
            reel.OnReelStopped += HandleReelStopped;
        }
    }

   
    public void PlaceBet(int betAmount)
    {
        if (State != GameState.Idle)
        {
            Debug.LogWarning("[SlotMachine] Cannot bet while not Idle.");
            return;
        }

        if (betAmount > Gold)
        {
            Debug.Log("[SlotMachine] Not enough gold!");
            return;
        }

        CurrentBet = betAmount;
        Gold -= betAmount;
        onGoldChanged?.Invoke(Gold);

        StartCoroutine(SpinAllReels());
    }

    // Convenience wrappers matching the UI buttons in the screenshot
    public void BetTen() => PlaceBet(10);
    public void BetFifty() => PlaceBet(50);
    public void BetHundred() => PlaceBet(100);

  
    private IEnumerator SpinAllReels()
    {
        SetState(GameState.Spinning);
        _reelsStopped = 0;

        // Decide the outcome NOW (server-side style — result chosen before animation)
        SlotSymbol[] result = RollResult();
        LastResult = result;

        // Kick off each reel with staggered stop times
        for (int i = 0; i < reels.Length; i++)
        {
            float duration = baseSpinDuration + i * spinStaggerDelay;
            reels[i].Spin(result[i], duration);
        }

        // Wait until all reels have stopped (handled by HandleReelStopped callback)
        yield return null;
    }

    private void HandleReelStopped(ReelController reel)
    {
        _reelsStopped++;
        if (_reelsStopped < reels.Length) return;   // still waiting for others

        // All reels stopped
        StartCoroutine(EvaluateResult());
    }

    private IEnumerator EvaluateResult()
    {
        SetState(GameState.Evaluating);

        // Small pause for visual effect before showing result
        yield return new WaitForSeconds(0.2f);

        LastPayout = PayTable.Calculate(LastResult, CurrentBet);

        SetState(GameState.ShowResult);

        if (LastPayout > 0)
        {
            Gold += LastPayout;
            onGoldChanged?.Invoke(Gold);
            onWin?.Invoke(LastPayout);

            if (PayTable.IsJackpot(LastResult))
                onJackpot?.Invoke();

            Debug.Log($"[SlotMachine] WIN! Payout: {LastPayout}G  Gold: {Gold}G");
        }
        else
        {
            onLose?.Invoke();
            Debug.Log($"[SlotMachine] No win. Gold: {Gold}G");
        }

        // Wait for a beat then return to Idle
        yield return new WaitForSeconds(1.5f);
        SetState(GameState.Idle);
    }

    private SlotSymbol[] RollResult()
    {
        SlotSymbol[] result = new SlotSymbol[reels.Length];
        for (int i = 0; i < reels.Length; i++)
            result[i] = reelStrip[Random.Range(0, reelStrip.Length)];
        return result;
    }


    private void SetState(GameState newState)
    {
        State = newState;
        onStateChanged?.Invoke(newState);
    }
}