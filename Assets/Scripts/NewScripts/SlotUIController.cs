// SlotUIController.cs
// Attach to Canvas → UIController GameObject.
// Drives all UI and keeps the lever in sync with the selected bet.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUIController : MonoBehaviour
{
    [Header("Manager Reference")]
    public SlotMachineManager manager;

    [Header("Lever Reference")]
    [Tooltip("Drag the Lever GameObject here so bet selection updates it")]
    public LeverController lever;

    [Header("Gold Display")]
    public TMP_Text goldText;

    [Header("Bet Menu Panel")]
    public GameObject betMenuPanel;
    public Button bet10Button;
    public Button bet50Button;
    public Button bet100Button;
    public Button exitButton;

    [Header("Result Banners")]
    public GameObject winBanner;
    public TMP_Text winAmountText;
    public GameObject loseBanner;
    public GameObject jackpotBanner;

    // =========================================================
    //  Unity Lifecycle
    // =========================================================

    private void Start()
    {
        // Button clicks
        bet10Button?.onClick.AddListener(() => SelectBet(10));
        bet50Button?.onClick.AddListener(() => SelectBet(50));
        bet100Button?.onClick.AddListener(() => SelectBet(100));
        exitButton?.onClick.AddListener(OnExitPressed);

        // Manager events
        manager.onGoldChanged.AddListener(UpdateGold);
        manager.onStateChanged.AddListener(HandleStateChange);
        manager.onWin.AddListener(ShowWin);
        manager.onLose.AddListener(ShowLose);
        manager.onJackpot.AddListener(ShowJackpot);

        // Init
        UpdateGold(manager.Gold);
        HandleStateChange(manager.State);

        // Default bet
        SelectBet(10);
    }

    // =========================================================
    //  Bet Selection
    // =========================================================

    /// <summary>
    /// Called when player clicks a bet button.
    /// Updates the lever's stored bet then immediately pulls it.
    /// </summary>
    private void SelectBet(int amount)
    {
        if (lever != null)
        {
            lever.SetBet(amount);
            lever.TryPullLever();   // clicking bet button also pulls the lever
        }
        else
        {
            // Fallback if no lever assigned — bet directly
            manager.PlaceBet(amount);
        }
    }

    // =========================================================
    //  Manager Event Handlers  (must be public for UnityEvent wiring)
    // =========================================================

    public void UpdateGold(int gold)
    {
        if (goldText) goldText.text = gold.ToString();
    }

    public void HandleStateChange(SlotMachineManager.GameState state)
    {
        // Show bet menu only while idle
        if (betMenuPanel) betMenuPanel.SetActive(state == SlotMachineManager.GameState.Idle);

        // Clear banners when a new spin starts
        if (state == SlotMachineManager.GameState.Spinning)
            SetBanners(false, false, false);
    }

    public void ShowWin(int amount)
    {
        if (winBanner) winBanner.SetActive(true);
        if (winAmountText) winAmountText.text = $"+{amount}G";
    }

    public void ShowLose()
    {
        if (loseBanner) loseBanner.SetActive(true);
    }

    public void ShowJackpot()
    {
        if (jackpotBanner) jackpotBanner.SetActive(true);
        StartCoroutine(HideAfter(jackpotBanner, 2f));
    }

    // =========================================================
    //  Helpers
    // =========================================================

    private void SetBanners(bool win, bool lose, bool jackpot)
    {
        if (winBanner) winBanner.SetActive(win);
        if (loseBanner) loseBanner.SetActive(lose);
        if (jackpotBanner) jackpotBanner.SetActive(jackpot);
    }

    private IEnumerator HideAfter(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (go) go.SetActive(false);
    }

    private void OnExitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}