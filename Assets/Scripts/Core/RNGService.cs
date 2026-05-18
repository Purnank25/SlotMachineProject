using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stateless service that handles all randomization for the slot machine.
/// Uses weighted probability so rarer symbols appear less often.
/// 
/// Usage:
///   int index = RNGService.PickWeightedIndex(symbolList);
/// </summary>
public static class RNGService
{
    // ------------------------------------------------------------------
    // Core weighted-pick logic
    // ------------------------------------------------------------------

    /// <summary>
    /// Picks a random index from a list of SlotSymbols using each symbol's
    /// weight as its relative probability.
    ///
    /// Algorithm:
    ///   1. Sum all weights.
    ///   2. Roll a random number in [0, totalWeight).
    ///   3. Walk the list subtracting each weight until we reach 0.
    ///   4. Return the index we stopped at.
    ///
    /// Example: weights [60, 30, 10] → 60 % chance / 30 % / 10 %
    /// </summary>
    public static int PickWeightedIndex(List<SlotSymbol> symbols)
    {
        if (symbols == null || symbols.Count == 0)
        {
            Debug.LogError("[RNGService] Symbol list is null or empty.");
            return 0;
        }

        int totalWeight = 0;
        foreach (SlotSymbol sym in symbols)
            totalWeight += Mathf.Max(1, sym.weight); // guard against 0-weight entries

        int roll = Random.Range(0, totalWeight); // [0, totalWeight)

        int cumulative = 0;
        for (int i = 0; i < symbols.Count; i++)
        {
            cumulative += Mathf.Max(1, symbols[i].weight);
            if (roll < cumulative)
                return i;
        }

        // Fallback (should never reach here)
        return symbols.Count - 1;
    }

    /// <summary>
    /// Generates a full spin result: one symbol index per reel.
    /// </summary>
    /// <param name="symbols">The shared symbol pool used by all reels.</param>
    /// <param name="reelCount">How many reels to generate results for.</param>
    /// <returns>Array of symbol indices, one per reel.</returns>
    public static int[] GenerateSpinResult(List<SlotSymbol> symbols, int reelCount)
    {
        int[] result = new int[reelCount];
        for (int i = 0; i < reelCount; i++)
            result[i] = PickWeightedIndex(symbols);
        return result;
    }
}
