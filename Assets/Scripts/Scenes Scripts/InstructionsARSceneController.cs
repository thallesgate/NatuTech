using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
//using UnityEngine.UIElements;

public class InstructionsARSceneController : MonoBehaviour
{
    [Header("UI Elements")]
    public Animator instructionsAnimator;
    public TextMeshProUGUI instructionsText;
    public Image instructionsImage;
    public Button skipButton;
    [SerializeField] private InputAction tap;

    [Header("Instruction Data")]
    public Sprite[] instructionImages;
    public string[] instructionTexts;

    [Header("Timing")]
    public float delayBeforeInput = 1f;

    [SerializeField] private GameObject nextScenePrefab;

    private int currentInstructionIndex = 0;
    private bool inputEnabled = false;

    private void Start()
    {
        tap = InputSystem.actions.FindAction("Spawn Object");

        if (instructionImages.Length == 0 || instructionTexts.Length == 0 || instructionsAnimator == null)
        {
            Debug.LogError("Missing required components or data!");
            return;
        }

        // Initialize the first instruction
        SetInstruction(0);
        PlayAnimation("InstructionsEaseIn");

        // Enable input after a delay
        Invoke(nameof(EnableInput), delayBeforeInput);

        // Assign skip button action
        skipButton.onClick.AddListener(SkipInstructions);
    }

    private void EnableInput()
    {
        inputEnabled = true;
        tap.Enable();
        tap.performed += OnClick;
    }

    private void DisableInput()
    {
        inputEnabled = false;
        tap.performed -= OnClick;
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (!inputEnabled || !context.performed) return;

        DisableInput(); // Prevent multiple taps
        PlayAnimation("InstructionsEaseOut");

        // Wait for the animation to finish easing out before proceeding
        StartCoroutine(WaitAndAdvanceInstruction());
        Debug.Log("Tap!!");
    }

    private void SetInstruction(int index)
    {
        if (index >= 0 && index < instructionImages.Length && index < instructionTexts.Length)
        {
            instructionsImage.sprite = instructionImages[index];
            instructionsText.text = instructionTexts[index];
        }
        else
        {
            Debug.LogError("Instruction index out of bounds!");
        }
    }

    private void PlayAnimation(string triggerName)
    {
        if (instructionsAnimator != null)
        {
            instructionsAnimator.SetTrigger(triggerName);
        }
    }

    private System.Collections.IEnumerator WaitAndAdvanceInstruction()
    {
        yield return new WaitForSeconds( 0.5f); // Adjust if ease-out animation duration changes

        currentInstructionIndex++;
        Debug.Log("Instructions Controller Index: " + currentInstructionIndex);
        if (currentInstructionIndex < instructionImages.Length)
        {
            // Show next instruction
            SetInstruction(currentInstructionIndex);
            PlayAnimation("InstructionsEaseIn");
            Invoke(nameof(EnableInput), delayBeforeInput);
            Debug.Log("Next Instruction.");
        }
        else
        {
            // Trigger despawn animation
            Debug.Log("Despawn.");
            PlayAnimation("InstructionsDespawn");

            //Fallback
            Invoke(nameof(OnInstructionsARAnimatorExit), 1f);
        }
    }
    // Skip button functionality
    public void SkipInstructions()
    {
        DisableInput(); // Disable input to prevent conflicts
        PlayAnimation("InstructionsDespawn");
    }
    // Called at the end of the despawn animation via Animation Event
    public void OnInstructionsARAnimatorExit()
    {
        Debug.Log("Instructions UI has despawned. Transitioning to game...");
        // Additional logic for transitioning to the game can be added here
        DisableInput();
        Instantiate(nextScenePrefab);
        Destroy(gameObject);
    }
}
