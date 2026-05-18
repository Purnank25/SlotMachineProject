using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Manually handles sprite-swap button states using the provided button sprite sheets.
///
/// Each button sheet (slot_machine_buttons-02/03/04.png) is 256×1024:
///   Row 0 (y 768-1024): Normal state
///   Row 1 (y 512-768):  Highlighted (hover)
///   Row 2 (y 256-512):  Pressed
///   Row 3 (y 0-256):    Disabled
///
/// HOW TO SET UP IN UNITY:
///   1. Select a button PNG in Project → Inspector:
///      - Texture Type: Sprite (2D and UI)
///      - Sprite Mode: Multiple
///      - Click "Sprite Editor" → Slice → Grid By Cell Size → 256 × 256
///      - Apply. You'll get 4 sliced sprites named e.g. "slot_machine_buttons-02_0" … "_3"
///   2. Attach this script to a Button GameObject (alongside the Button component).
///   3. Assign the 4 sliced sprites to the fields below.
///   4. Set the Button's Transition to "None" (we handle it here).
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class ButtonSpriteSwap : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    // ------------------------------------------------------------------
    // Inspector Fields
    // ------------------------------------------------------------------

    [Header("Sliced Sprites (from sprite sheet)")]
    [Tooltip("slot_machine_buttons-XX_0  — top slice = Normal")]
    [SerializeField] private Sprite normalSprite;

    [Tooltip("slot_machine_buttons-XX_1  — Highlighted / hover")]
    [SerializeField] private Sprite highlightedSprite;

    [Tooltip("slot_machine_buttons-XX_2  — Pressed")]
    [SerializeField] private Sprite pressedSprite;

    [Tooltip("slot_machine_buttons-XX_3  — Disabled")]
    [SerializeField] private Sprite disabledSprite;

    // ------------------------------------------------------------------
    // Private References
    // ------------------------------------------------------------------

    private Button _button;
    private Image _image;
    private bool _isHovered = false;

    // ------------------------------------------------------------------
    // Unity Lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        _button = GetComponent<Button>();
        _image  = GetComponent<Image>();
        _image.sprite = normalSprite;
    }

    private void Update()
    {
        // Poll interactable state each frame to react to runtime enable/disable
        if (!_button.interactable)
        {
            _image.sprite = disabledSprite;
        }
        else if (_image.sprite == disabledSprite)
        {
            _image.sprite = normalSprite; // restore when re-enabled
        }
    }

    // ------------------------------------------------------------------
    // Pointer Event Handlers
    // ------------------------------------------------------------------

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        _isHovered = true;
        _image.sprite = highlightedSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        _isHovered = false;
        _image.sprite = normalSprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        _image.sprite = pressedSprite;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        _image.sprite = _isHovered ? highlightedSprite : normalSprite;
    }
}
