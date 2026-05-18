using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stateless service that evaluates a completed spin result and
/// calculates the total payout.
///
/// Win conditions:
///   - All reels show the SAME symbol  → symbol.payoutMultiplier × bet
///   - Wild symbols count as any symbol
///   - Scatter symbols pay regardless of position (if 3+ present)
/// </summary>
public static class PayoutCalculator
{
    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Evaluates the spin outcome and returns the credit payout.
    /// </summary>
    /// <param name="resultIndices">Symbol index landed on each reel.</param>
    /// <param name="symbols">The full symbol pool (same list used by RNGService).</param>
    /// <param name="bet">Current bet amount.</param>
    /// <param name="winType">Out parameter describing the kind of win.</param>
    /// <returns>Total credits won (0 if no win).</returns>
    public static int Evaluate(int[] resultIndices, List<SlotSymbol> symbols, int bet, out WinType winType)
    {
        winType = WinType.None;

        if (resultIndices == null || resultIndices.Length == 0)
            return 0;

        // --- Collect the actual symbol objects ---
        SlotSymbol[] landed = new SlotSymbol[resultIndices.Length];
        for (int i = 0; i < resultIndices.Length; i++)
            landed[i] = symbols[resultIndices[i]];

        // --- Check scatter bonus first ---
        int scatterCount = CountScatters(landed);
        if (scatterCount >= 3)
        {
            winType = WinType.Scatter;
            return bet * 5 * scatterCount; // scatter pays 5× per scatter symbol
        }

        // --- Check main line (all match, wilds count as anything) ---
        SlotSymbol baseSymbol = GetBaseSymbol(landed);
        if (baseSymbol != null && AllMatch(landed, baseSymbol))
        {
            if (baseSymbol.isWild)
            {
                winType = WinType.AllWilds;
                return bet * 50; // jackpot: all wilds
            }

            winType = WinType.ThreeOfAKind;
            return bet * baseSymbol.payoutMultiplier;
        }

        return 0; // no win
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns the first non-wild symbol, used as the reference for matching.
    /// If everything is wild, returns the wild symbol itself (jackpot).
    /// </summary>
    private static SlotSymbol GetBaseSymbol(SlotSymbol[] landed)
    {
        foreach (SlotSymbol sym in landed)
        {
            if (!sym.isWild && !sym.isScatter)
                return sym;
        }
        // All symbols are wilds
        return landed[0];
    }

    /// <summary>
    /// Returns true if every reel shows the base symbol or a wild.
    /// </summary>
    private static bool AllMatch(SlotSymbol[] landed, SlotSymbol baseSymbol)
    {
        foreach (SlotSymbol sym in landed)
        {
            if (sym.isScatter) return false; // scatters don't participate in line wins
            if (!sym.isWild && sym.symbolID != baseSymbol.symbolID)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Counts how many scatter symbols landed on the reels.
    /// </summary>
    private static int CountScatters(SlotSymbol[] landed)
    {
        int count = 0;
        foreach (SlotSymbol sym in landed)
            if (sym.isScatter) count++;
        return count;
    }
}

/// <summary>
/// Describes the type of win that occurred.
/// Used by the UI to play the correct celebration animation.
/// </summary>
public enum WinType
{
    None,
    ThreeOfAKind,
    Scatter,
    AllWilds   // jackpot
}
