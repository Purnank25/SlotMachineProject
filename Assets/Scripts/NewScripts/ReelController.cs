
using System;
using System.Collections;
using UnityEngine;

public class ReelController : MonoBehaviour
{
    [Header("Reel Settings")]
    [Tooltip("Height of one symbol cell in world units")]
    public float symbolHeight = 1.5f;

    [Tooltip("How many symbol slots are visible (including hidden top/bottom)")]
    public int visibleRows = 5;                // 3 visible + 1 top buffer + 1 bottom buffer

    [Tooltip("Base spin speed (world units per second)")]
    public float spinSpeed = 8f;

    [Tooltip("Deceleration curve when stopping")]
    public AnimationCurve decelerateCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Tooltip("Time (seconds) the reel decelerates before fully stopping")]
    public float stopDuration = 0.4f;

    [Header("References")]
    [Tooltip("SpriteRenderers for each row, top-to-bottom")]
    public SpriteRenderer[] rowRenderers;      // Assign 5 renderers in Inspector

    // Symbols on this reel strip (set by SlotMachineManager before spinning)
    [HideInInspector] public SlotSymbol[] strip;

    // The symbol that landed on the centre payline after stopping
    public SlotSymbol ResultSymbol { get; private set; }

    // Is the reel currently spinning?
    public bool IsSpinning { get; private set; }

    // Called when the reel has fully stopped
    public event Action<ReelController> OnReelStopped;

    // ?? Sprites (injected by SlotMachineManager) ??????????????????????????
    private Sprite[] _symbolSprites;   // index matches (int)SlotSymbol

    // ?? Internal state ????????????????????????????????????????????????????
    private int _stripIndex;         // index of symbol currently shown in centre row
    private float _scrollOffset;       // sub-cell scroll position (0..symbolHeight)
    private Coroutine _spinCoroutine;

    // ?????????????????????????????????????????????????????????????????????
    // Public API
    // ?????????????????????????????????????????????????????????????????????

    /// <summary>Inject sprite array from manager. Must be called before Spin().</summary>
    public void Init(Sprite[] symbolSprites)
    {
        _symbolSprites = symbolSprites;
    }

    /// <summary>Start spinning. targetSymbol is where the reel will land.</summary>
    public void Spin(SlotSymbol targetSymbol, float spinDuration)
    {
        if (_spinCoroutine != null) StopCoroutine(_spinCoroutine);
        _spinCoroutine = StartCoroutine(SpinRoutine(targetSymbol, spinDuration));
    }

    /// <summary>Force-stop (e.g. instant stop cheat — not used in normal flow).</summary>
    public void ForceStop()
    {
        if (_spinCoroutine != null) StopCoroutine(_spinCoroutine);
        IsSpinning = false;
    }

    // ?????????????????????????????????????????????????????????????????????
    // Spin Coroutine
    // ?????????????????????????????????????????????????????????????????????

    private IEnumerator SpinRoutine(SlotSymbol targetSymbol, float spinDuration)
    {
        IsSpinning = true;

        // ?? Phase 1: Free spin for spinDuration seconds ???????????????????
        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            float delta = Time.deltaTime;
            elapsed += delta;
            ScrollReel(spinSpeed * delta);
            yield return null;
        }

        // ?? Phase 2: Align to targetSymbol then decelerate ???????????????
        // Find the target in the strip
        int targetIndex = FindSymbolInStrip(targetSymbol);
        _stripIndex = targetIndex;

        // Snap scroll offset to 0 so we cleanly decelerate into the slot
        _scrollOffset = 0f;
        RefreshRenderers();

        float stopElapsed = 0f;
        while (stopElapsed < stopDuration)
        {
            float t = stopElapsed / stopDuration;
            float speedMult = decelerateCurve.Evaluate(t);
            float delta = Time.deltaTime;
            stopElapsed += delta;
            ScrollReel(spinSpeed * speedMult * delta);
            yield return null;
        }

        // ?? Phase 3: Snap to final position ??????????????????????????????
        _scrollOffset = 0f;
        _stripIndex = targetIndex;
        RefreshRenderers();

        ResultSymbol = strip[_stripIndex];
        IsSpinning = false;

        OnReelStopped?.Invoke(this);
    }

    // ?????????????????????????????????????????????????????????????????????
    // Helpers
    // ?????????????????????????????????????????????????????????????????????

    private void ScrollReel(float amount)
    {
        _scrollOffset += amount;

        // Advance strip index each time we scroll past one cell
        while (_scrollOffset >= symbolHeight)
        {
            _scrollOffset -= symbolHeight;
            _stripIndex = (_stripIndex + 1) % strip.Length;
        }

        RefreshRenderers();
    }

    private void RefreshRenderers()
    {
        if (rowRenderers == null || rowRenderers.Length == 0) return;

        int half = rowRenderers.Length / 2;          // centre renderer index

        for (int row = 0; row < rowRenderers.Length; row++)
        {
            // Determine which strip symbol this row shows
            int offset = row - half;
            int symbolIndex = WrapIndex(_stripIndex + offset, strip.Length);

            // Position the renderer
            float yPos = (half - row) * symbolHeight + _scrollOffset;
            rowRenderers[row].transform.localPosition = new Vector3(0f, yPos, 0f);

            // Assign sprite
            if (_symbolSprites != null)
                rowRenderers[row].sprite = _symbolSprites[(int)strip[symbolIndex]];
        }
    }

    private int FindSymbolInStrip(SlotSymbol symbol)
    {
        for (int i = 0; i < strip.Length; i++)
            if (strip[i] == symbol) return i;
        return 0; // fallback
    }

    private static int WrapIndex(int index, int length)
    {
        return ((index % length) + length) % length;
    }
}