// LeverController.cs
// Attach to the "Lever" GameObject (which has a SpriteRenderer).
// Assign the two lever sprites (up / down) in the Inspector.
// Has a BoxCollider2D for click detection — add one via Add Component.
//
// HOW IT WORKS:
//  1. Player clicks the lever sprite.
//  2. Lever animates DOWN  (sprite swap + smooth move).
//  3. SlotMachineManager.PlaceBet() is called with the currently selected bet.
//  4. Lever animates back UP after a short delay.

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class LeverController : MonoBehaviour
{
    // =========================================================
    //  Inspector Fields
    // =========================================================

    [Header("Sprites")]
    [Tooltip("Lever in the resting UP position (Image 1)")]
    public Sprite leverUpSprite;

    [Tooltip("Lever fully pulled DOWN position (Image 2)")]
    public Sprite leverDownSprite;

    [Header("References")]
    [Tooltip("Drag the SlotMachineManager GameObject here")]
    public SlotMachineManager slotManager;

    [Header("Bet Amount")]
    [Tooltip("How much gold to bet when lever is pulled.\n" +
             "Change this at runtime via SetBet() or from SlotUIController.")]
    public int betAmount = 10;

    [Header("Animation")]
    [Tooltip("How far down the lever moves in world units when pulled")]
    public float pullDistance = 0.4f;

    [Tooltip("How fast the lever moves to DOWN position (seconds)")]
    public float pullDuration = 0.12f;

    [Tooltip("How long the lever stays DOWN before returning UP (seconds)")]
    public float holdDuration = 0.15f;

    [Tooltip("How fast the lever returns to UP position (seconds)")]
    public float returnDuration = 0.2f;

    // =========================================================
    //  Private
    // =========================================================

    private SpriteRenderer _sr;
    private Vector3 _restPosition;       // original local position
    private bool _isAnimating = false;

    // =========================================================
    //  Unity Lifecycle
    // =========================================================

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _restPosition = transform.localPosition;

        // Start with lever up
        SetLeverUp();
    }

    private void OnMouseDown()
    {
        TryPullLever();
    }

    // =========================================================
    //  Public API
    // =========================================================

    /// <summary>
    /// Called by SlotUIController when the player selects a different bet.
    /// </summary>
    public void SetBet(int amount)
    {
        betAmount = amount;
    }

    /// <summary>
    /// Pulls the lever programmatically (e.g. from a keyboard shortcut).
    /// </summary>
    public void TryPullLever()
    {
        if (_isAnimating) return;
        if (slotManager == null) return;
        if (slotManager.State != SlotMachineManager.GameState.Idle) return;

        StartCoroutine(PullSequence());
    }

    // =========================================================
    //  Animation Coroutine
    // =========================================================

    private IEnumerator PullSequence()
    {
        _isAnimating = true;

        // ── Phase 1: Move lever DOWN ──────────────────────────
        Vector3 downPosition = _restPosition + Vector3.down * pullDistance;
        yield return MoveLocal(_restPosition, downPosition, pullDuration);
        SetLeverDown();

        // ── Phase 2: Trigger the spin ─────────────────────────
        slotManager.PlaceBet(betAmount);

        // ── Phase 3: Hold at bottom ───────────────────────────
        yield return new WaitForSeconds(holdDuration);

        // ── Phase 4: Move lever back UP ───────────────────────
        yield return MoveLocal(downPosition, _restPosition, returnDuration);
        SetLeverUp();

        _isAnimating = false;
    }

    // Smooth lerp between two local positions
    private IEnumerator MoveLocal(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }
        transform.localPosition = to;
    }

    // =========================================================
    //  Sprite Helpers
    // =========================================================

    private void SetLeverUp()
    {
        if (leverUpSprite != null)
            _sr.sprite = leverUpSprite;
    }

    private void SetLeverDown()
    {
        if (leverDownSprite != null)
            _sr.sprite = leverDownSprite;
    }
}