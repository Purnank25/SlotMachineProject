using UnityEngine;

[CreateAssetMenu(fileName = "SlotSymbol", menuName = "SlotGame/SlotSymbol")]
public class SlotSymbol : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique ID matching asset name: 'symbol1', 'symbol2', 'symbol3', 'symbol4'")]
    public string symbolID;

    [Tooltip("Assign the 96×96 sprite from Assets/Sprites/Symbols/")]
    public Sprite sprite;

    [Header("Payout")]
    [Tooltip("Credits won = bet × payoutMultiplier when all 3 reels match this symbol.\n" +
             "Suggested: Symbol1=2x  Symbol2=5x  Symbol3=10x  Symbol4=25x")]
    public int payoutMultiplier = 2;

    [Header("Rarity")]
    [Tooltip("Relative draw probability. Higher = appears more often.\n" +
             "Suggested: Symbol1=40  Symbol2=30  Symbol3=20  Symbol4=10")]
    [Range(1, 100)]
    public int weight = 20;

    [Header("Special Flags")]
    [Tooltip("Wild: substitutes for any regular symbol when evaluating a win line")]
    public bool isWild = false;

    [Tooltip("Scatter: triggers free spins when 3 appear anywhere on the reels")]
    public bool isScatter = false;
}