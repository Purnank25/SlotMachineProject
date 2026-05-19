// SlotMachineBackground.cs
// Attach to the "SlotMachineBackground" GameObject.
//
// SPRITE LAYERING (back to front):
//
//   Layer 0  →  slot-machine4.png  (machine body, black transparent windows)
//   Layer 1  →  Reel sprites scroll here  (symbols visible through black holes)
//   Layer 2  →  slot-machine5.png  (reel window frames / glass overlay)
//   Layer 3  →  Lever
//
// The black areas in slot-machine4 are TRANSPARENT in the PNG —
// reels show through them. slot-machine5 sits ON TOP of the reels
// as a decorative glass frame overlay.

using UnityEngine;

public class SlotMachineBackground : MonoBehaviour
{
    [Header("Body Sprite  (slot-machine4 — black window cutouts)")]
    public SpriteRenderer bodyRenderer;         // Order In Layer: 0

    [Header("Window Frame Sprite  (slot-machine5 — reel frame overlay)")]
    public SpriteRenderer windowFrameRenderer;  // Order In Layer: 2

    [Header("Objects to Align")]
    public Transform reel0;
    public Transform reel1;
    public Transform reel2;
    public Transform lever;

    [Header("Local Position Offsets  (relative to SlotMachine root)")]
    [Tooltip("Pre-calculated for slot-machine4.png at PPU=100, scale=1.\n" +
             "Right-click → 'Align Objects To Background' to apply.")]
    public Vector3 reel0LocalPos = new Vector3(-1.52f, 0.25f, 0f);
    public Vector3 reel1LocalPos = new Vector3(0.00f, 0.25f, 0f);
    public Vector3 reel2LocalPos = new Vector3(1.52f, 0.25f, 0f);
    public Vector3 leverLocalPos = new Vector3(2.60f, -0.15f, 0f);
    public Vector3 windowFrameLocalPos = new Vector3(0.00f, 0.25f, 0f);

    [Header("Reel Window Size (world units)  —  used for SpriteMask scale")]
    [Tooltip("Size of each black cutout window in slot-machine4.png at PPU=100.")]
    public Vector2 reelWindowSize = new Vector2(1.05f, 1.75f);

    // =========================================================
    private void Awake()
    {
        // Enforce correct sort orders at runtime
        if (bodyRenderer != null) bodyRenderer.sortingOrder = 3;
        if (windowFrameRenderer != null) windowFrameRenderer.sortingOrder = 0;
    }

#if UNITY_EDITOR
    [ContextMenu("Align Objects To Background")]
    private void AlignObjects()
    {
        if (reel0 != null) reel0.localPosition = reel0LocalPos;
        if (reel1 != null) reel1.localPosition = reel1LocalPos;
        if (reel2 != null) reel2.localPosition = reel2LocalPos;
        if (lever != null) lever.localPosition = leverLocalPos;

        if (windowFrameRenderer != null)
            windowFrameRenderer.transform.localPosition = windowFrameLocalPos;

        Debug.Log("[SlotMachineBackground] Objects aligned.");
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }
#endif
}