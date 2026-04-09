using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for clearing the selected state

[RequireComponent(typeof(Button), typeof(Image))]
public class AudioToggleButton : MonoBehaviour
{
    [Header("Audio ON Sprites")]
    public Sprite audioOnSprite;
    public Sprite audioOnHoverSprite;

    [Header("Audio OFF Sprites")]
    public Sprite audioOffSprite;
    public Sprite audioOffHoverSprite;

    private Button button;
    private Image buttonImage;

    void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        
        // Ensure the button is set up for Sprite Swap
        button.transition = Selectable.Transition.SpriteSwap;
    }

    void OnEnable()
    {
        // Ensure references are initialized (Awake might not have been called yet)
        if (button == null) button = GetComponent<Button>();
        if (buttonImage == null) buttonImage = GetComponent<Image>();
        
        // Subscribe to audio toggle events
        GameEvents.OnAudioToggled += OnAudioStateChanged;
        
        // Initialize visuals based on current audio state
        UpdateVisuals();
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        GameEvents.OnAudioToggled -= OnAudioStateChanged;
    }

    // Call this function from your Button's OnClick event
    public void OnToggleAudioPressed()
    {
        // Toggle the audio state in AudioManager
        AudioManager.Instance.ToggleAllAudio(!AudioManager.Instance._audioEnabled);

        // Fix the "Stuck on Hover" issue by deselecting the button
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnAudioStateChanged(bool isAudioOn)
    {
        // Called whenever any button toggles audio - ensures all buttons sync
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // Guard against null references during early initialization
        if (button == null || buttonImage == null || AudioManager.Instance == null)
            return;
        
        bool isAudioOn = AudioManager.Instance._audioEnabled;
        
        // We have to create a copy of the SpriteState, modify it, and assign it back
        SpriteState state = button.spriteState;

        if (isAudioOn)
        {
            buttonImage.sprite = audioOnSprite; // The base visual
            state.highlightedSprite = audioOnHoverSprite; // The hover visual
        }
        else
        {
            buttonImage.sprite = audioOffSprite; // The base visual
            state.highlightedSprite = audioOffHoverSprite; // The hover visual
        }

        // Apply the modified state back to the button
        button.spriteState = state;
    }
}