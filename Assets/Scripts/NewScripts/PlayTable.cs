

using System.Collections.Generic;

public static class PayTable
{
    // Key: symbol that must appear on all 3 reels
    // Value: multiplier applied to the bet amount
    public static readonly Dictionary<SlotSymbol, int> ThreeOfAKind = new Dictionary<SlotSymbol, int>
    {
        { SlotSymbol.Cherry, 5  },   // Cherry x3   5x bet
        { SlotSymbol.Bell,   10 },   // Bell   x3   10x bet
        { SlotSymbol.Bar,    20 },   // BAR    x3   20x bet
        { SlotSymbol.Seven,  50 },   // 7      x3   50x bet  (JACKPOT)
    };

    // Any two cherries anywhere on the payline  2x bet
    public static readonly int TwoCherriesMultiplier = 2;

    // Returns payout for a given result (0 = no win)
    public static int Calculate(SlotSymbol[] result, int bet)
    {
        if (result == null || result.Length != 3) return 0;

        // Three of a kind
        if (result[0] == result[1] && result[1] == result[2])
        {
            if (ThreeOfAKind.TryGetValue(result[0], out int mult))
                return bet * mult;
        }

        // Two cherries (any positions)
        int cherryCount = 0;
        for (int i = 0; i < result.Length; i++)
            if (result[i] == SlotSymbol.Cherry) cherryCount++;

        if (cherryCount >= 2)
            return bet * TwoCherriesMultiplier;

        return 0;
    }

    // True when all three are Sevens (used to trigger jackpot FX)
    public static bool IsJackpot(SlotSymbol[] result)
    {
        return result != null && result.Length == 3
            && result[0] == SlotSymbol.Seven
            && result[1] == SlotSymbol.Seven
            && result[2] == SlotSymbol.Seven;
    }
}