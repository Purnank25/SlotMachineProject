using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls a single reel's visual behaviour:
///   - Scrolls a strip of symbol images upward while spinning
///   - Snaps to the target symbol with a satisfying bounce
///   - Exposes StartSpin() / StopSpin() for GameManager to call
///
/// Setup:
///   1. Create a UI mask panel (the "window") for this reel.
///   2. Inside it, place a vertical LayoutGroup container (symbolContainer).
///   3. Populate symbolContainer with enough Image children (symbolHeight × 6 minimum).
///   4. Assign this component + the config + symbol pool.
/// </summary>
public class ReelController : MonoBehaviour
{
    // ------------------------------------------------------------------
    // Inspector Fields
    // ------------------------------------------------------------------

    [Header("References")]
    [Tooltip("The RectTransform that holds all symbol Image children")]
    [SerializeField] private RectTransform symbolContainer;

    [Tooltip("Prefab: a single symbol cell (Image + optional glow frame)")]
    [SerializeField] private GameObject symbolCellPrefab;

    [Header("Config")]
    [SerializeField] private GameConfig config;

    [Tooltip("Full symbol pool — must match GameManager's pool")]
    [SerializeField] private List<SlotSymbol> symbolPool;

    [Tooltip("Height of each symbol cell in pixels")]
    [SerializeField] private float symbolHeight = 150f;

    // ------------------------------------------------------------------
    // State
    // ------------------------------------------------------------------

    private List<Image> _symbolImages = new List<Image>();
    private bool _isSpinning;
    private int _targetSymbolIndex;
    private Coroutine _spinCoroutine;

    // The number of symbol cells rendered (enough to loop seamlessly)
    private const int CellCount = 20;

    // ------------------------------------------------------------------
    // Unity Lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        BuildSymbolStrip();
    }

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Begins the reel's continuous upward scroll animation.
    /// </summary>
    public void StartSpin()
    {
        if (_isSpinning) return;
        _isSpinning = true;
        _spinCoroutine = StartCoroutine(ScrollLoop());
    }

    /// <summary>
    /// Signals the reel to decelerate and land on <paramref name="targetSymbol"/>.
    /// </summary>
    public void StopSpin(SlotSymbol targetSymbol)
    {
        if (!_isSpinning) return;

        // Find the index of the target symbol in the pool
        _targetSymbolIndex = symbolPool.IndexOf(targetSymbol);
        if (_targetSymbolIndex < 0) _targetSymbolIndex = 0;

        _isSpinning = false; // ScrollLoop will detect this and exit
    }

    // ------------------------------------------------------------------
    // Animation Coroutines
    // ------------------------------------------------------------------

    /// <summary>
    /// Continuously scrolls the symbol strip upward.
    /// When _isSpinning becomes false, hands off to SnapToTarget.
    /// </summary>
    private IEnumerator ScrollLoop()
    {
        float currentY = symbolContainer.anchoredPosition.y;

        while (_isSpinning)
        {
            currentY += config.reelScrollSpeed * Time.deltaTime * symbolHeight;

            // Wrap: when we've scrolled past one full symbol height, reset
            if (currentY >= symbolHeight)
            {
                currentY -= symbolHeight;
                CycleTopSymbolToBottom();
            }

            symbolContainer.anchoredPosition = new Vector2(
                symbolContainer.anchoredPosition.x, currentY);

            yield return null;
        }

        // Reel was told to stop — snap to target
        yield return StartCoroutine(SnapToTarget());
    }

    /// <summary>
    /// Smoothly decelerates the reel and snaps to the target symbol position,
    /// with a small bounce overshoot for game-feel.
    /// </summary>
    private IEnumerator SnapToTarget()
    {
        // Place the target symbol at position 0 of the strip
        PlaceTargetAtTop(_targetSymbolIndex);

        float startY = symbolContainer.anchoredPosition.y;
        float endY = 0f; // target rests at y = 0

        // Overshoot downward then snap back (bounce effect)
        float bounceY = endY - config.snapBounceDistance * symbolHeight;

        // Phase 1: slide to overshoot
        yield return StartCoroutine(LerpPosition(startY, bounceY, 0.25f, EaseOutCubic));

        // Phase 2: snap back to final position
        yield return StartCoroutine(LerpPosition(bounceY, endY, 0.1f, EaseInCubic));
    }

    /// <summary>
    /// Generic position lerp coroutine with a pluggable easing function.
    /// </summary>
    private IEnumerator LerpPosition(float from, float to, float duration, System.Func<float, float> ease)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = ease(Mathf.Clamp01(elapsed / duration));
            float y = Mathf.Lerp(from, to, t);
            symbolContainer.anchoredPosition = new Vector2(symbolContainer.anchoredPosition.x, y);
            yield return null;
        }
        symbolContainer.anchoredPosition = new Vector2(symbolContainer.anchoredPosition.x, to);
    }

    // ------------------------------------------------------------------
    // Symbol Strip Management
    // ------------------------------------------------------------------

    /// <summary>
    /// Instantiates CellCount symbol cells, filling them with random symbols.
    /// </summary>
    private void BuildSymbolStrip()
    {
        _symbolImages.Clear();

        for (int i = 0; i < CellCount; i++)
        {
            GameObject cell = Instantiate(symbolCellPrefab, symbolContainer);
            Image img = cell.GetComponentInChildren<Image>();
            _symbolImages.Add(img);

            // Fill with a random symbol during idle
            int rndIndex = Random.Range(0, symbolPool.Count);
            img.sprite = symbolPool[rndIndex].sprite;
        }
    }

    /// <summary>
    /// Moves the topmost symbol to the bottom and assigns a new random sprite.
    /// This creates a seamless infinite scroll effect.
    /// </summary>
    private void CycleTopSymbolToBottom()
    {
        // Move first child to last position in hierarchy
        Transform first = symbolContainer.GetChild(0);
        first.SetAsLastSibling();

        // Assign a random sprite to the recycled cell
        Image img = first.GetComponentInChildren<Image>();
        int rndIndex = Random.Range(0, symbolPool.Count);
        img.sprite = symbolPool[rndIndex].sprite;
    }

    /// <summary>
    /// Rearranges the strip so the target symbol appears at the top cell (index 0),
    /// and resets the container's Y to 0, ready for the snap animation.
    /// </summary>
    private void PlaceTargetAtTop(int targetIndex)
    {
        // Set top cell to target symbol
        _symbolImages[0].sprite = symbolPool[targetIndex].sprite;

        // Fill remaining cells with random symbols for visual variety
        for (int i = 1; i < _symbolImages.Count; i++)
        {
            int rndIndex = Random.Range(0, symbolPool.Count);
            _symbolImages[i].sprite = symbolPool[rndIndex].sprite;
        }

        // Reset container position so the snap lerp starts correctly
        symbolContainer.anchoredPosition = new Vector2(
            symbolContainer.anchoredPosition.x, symbolHeight * 2f);
    }

    // ------------------------------------------------------------------
    // Easing Functions
    // ------------------------------------------------------------------

    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    private static float EaseInCubic(float t) => t * t * t;
}
