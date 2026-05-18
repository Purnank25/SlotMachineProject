using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "SlotGame/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Economy")]
    [Tooltip("Credits the player starts with")]
    public int startingCredits = 1000;

    [Tooltip("Default bet amount per spin")]
    public int defaultBet = 10;

    [Tooltip("Minimum allowed bet")]
    public int minBet = 1;

    [Tooltip("Maximum allowed bet")]
    public int maxBet = 100;

    [Header("Reels")]
    [Tooltip("Number of reels (columns) in the slot machine")]
    public int reelCount = 3;

    [Tooltip("Symbols visible per reel at any time")]
    public int visibleRows = 1;

    [Header("Animation")]
    [Tooltip("Base spin duration in seconds before reels start stopping")]
    public float baseSpinDuration = 1.5f;

    [Tooltip("Extra delay (seconds) between each reel stopping")]
    public float reelStopDelay = 0.3f;

    [Tooltip("Speed of the reel scroll (units per second)")]
    public float reelScrollSpeed = 8f;

    [Tooltip("Bounce overshoot distance when a reel snaps to final position")]
    public float snapBounceDistance = 0.15f;

    [Header("Bonus")]
    [Tooltip("Number of scatter symbols needed to trigger free spins")]
    public int scatterCountForBonus = 3;

    [Tooltip("Number of free spins awarded on bonus trigger")]
    public int freeSpinsAwarded = 10;
}