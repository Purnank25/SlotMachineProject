using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animates the slot machine body graphic by cycling through the 5 provided
/// machine frame sprites (slot-machine1.png … slot-machine5.png) while spinning.
///
/// Attach to the GameObject that holds the main machine body Image.
///
/// HOW TO SET UP:
///   1. Import slot-machine1..5.png as Sprites (Texture Type: Sprite 2D and UI).
///   2. Drag this component onto the "SlotMachineBody" Image GameObject.
///   3. Assign the Image reference and drag all 5 sprites into the
///      machineFrames array IN ORDER (1→2→3→4→5).
///   4. GameManager calls SetSpinning(true) on spin start and SetSpinning(false) on stop.
/// </summary>
public class ReelAnimator : MonoBehaviour
{
    // ------------------------------------------------------------------
    // Inspector Fields
    // ------------------------------------------------------------------

    [Header("Machine Body Image")]
    [Tooltip("The Image component showing the slot machine body graphic")]
    [SerializeField] private Image machineBodyImage;

    [Header("Frame Sprites")]
    [Tooltip("Drag slot-machine1 through slot-machine5 here IN ORDER")]
    [SerializeField] private Sprite[] machineFrames; // 5 frames

    [Header("Animation")]
    [Tooltip("Frames per second while spinning")]
    [SerializeField] private float fps = 12f;

    // ------------------------------------------------------------------
    // Private State
    // ------------------------------------------------------------------

    private int _currentFrame = 0;
    private bool _isAnimating = false;
    private Coroutine _animCoroutine;

    // ------------------------------------------------------------------
    // Unity Lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        // Show idle frame (frame 0 = slot-machine1)
        if (machineFrames != null && machineFrames.Length > 0)
            machineBodyImage.sprite = machineFrames[0];
    }

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Start or stop the reel body frame animation.
    /// Called by GameManager on spin start/end.
    /// </summary>
    public void SetSpinning(bool spinning)
    {
        if (spinning && !_isAnimating)
        {
            _isAnimating = true;
            _animCoroutine = StartCoroutine(AnimateFrames());
        }
        else if (!spinning && _isAnimating)
        {
            _isAnimating = false;
            if (_animCoroutine != null)
                StopCoroutine(_animCoroutine);

            // Return to idle frame
            machineBodyImage.sprite = machineFrames[0];
            _currentFrame = 0;
        }
    }

    // ------------------------------------------------------------------
    // Frame Animation Loop
    // ------------------------------------------------------------------

    private IEnumerator AnimateFrames()
    {
        float frameDuration = 1f / fps;

        while (_isAnimating)
        {
            // Advance frame (loop 1→2→3→4→5→1→…, skip frame 0 = idle)
            _currentFrame = (_currentFrame % (machineFrames.Length - 1)) + 1;
            machineBodyImage.sprite = machineFrames[_currentFrame];
            yield return new WaitForSeconds(frameDuration);
        }
    }
}
