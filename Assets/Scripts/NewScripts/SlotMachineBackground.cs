// SlotMachineBackground.cs
// Attach to the "SlotMachineBackground" SpriteRenderer GameObject.
// Handles:
//   1. Rendering the background sprite behind everything.
//   2. (Editor helper) Auto-positions Reel0/1/2 and the Lever
//      to align with the three reel windows in the background art.
//
// After hitting "Align Objects To Background" in the Inspector
// (via the context menu), fine-tune positions manually if needed.

using UnityEngine;

public class SlotMachineBackground : MonoBehaviour
{
    [Header("Background Sprite")]
    [Tooltip("Assign slot-machine1 sprite here — same as the SpriteRenderer on this GameObject")]
    public Sprite backgroundSprite;

    [Header("Objects to Align")]
    public Transform reel0;
    public Transform reel1;
    public Transform reel2;
    public Transform lever;

    [Header("Reel Window Offsets (local, relative to this GameObject)")]
    [Tooltip("These values are pre-calculated for the provided slot-machine1.png at PPU=100.\n" +
             "Adjust if your PPU or scale differs.")]
    public Vector3 reel0LocalPos = new Vector3(-1.55f, 0.30f, -0.1f);
    public Vector3 reel1LocalPos = new Vector3(0.00f, 0.30f, -0.1f);
    public Vector3 reel2LocalPos = new Vector3(1.55f, 0.30f, -0.1f);
    public Vector3 leverLocalPos = new Vector3(2.55f, -0.20f, -0.1f);

    [Header("Reel Mask Size (world units)")]
    [Tooltip("Width and height of the visible reel window — used to size SpriteMasks.\n" +
             "Matches the 3 blue windows in the background art at PPU=100.")]
    public Vector2 reelWindowSize = new Vector2(1.10f, 1.80f);

    // =========================================================
    //  Called automatically in Awake — applies sprite to renderer
    // =========================================================
    private void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && backgroundSprite != null)
            sr.sprite = backgroundSprite;
    }

    // =========================================================
    //  Context Menu helper — run this once in the Editor
    //  to snap reels and lever into correct positions.
    //  Right-click the component header → "Align Objects To Background"
    // =========================================================
#if UNITY_EDITOR
    [ContextMenu("Align Objects To Background")]
    private void AlignObjects()
    {
        if (reel0 != null) reel0.localPosition = reel0LocalPos;
        if (reel1 != null) reel1.localPosition = reel1LocalPos;
        if (reel2 != null) reel2.localPosition = reel2LocalPos;
        if (lever != null) lever.localPosition = leverLocalPos;

        Debug.Log("[SlotMachineBackground] Objects aligned to background windows.");
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }
#endif
}